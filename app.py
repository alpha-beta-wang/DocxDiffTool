#!/usr/bin/env python3
"""DocxDiffTool — compare two .docx files with side-by-side diff."""

import io
import os
import shutil
import subprocess
import sys
import threading
import time
import xml.etree.ElementTree as ET
import zipfile
from difflib import SequenceMatcher

from flask import Flask, jsonify, request, send_from_directory

app = Flask(__name__, static_folder="static", static_url_path="")

NS = "{http://schemas.openxmlformats.org/wordprocessingml/2006/main}"


def extract_text(file_bytes):
    """Extract plain text paragraphs from a .docx file (stdlib only)."""
    with zipfile.ZipFile(io.BytesIO(file_bytes)) as z:
        xml_bytes = z.read("word/document.xml")
    root = ET.fromstring(xml_bytes)
    paragraphs = []
    for p in root.iter(f"{NS}p"):
        texts = []
        for t in p.iter(f"{NS}t"):
            if t.text:
                texts.append(t.text)
        line = "".join(texts)
        if line.strip():
            paragraphs.append(line)
    return paragraphs


def build_diffs(paras1, paras2):
    """Compare two paragraph lists and return structured diff list."""
    m = SequenceMatcher(None, paras1, paras2)
    diffs = []

    raw = m.get_opcodes()

    i = 0
    while i < len(raw):
        tag, i1, i2, j1, j2 = raw[i]
        if tag == "equal":
            for p in paras1[i1:i2]:
                diffs.append({"type": "equal", "old": p, "new": p})
        elif tag == "delete":
            if i + 1 < len(raw) and raw[i + 1][0] == "insert":
                _, _ni1, _ni2, _nj1, _nj2 = raw[i + 1]
                old_paras = paras1[i1:i2]
                new_paras = paras2[j1:j2]
                for op in _align_paragraphs(old_paras, new_paras):
                    diffs.append(op)
                i += 1
            else:
                for p in paras1[i1:i2]:
                    diffs.append({"type": "delete", "old": p, "new": None})
        elif tag == "insert":
            for p in paras2[j1:j2]:
                diffs.append({"type": "insert", "old": None, "new": p})
        elif tag == "replace":
            old_paras = paras1[i1:i2]
            new_paras = paras2[j1:j2]
            for op in _align_paragraphs(old_paras, new_paras):
                diffs.append(op)
        i += 1

    return diffs


def _word_diff(old, new):
    """Character-level diff, returns (old_spans, new_spans) for inline highlight."""
    m = SequenceMatcher(None, old, new)
    old_spans = []
    new_spans = []
    for tag, i1, i2, j1, j2 in m.get_opcodes():
        if tag == "equal":
            t = old[i1:i2]
            old_spans.append({"text": t, "type": "equal"})
            new_spans.append({"text": t, "type": "equal"})
        elif tag == "delete":
            old_spans.append({"text": old[i1:i2], "type": "delete"})
        elif tag == "insert":
            new_spans.append({"text": new[j1:j2], "type": "insert"})
        elif tag == "replace":
            old_spans.append({"text": old[i1:i2], "type": "delete"})
            new_spans.append({"text": new[j1:j2], "type": "insert"})
    return old_spans, new_spans


def _align_paragraphs(old_paras, new_paras):
    """Align old and new paragraphs best-effort, producing replace/delete/insert ops."""
    old_texts = old_paras[:]
    new_texts = new_paras[:]
    used_new = set()
    result = []

    for op in old_texts:
        best = None
        best_score = 0
        best_idx = -1
        for j, np in enumerate(new_texts):
            if j in used_new:
                continue
            score = SequenceMatcher(None, op, np).ratio()
            if score > best_score:
                best_score = score
                best = np
                best_idx = j
        if best is not None and best_score > 0.35:
            used_new.add(best_idx)
            old_s, new_s = _word_diff(op, best)
            result.append({
                "type": "replace", "old": op, "new": best,
                "old_spans": old_s, "new_spans": new_s,
            })
        else:
            result.append({"type": "delete", "old": op, "new": None})

    for j, np in enumerate(new_texts):
        if j not in used_new:
            result.append({"type": "insert", "old": None, "new": np})

    return result


@app.route("/")
def index():
    return send_from_directory("static", "index.html")


@app.route("/compare", methods=["POST"])
def compare():
    f1 = request.files.get("file1")
    f2 = request.files.get("file2")
    if not f1 or not f2:
        return jsonify({"error": "请上传两个 .docx 文件"}), 400

    name1 = f1.filename
    name2 = f2.filename

    try:
        paras1 = extract_text(f1.read())
        paras2 = extract_text(f2.read())
    except Exception as e:
        return jsonify({"error": f"文件解析失败: {str(e)}"}), 400

    diffs = build_diffs(paras1, paras2)
    text1 = "\n".join(paras1)
    text2 = "\n".join(paras2)

    return jsonify({
        "name1": name1,
        "name2": name2,
        "text1": text1,
        "text2": text2,
        "diffs": diffs,
        "stats": {
            "equal": sum(1 for d in diffs if d["type"] == "equal"),
            "deleted": sum(1 for d in diffs if d["type"] == "delete"),
            "inserted": sum(1 for d in diffs if d["type"] == "insert"),
            "replaced": sum(1 for d in diffs if d["type"] == "replace"),
        },
    })


def _find_edge():
    """Locate msedge.exe on Windows."""
    paths = [
        r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
        r"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
    ]
    for p in paths:
        if os.path.exists(p):
            return p
    found = shutil.which("msedge")
    if found:
        return found
    return None


def main():
    host = "127.0.0.1"
    port = 5000

    edge = _find_edge()
    if not edge:
        print("未找到 Microsoft Edge，请确认已安装。")
        sys.exit(1)

    # Start Flask in daemon thread
    t = threading.Thread(
        target=lambda: app.run(host=host, port=port, debug=False, use_reloader=False),
        daemon=True,
    )
    t.start()

    # Wait for Flask to be ready
    time.sleep(1)

    # Launch Edge in app mode (standalone window, no browser chrome)
    proc = subprocess.Popen(
        [edge, f"--app=http://{host}:{port}", "--window-size=1400,900"],
        stdout=subprocess.DEVNULL,
        stderr=subprocess.DEVNULL,
    )

    # Block until user closes the Edge window
    proc.wait()
    os._exit(0)


if __name__ == "__main__":
    main()
