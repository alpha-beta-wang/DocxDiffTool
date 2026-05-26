// --- File upload state ---
let file1 = null, file2 = null;

// --- Setup drop zones ---
function setupDropZone(id, inputId, nameId, fileSlot) {
  const zone = document.getElementById(id);
  const input = document.getElementById(inputId);

  zone.addEventListener("click", () => input.click());

  zone.addEventListener("dragover", (e) => {
    e.preventDefault();
    zone.classList.add("dragover");
  });
  zone.addEventListener("dragleave", () => zone.classList.remove("dragover"));
  zone.addEventListener("drop", (e) => {
    e.preventDefault();
    zone.classList.remove("dragover");
    const files = e.dataTransfer.files;
    if (files.length > 0 && files[0].name.endsWith(".docx")) {
      fileSlot(files[0]);
      document.getElementById(nameId).textContent = files[0].name;
      zone.classList.add("loaded");
    }
  });

  input.addEventListener("change", () => {
    if (input.files.length > 0) {
      fileSlot(input.files[0]);
      document.getElementById(nameId).textContent = input.files[0].name;
      zone.classList.add("loaded");
    }
  });
}

setupDropZone("drop1", "fileInput1", "name1", (f) => { file1 = f; });
setupDropZone("drop2", "fileInput2", "name2", (f) => { file2 = f; });

// --- Enable compare button when both files selected ---
const observer = new MutationObserver(checkReady);
observer.observe(document.getElementById("name1"), { childList: true, characterData: true, subtree: true });
observer.observe(document.getElementById("name2"), { childList: true, characterData: true, subtree: true });

function checkReady() {
  const ready = !!(file1 && file2);
  document.getElementById("compareBtn").disabled = !ready;
}
// Also check on drop zone load
["drop1", "drop2"].forEach((id) => {
  const zone = document.getElementById(id);
  const origDrop = zone.ondrop;
  zone.addEventListener("drop", () => setTimeout(checkReady, 50));
});

// --- Compare button ---
document.getElementById("compareBtn").addEventListener("click", () => {
  if (!file1 || !file2) return;
  document.getElementById("loading").classList.add("visible");
  document.getElementById("statsBar").style.display = "none";
  document.getElementById("diffContainer").style.display = "none";
  document.getElementById("unifiedContainer").style.display = "none";

  const form = new FormData();
  form.append("file1", file1);
  form.append("file2", file2);

  fetch("/compare", { method: "POST", body: form })
    .then((r) => r.json())
    .then((data) => {
      document.getElementById("loading").classList.remove("visible");
      if (data.error) { alert(data.error); return; }
      render(data);
    })
    .catch((err) => {
      document.getElementById("loading").classList.remove("visible");
      alert("请求失败: " + err.message);
    });
});

// --- Render diff results ---
let currentView = "side"; // "side" | "unified"

function render(data) {
  const stats = data.stats;
  document.getElementById("statEqual").textContent = stats.equal;
  document.getElementById("statReplaced").textContent = stats.replaced;
  document.getElementById("statDeleted").textContent = stats.deleted;
  document.getElementById("statInserted").textContent = stats.inserted;
  document.getElementById("labelOld").textContent = data.name1;
  document.getElementById("labelNew").textContent = data.name2;

  const paneOld = document.getElementById("paneOldContent");
  const paneNew = document.getElementById("paneNewContent");
  const unified = document.getElementById("unifiedContent");
  paneOld.innerHTML = "";
  paneNew.innerHTML = "";
  unified.innerHTML = "";

  data.diffs.forEach((d) => {
    const left = makeLine(d, "old");
    const right = makeLine(d, "new");
    paneOld.appendChild(left);
    paneNew.appendChild(right);

    const uline = makeUnifiedLine(d);
    unified.appendChild(uline);
  });

  currentView = "side";
  document.getElementById("statsBar").style.display = "flex";
  document.getElementById("diffContainer").style.display = "flex";
  document.getElementById("unifiedContainer").classList.remove("visible");
  document.getElementById("unifiedContainer").style.display = "none";
  document.getElementById("toggleView").textContent = "合并视图";

  // Sync scroll
  syncScroll(paneOld, paneNew);
}

function makeLine(d, side) {
  const div = document.createElement("div");
  div.className = `diff-line ${d.type}`;

  if (d.type === "equal") {
    setContent(div, "", side === "old" ? d.old : d.new);
  } else if (d.type === "delete") {
    setContent(div, "−", side === "old" ? d.old : "");
    if (side === "new") div.classList.add("empty");
  } else if (d.type === "insert") {
    setContent(div, "+", side === "new" ? d.new : "");
    if (side === "old") div.classList.add("empty");
  } else if (d.type === "replace") {
    const spans = side === "old" ? d.old_spans : d.new_spans;
    if (spans) {
      renderInlineSpans(div, spans, side);
    } else {
      setContent(div, "~", side === "old" ? d.old : d.new);
    }
  }

  return div;
}

function setContent(div, prefix, text) {
  if (!text) {
    div.classList.add("placeholder");
    div.textContent = " ";
    return;
  }
  const span = document.createElement("span");
  span.className = "prefix";
  span.textContent = prefix;
  div.appendChild(span);
  div.appendChild(document.createTextNode(text));
}

function makeUnifiedLine(d) {
  const div = document.createElement("div");
  div.className = `diff-line ${d.type}`;
  if (d.type === "equal") {
    setContent(div, "", d.old);
  } else if (d.type === "delete") {
    setContent(div, "−", d.old);
  } else if (d.type === "insert") {
    setContent(div, "+", d.new);
  } else if (d.type === "replace") {
    if (d.old_spans) {
      const frag = document.createDocumentFragment();
      const oldDiv = document.createElement("div");
      oldDiv.className = `diff-line ${d.type}`;
      renderInlineSpans(oldDiv, d.old_spans, "old");
      frag.appendChild(oldDiv);
      const newDiv = document.createElement("div");
      newDiv.className = `diff-line ${d.type}`;
      renderInlineSpans(newDiv, d.new_spans, "new");
      frag.appendChild(newDiv);
      return frag;
    } else {
      setContent(div, "−", d.old);
      const div2 = document.createElement("div");
      div2.className = `diff-line ${d.type}`;
      setContent(div2, "+", d.new);
      div.after(div2);
    }
  }
  return div;
}

function renderInlineSpans(container, spans, side) {
  spans.forEach((s) => {
    if (s.type === "equal") {
      container.appendChild(document.createTextNode(s.text));
    } else if (s.type === "delete") {
      const span = document.createElement("span");
      span.className = "diff-del";
      span.textContent = s.text;
      container.appendChild(span);
    } else if (s.type === "insert") {
      const span = document.createElement("span");
      span.className = "diff-ins";
      span.textContent = s.text;
      container.appendChild(span);
    }
  });
}

function syncScroll(paneA, paneB) {
  let syncing = false;
  paneA.addEventListener("scroll", () => {
    if (syncing) return;
    syncing = true;
    paneB.scrollTop = paneA.scrollTop;
    syncing = false;
  });
  paneB.addEventListener("scroll", () => {
    if (syncing) return;
    syncing = true;
    paneA.scrollTop = paneB.scrollTop;
    syncing = false;
  });
}

// --- Toggle view ---
document.getElementById("toggleView").addEventListener("click", () => {
  const side = document.getElementById("diffContainer");
  const uni = document.getElementById("unifiedContainer");
  const btn = document.getElementById("toggleView");

  if (currentView === "side") {
    side.style.display = "none";
    uni.style.display = "block";
    uni.classList.add("visible");
    btn.textContent = "分栏视图";
    currentView = "unified";
  } else {
    side.style.display = "flex";
    uni.style.display = "none";
    uni.classList.remove("visible");
    btn.textContent = "合并视图";
    currentView = "side";
  }
});
