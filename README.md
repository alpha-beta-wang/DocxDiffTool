# DocxDiffTool

上传两个文件，逐段对比文本差异。支持段落级和词级高亮。

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
| 状态 | 已冻结 | 活跃开发 |

## 许可证

本项目以 [CC BY-NC 4.0](https://creativecommons.org/licenses/by-nc/4.0/deed.zh) 协议开源分发。

- **非商业使用**：可自由使用、修改、分发，无需另行授权，但须保留署名。
- **商业使用**：需另行获得作者授权。如有商用需求，请联系 wangyongsheng23@mails.ucas.ac.cn。

完整协议文本见 [LICENSE](LICENSE) 文件。

本工具由 2025-2026 届宣传委员 wys（wangyongsheng23@mails.ucas.ac.cn）制作，供文案组与推送组进行文案版本比对。
