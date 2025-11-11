#!/bin/bash

# ==============================
# PostgreSQL 初始化資料庫腳本
# ==============================

# 連線設定
export PGHOST="localhost"         # PostgreSQL 主機
export PGPORT="5432"              # PostgreSQL 連接埠
export PGUSER="postgres"          # 使用者
export PGPASSWORD="123456"        # 密碼

# 資料庫名稱
DB_NAME="pharmacy_db"

# SQL 檔案
CREATE_DB_SQL="./create_database.sql"
CREATE_TABLES_SQL="./create_tables.sql"

echo " 建立資料庫..."
psql -h "$PGHOST" -p "$PGPORT" -U "$PGUSER" -d postgres -f "$CREATE_DB_SQL" --echo-all --set ON_ERROR_STOP=on
if [ $? -ne 0 ]; then
    echo "? 建立資料庫失敗!"
    exit 1
fi

echo " 建立資料表..."
psql -h "$PGHOST" -p "$PGPORT" -U "$PGUSER" -d "$DB_NAME" -f "$CREATE_TABLES_SQL" --echo-all --set ON_ERROR_STOP=on
if [ $? -ne 0 ]; then
    echo " 建立資料表失敗!"
    exit 1
fi

echo " 資料庫初始化完成！"
