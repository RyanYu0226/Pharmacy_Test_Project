@echo off
REM ==============================
REM PostgreSQL 初始化資料庫批次檔
REM ==============================

SET PGHOST=192.168.2.25
SET PGPORT=5432
SET PGUSER=postgres
SET PGPASSWORD=xxxxxxx
SET TESTUSER=kdan_pharmacy_db
SET TESTPASS=pharmacydb@1234

SET DB_NAME=KDAN_TEST
SET CREATE_DB_SQL=.\kdan_test_database_create_20251107.sql
SET CREATE_TABLES_SQL=.\kdan_test_table_create_20251107.sql

echo  建立資料庫...
psql -h %PGHOST% -p %PGPORT% -U %PGUSER% -d postgres -f %CREATE_DB_SQL% --echo-all --set ON_ERROR_STOP=on
IF %ERRORLEVEL% NEQ 0 (
    echo  建立資料庫失敗!
	pause
    exit /b %ERRORLEVEL%
)

echo  建立資料表...
psql -h %PGHOST% -p %PGPORT% -U %TESTUSER% -d %DB_NAME% -f %CREATE_TABLES_SQL% --echo-all --set ON_ERROR_STOP=on
IF %ERRORLEVEL% NEQ 0 (
    echo ? 建立資料表失敗!
	pause
    exit /b %ERRORLEVEL%
)

echo  資料庫初始化完成！

pause
