@echo off
setlocal enabledelayedexpansion

REM 清理 .NET 项目的 bin 和 obj 目录
REM 使用方法：
REM   1. 直接运行：清理当前目录
REM   2. 拖拽目录到脚本上：清理指定目录
REM   3. 命令行参数：clean.bat "C:\YourPath"

REM 获取目标目录
if "%~1"=="" (
    set "TARGET_DIR=%CD%"
) else (
    set "TARGET_DIR=%~1"
)

REM 检查目录是否存在
if not exist "%TARGET_DIR%" (
    echo 错误：目录不存在 - %TARGET_DIR%
    pause
    exit /b 1
)

cls
echo ============================================
echo  .NET 项目清理工具
echo ============================================
echo.
echo  目标目录: %TARGET_DIR%
echo.
echo  将删除以下目录：
echo  - bin (编译输出)
echo  - obj (中间文件)
echo.
echo ============================================

echo.
echo 总计将删除约 %preview_count% 个目录
echo.


REM 开始删除
echo.
echo 正在删除，请稍候...
echo.

set deleted_count=0
set failed_count=0
set total_size=0

REM 删除 bin 和 obj 目录
for /d /r "%TARGET_DIR%" %%i in (bin obj) do (
    if exist "%%i" (
        REM 获取目录大小（简化处理）
        echo [删除中] %%i
        
        REM 尝试删除
        rd /s /q "%%i" 2>nul
        
        if !errorlevel! equ 0 (
            set /a deleted_count+=1
        ) else (
            echo [失败] 无法删除: %%i
            set /a failed_count+=1
        )
    )
)

REM 显示结果
echo.
echo ============================================
echo  清理完成
echo ============================================
echo.
echo  成功删除: %deleted_count% 个目录
if %failed_count% gtr 0 (
    echo  删除失败: %failed_count% 个目录
)
echo.
echo ============================================
