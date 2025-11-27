## 專案開啟基本需求
1. Visual Studio 2022
2. .NET 8
3. PostgreSQL
4. Docker

## 資料庫建立
資料庫為PostgreSQL  
可透過/initDB/kdan_test_create_database_20251107.bat(.sh)建立DB和Table，但須要把對應參數內容作調整  
1. PGHOST=192.168.2.25  => 對應資料庫IP
2. PGPORT=5432  =>  對應Port
3. PGUSER=postgres  => 建立資料庫和使用者所要使用的帳號，這邊使用PostgreSQL預設最大權限帳號
4. PGPASSWORD=xxxxxxxx =>  對應第3項的帳號密碼
5. TESTUSER=kdan_pharmacy_db  =>  對應要新增的資料庫使用者帳號，這邊不用調整
6. TESTPASS=pharmacydb@1234  =>  對應新增資料庫使用者帳號密碼，這邊不用調整
7. DB_NAME=KDAN_TEST  =>  對應新增資料庫名稱，不用調整
8. CREATE_DB_SQL=.\kdan_test_database_create_20251107.sql  =>  建立DB和使用者的SQL語法，請確認執行的路徑是否對應
9. CREATE_TABLES_SQL=.\kdan_test_table_create_20251107.sql  =>  建立Table的SQL語法，請確認執行的路徑是否對應

## API需求
* [ ] List pharmacies, optionally filtered by specific time and/or day of the week.
  * 在/api/v1/Pharmacy/GetPharmacies 取得藥局資訊
* [ ] List all masks sold by a given pharmacy with an option to sort by name or price.
  * 在/api/v1/Masks/GetPharmacyMasksList 取得指定藥局底下的口照資料
* [ ] List all pharmacies that offer a number of mask products within a given price range, where the count is above, below, or between given thresholds.
  * 在/api/v1/Pharmacy/GetPharmaciesByMaskStock 取得販售口罩數量在特定價格區間內的所有藥局
* [ ] Show the top N users who spent the most on masks during a specific date range.
  * 在/api/v1/Customer/GetTopCustomerPurchaseHistory 顯示在指定日期區間內，購買口罩金額最高的前N名使用者
* [ ] Process a purchase where a user buys masks from multiple pharmacies at once.
  *  在/api/v1/Purchase/InsertPurchaseHistory 處理一筆購買交易，讓使用可同時向多間藥局購買口罩
* [ ] Update the stock quantity of an existing mask product by increasing or decreasing it.
  * 在/api/v1/Masks/UpdateMaskQuantity 更新藥局口罩庫存
* [ ] Create or update multiple mask products for a pharmacy at once, including name, price, and stock quantity.
  * 在/api/v1/PharmacyMask/UpdateMaskData 更新藥局口罩價格庫存
* [ ] Search for pharmacies or masks by name and rank the results by relevance to the search term.
  * 在/api/v1/PharmacyMask/SearchPharmacyMaskByKeyword 透過關鍵字搜尋藥局和口罩

## API Document
本專案使用Swagger，內容在/swagger/index.html  
測試網站：https://kdan_pharmacy_mask.think4u-tech.com/swagger/index.html

## Import Data Commands
資料初始化的部分，可以透過API執行(/api/v1/DB/InitDBData)，將資料還原

## Test Coverage Report
總共寫了6個單元測試Class，如下
1. MaskHandlerServiceTests
2. PharmacyMaskHandlerServiceTests
3. PharmacyMasksServiceTests
4. PharmacyServiceTests
5. PurchaseHandlerServiceTests
6. PurchaseHistoryServiceTests

覆蓋率報告位於路徑/T4U_Pharmacy_API/T4U_Pharmacy_Web_API_UnitTest/TestResults/coveragereport/index.html  
覆蓋率報告，沒覆蓋到的部分為DB相關的部分，因為API使用上不會使用到所有Table，所以在測試覆蓋率上會有缺  
  
P.S. 如需自己產出，可使用以下指令產出報告的XML檔案，並透過ReportGenerator工具產程html報告  
1. 在/T4U_Pharmacy_API/T4U_Pharmacy_Web_API_UnitTest資料夾下 `dotnet test --collect:"XPlat Code Coverage"`  
=>產出 xml檔案，會放在在TestResults/{GUID}/coverage.cobertura.xml
2. `dotnet tool install -g dotnet-reportgenerator-globaltool`  => 安裝ReportGenerator，如果已有，此步驟可以跳過
3. `reportgenerator -reports:"XML檔案位置" -targetdir:"輸出檔案資料夾位置"`  
EX: `reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport"`


## Deployment
此專案可透過Docker建立，執行以下程式，產出對應image和tar檔案  
Windows => /T4U_Pharmacy_API/build_and_export.bat  
Linus/iOS => /T4U_Pharmacy_API/build_and_export.sh  

目標環境為Linus，將image檔案Load進去  
1. CD到tar檔案目標資料夾
2. 執行命令`docker load -i "tar檔案名稱"`
3. 使用/docker_deploy/docker-compose.yml建置  
//CD到對應資料夾上用對應的docker-compose檔案 將docker卸載移除  
`docker compose -f docker-compose.yml down`  
//CD到對應資料夾上用對應的docker-compose檔案 將docker安裝  
`docker compose -f docker-compose.yml up -d`  

預設網站建置在該環境8080 port  


## Additional Data
DB_SCHEMA => DB_SCHEMA_20251105.xlsx
