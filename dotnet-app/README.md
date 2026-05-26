# DocxDiffTool — .NET 桌面版

独立桌面窗口应用，使用 WinForms + WebView2 + ASP.NET Core。

## 技术栈

| 层 | 技术 |
|---|---|
| 框架 | .NET 8 (Windows) |
| 窗口 | WinForms |
| 渲染 | WebView2（Edge 内核） |
| 后端 | ASP.NET Core Minimal API |
| docx 解析 | `System.IO.Compression.ZipArchive` + `System.Xml.Linq` |
| doc 解析 | NPOI 2.5.6 + ScratchPad.NPOI.HWPF |
| pdf 解析 | PdfPig 0.1.14 |
| diff 引擎 | 自实现 `SequenceMatcher<T>`（difflib 移植） |
| 前端 | 原生 HTML/CSS/JS |

## 开发运行

```bash
# 安装 .NET 8 SDK 后
cd dotnet-app
dotnet run
```

## 发布为 EXE

自动输出自包含单文件 exe，无需安装 .NET 运行时：

```bash
cd dotnet-app
dotnet publish -c Release -o dist
```

输出 `dist/DocxDiffTool.exe`（约 200MB），拷贝到任何 Windows x64 电脑双击即可运行。

## 架构

```
DocxDiffTool.exe
├── WebView2 窗口（前端 UI）
│   └── 通过 fetch 调用后端 API
└── ASP.NET Core（后台线程，127.0.0.1:5000）
    └── POST /api/compare
        ├── magic bytes 格式检测
        ├── 对应解析器提取段落文本
        ├── SequenceMatcher 段落级 diff
        ├── 段落内词级 WordDiff
        └── 返回 JSON（diffs + stats）
```

## 格式检测

通过文件头 magic bytes 自动识别格式，不需要依赖扩展名：

| 格式 | Magic Bytes | 解析器 |
|---|---|---|
| .docx | `PK\x03\x04` | `ExtractTextFromDocx` (ZipArchive) |
| .doc | `\xD0\xCF\x11\xE0` | `ExtractTextFromDoc` (NPOI HWPF) |
| .pdf | `%PDF` | `ExtractTextFromPdf` (PdfPig) |
| .txt / .md | （无） | `ExtractTextFromPlainText` (StreamReader) |

## 隐私

所有文件解析、文本比对均在本地内存中完成，无网络通信，不产生缓存文件，退出时清理 WebView2 临时目录。
