@echo off
chcp 65001 >nul
echo ========================================
echo   DocxDiffTool — 构建安装包
echo ========================================
echo.

REM Check Python
python --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [错误] 未找到 Python，请先安装 Python 3.10+
    pause
    exit /b 1
)

echo [1/3] 安装依赖...
pip install -r requirements.txt -q

echo [2/3] 正在打包为 exe...
pyinstaller --onefile --noconsole --add-data "static;static" --name DocxDiffTool app.py

echo [3/3] 清理临时文件...
rmdir /s /q build 2>nul
del /q DocxDiffTool.spec 2>nul

echo.
echo ========================================
echo   构建完成！
echo   安装包位置: dist\DocxDiffTool.exe
echo   双击即可运行，自动打开浏览器。
echo ========================================
pause
