# DocxDiffTool

上传两个文件，逐段对比文本差异。支持段落级和词级高亮。

## 为什么要做这个小工具呢
起因是写一篇新闻稿，老师给我发了一版 ta 改过的版本，让我看一下。但发来的是一篇格式规整的成品稿子，并没有特别标注相对我的原版改了哪些地方。一处一处对比查看无疑是一个费时费力的笨办法
然后就想起在我们的团队中，也有这种快速比对文件差异的需求：做推送的同学对着一版文案开始做的时候，文案审核的人员又给出了新的修改意见，那推送也要同步更改。有时因为看不全具体改了什么地方，导致推送可能包含一些老版本的文案表述。要解决这个问题优化工作流固然重要，但也需要有好用的文案比对工具
之前我们团队一直要求文案的修改需要使用审阅模式，但只有团队内的成员会遵守，但对外部参与审核修改的人员是没有也无力约束的。加上团队内很多成员对审阅模式用的一直不是很顺、社交媒体内直接打开审阅模式文章可能无法显示、有些平台复制审阅模式下文段会异常等乱七八糟的问题，有一个好用、易用的小工具还是有必要的
今天 d 老师解决文案比对的思路是写了一段 py 程序直接输出比对结果，就想着能不能顺着这个思路 vibe 一个小工具并且支持图形化界面，就有个这个项目

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

