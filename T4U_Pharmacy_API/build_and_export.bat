@echo off
REM ===========================================
REM Docker Build + Export TAR
REM ===========================================

REM 取得 BAT 檔案所在目錄
SET SCRIPT_DIR=%~dp0

REM 切換到 sln 根目錄
cd /d "%SCRIPT_DIR%\T4U_Pharmacy_API"

REM 打印當前目錄確認
echo Current working directory: %CD%

REM Docker image 名稱
SET IMAGE_NAME=t4u_pharmacy_web_api:latest
SET TAR_OUTPUT=output\t4u_pharmacy_web_api_%DATE:~0,4%%DATE:~5,2%%DATE:~8,2%_%TIME:~0,2%%TIME:~3,2%.tar

REM 建立 output 目錄（如果不存在）
if not exist output mkdir output

REM Build Docker image
docker build -f docker_deploy/Dockerfile -t %IMAGE_NAME% .

REM Build 成功再 export
if %ERRORLEVEL% EQU 0 (
    echo Docker build succeeded, exporting to TAR...
    docker save -o "%TAR_OUTPUT%" %IMAGE_NAME%
    if %ERRORLEVEL% EQU 0 (
        echo Exported Docker image to %TAR_OUTPUT%
    ) else (
        echo Failed to export Docker image!
    )
) else (
    echo Docker build failed!
)
pause
