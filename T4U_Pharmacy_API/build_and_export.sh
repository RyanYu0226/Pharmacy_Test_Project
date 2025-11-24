#!/bin/bash
# ===========================================
# Docker Build + Export TAR
# ===========================================

# 取得腳本所在目錄
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

# 切換到 sln 根目錄
cd "$SCRIPT_DIR/T4U_Pharmacy_API" || { echo "Directory not found!"; exit 1; }

# 打印當前目錄確認
echo "Current working directory: $(pwd)"

# Docker image 名稱
IMAGE_NAME="t4u_pharmacy_web_api:latest"
TIMESTAMP=$(date +"%Y%m%d_%H%M")
TAR_OUTPUT="output/t4u_pharmacy_web_api_${TIMESTAMP}.tar"

# 建立 output 目錄（如果不存在）
mkdir -p output

# Build Docker image
docker build -f docker_deploy/Dockerfile -t "$IMAGE_NAME" .

# Build 成功再 export
if [ $? -eq 0 ]; then
    echo "Docker build succeeded, exporting to TAR..."
    docker save -o "$TAR_OUTPUT" "$IMAGE_NAME"
    if [ $? -eq 0 ]; then
        echo "Exported Docker image to $TAR_OUTPUT"
    else
        echo "Failed to export Docker image!"
        exit 1
    fi
else
    echo "Docker build failed!"
    exit 1
fi
