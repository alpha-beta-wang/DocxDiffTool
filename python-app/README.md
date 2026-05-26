# DocxDiffTool — Python Flask 版

## 技术栈

| 层 | 技术 |
|---|---|
| 后端 | Python 3 + Flask |
| docx 解析 | stdlib `zipfile` + `xml.etree` |
| diff 引擎 | stdlib `difflib.SequenceMatcher` |
| 前端 | 原生 HTML/CSS/JS |

## 运行

```bash
pip install flask
python app.py
```

浏览器打开 `http://localhost:5000`。

## 打包为 EXE

```bash
pip install pyinstaller flask
pyinstaller --onefile --add-data "static;static" --name DocxDiffTool app.py
```

输出在 `dist/DocxDiffTool.exe`。

## API

**POST `/compare`**

接收两个 .docx 文件（multipart/form-data，字段名 `file1`、`file2`），返回 JSON：

```json
{
  "name1": "v2.docx",
  "name2": "v3.docx",
  "diffs": [
    { "type": "equal",   "old": "相同段落", "new": "相同段落" },
    { "type": "delete",  "old": "被删除段落" },
    { "type": "insert",  "new": "新增段落" },
    { "type": "replace", "old": "原文", "new": "修改后",
      "old_spans": [{"text": "原", "type": "equal"}, {"text": "删", "type": "delete"}],
      "new_spans": [{"text": "修", "type": "equal"}, {"text": "增", "type": "insert"}] }
  ],
  "stats": { "equal": 10, "deleted": 2, "inserted": 3, "replaced": 1 }
}
```

## 局限

- 仅支持 .docx 格式
- 需 Python 运行环境
- 浏览器窗口中运行，非独立桌面窗口
