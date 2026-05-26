# DocxDiffTool

上传两个文件，逐段对比文本差异。支持段落级和词级高亮。

## 为什么要做这个工具呢
写一篇新闻稿时，审核发来一版修改后的成品稿让我看下，但里边没有显式标注改动之处，逐字对比太费时。这让我想起我们宣传团队内类似的问题：做推送的人员正按一版文案制作，审核方又出了修改意见，双方容易因看不清具体改动而混入旧版表述。虽然团队曾要求使用审阅模式，但外部参与者难以约束，内部成员也常遇到不顺手、社交媒体无法显示、复制后格式异常等问题。今天 d 老师直接用 py 程序输出比对结果，让我想到可以顺着这个思路，vibe 一个带图形界面的小工具 —— 于是就有了本项目

## 功能

- 双栏对比视图 + 合并对比视图，同步滚动
- 段落级差异匹配 + 词级行内高亮（增/删/改）
- 拖拽或点击上传文件
- 所有数据在本地处理，无网络传输
- 支持格式：`.docx` / `.doc` / `.pdf` / `.txt` / `.md`

## 仓库结构

```
DocxDiffTool/
├── python-app/              # Python Flask 版（已冻结）
│   ├── app.py               # Flask 后端 + docx 解析 + diff
│   ├── requirements.txt     # pip 依赖
│   ├── build.bat            # PyInstaller 打包脚本
│   └── static/              # 前端（HTML/CSS/JS）
│
├── dotnet-app/              # .NET 桌面版（活跃开发）
│   ├── Program.cs           # 入口：WinForms + WebView2 + ASP.NET Core
│   ├── DiffService.cs       # 多格式解析 + diff 算法
│   ├── SequenceMatcher.cs   # difflib.SequenceMatcher C# 移植
│   ├── DocxDiffTool.csproj  # 项目文件 + NuGet 依赖
│   └── wwwroot/             # 前端（HTML/CSS/JS）
│
└── .gitignore
```

## 两个版本对比

| | Python Flask 版 | .NET 桌面版 |
|---|---|---|
| 窗口 | 浏览器 | 独立桌面窗口（WebView2） |
| 格式支持 | 仅 .docx | .docx / .doc / .pdf / .txt / .md |
| 启动 | `python app.py` | 双击 exe 或 `dotnet run` |
| 运行时 | Python 3 + Flask | .NET 8（自包含 exe 无需安装） |

## 许可证

本项目以 [CC BY-NC 4.0](https://creativecommons.org/licenses/by-nc/4.0/deed.zh) 协议分发。

- **非商业使用**：可自由使用、修改、分发，无需另行授权，但须保留署名。
- **商业使用**：需另行获得作者授权。如有商用需求，请联系 wangyongsheng23@mails.ucas.ac.cn。

完整协议文本见 [LICENSE](LICENSE) 文件。

