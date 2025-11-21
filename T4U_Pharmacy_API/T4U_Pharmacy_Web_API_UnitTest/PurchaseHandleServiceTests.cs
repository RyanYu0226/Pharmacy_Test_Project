using Castle.Core.Resource;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T4U_Pharmacy_Common_Model;
using T4U_Pharmacy_Repository.DBModels;
using T4U_Pharmacy_Repository.Implement;
using T4U_Pharmacy_Repository.Infrastructure;
using T4U_Pharmacy_Service;

namespace T4U_Pharmacy_Web_API_UnitTest
{
    public class PurchaseHandleServiceTests
    {
        /// <summary>
        /// 建立DB In Memory
        /// </summary>
        /// <returns></returns>
        private KDAN_TESTContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<KDAN_TESTContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
                .Options;
            return new KDAN_TESTContext(options);
        }

        /// <summary>
        /// 建立對應Service
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private PurchaseHandleService CreateService(KDAN_TESTContext context)
        {
            var unitOfWork = new UnitOfWork(context);
            var CustomerRepository = new GenericRepository<Customer>(context);
            var customerService = new CustomerService(unitOfWork, CustomerRepository);
            var pharmacyRepository = new GenericRepository<Pharmacy>(context);
            var pharmacyService = new PharmacyService(unitOfWork, pharmacyRepository);
            var pharmacyMasksRepository = new GenericRepository<PharmacyMask>(context);
            var pharmacyMasksService = new PharmacyMasksService(unitOfWork, pharmacyMasksRepository);
            var purchaseHistoryRepository = new GenericRepository<PurchaseHistory>(context);
            var purchaseHistoryService = new PurchaseHistoryService(unitOfWork, purchaseHistoryRepository);
            var systemConfigRepository = new GenericRepository<SystemConfig>(context);
            var systemConfigService = new SystemConfigService(unitOfWork, systemConfigRepository);
            var pharmacyMasksStockLogRepository = new GenericRepository<PharmacyMasksStockLog>(context);
            var pharmacyMasksStockLogService = new PharmacyMasksStockLogService(unitOfWork, pharmacyMasksStockLogRepository);
            return new PurchaseHandleService(customerService, pharmacyService, pharmacyMasksService, purchaseHistoryService, systemConfigService, pharmacyMasksStockLogService);
        }

        /// <summary>
        /// 建立種子資料
        /// </summary>
        /// <param name="context"></param>
        private void SeedData(KDAN_TESTContext context)
        {
            var customer = new Customer {
                CustomerId = 1,
                CashBalance = 1000,
                Name = "Test",
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                CurrentCashBalance = 1000
            };
            context.Customers.Add(customer);    
            var pharmacy = new Pharmacy { PharmacyId = 1, Name = "A藥局" };
            context.Pharmacies.Add(pharmacy);
            var mask = new Mask { MasksId = 1, Name = "醫療口罩" };
            context.Masks.Add(mask);

            var item = new PharmacyMask
            {
                PharmacyId = 1,
                MasksId = 1,
                Price = 50,
                Pharmacy = pharmacy,
                Masks = mask,
                PharmacyMasksStockLogs = new List<PharmacyMasksStockLog>
                {
                    new PharmacyMasksStockLog { StockQuantity = 20, Price = 50 }
                }
            };

            context.PharmacyMasks.Add(item);
            context.SaveChanges();
        }

        /// <summary>
        /// 新增成功_測試案例
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task InsertPurchaseHistory_Success()
        {
            // Arrange: 建立 InMemory DB
            var db = CreateInMemoryDbContext();
            // 建立DB資料
            SeedData(db);
            // 建立邏輯Service
            var _service = CreateService(db);

            var input = new PharmacyOrdersInput
            {
                CustomerId = 1,
                PharmacyOrders = new List<PharmacyOrder>
                {
                    new PharmacyOrder
                    {
                        PharmacyId = 1,
                        Items = new List<MaskOrderDetail>
                        {
                            new MaskOrderDetail { MasksId = 1, Quantity = 3 }
                        }
                    }
                }
            };

            // Act
            var result = await _service.InsertPurchaseHistory(input);

            // Assert
            Assert.True(result.Result);
            Assert.True(string.IsNullOrEmpty(result.Message));

            // 確認購買紀錄有新增
            var purchase = db.PurchaseHistories.FirstOrDefault();
            Assert.NotNull(purchase);
            Assert.Equal(3, purchase.TransactionQuantity);
            Assert.Equal(50, purchase.TransactionAmount);

            // 確認庫存變動紀錄
            var stockLog = db.PharmacyMasksStockLogs.OrderByDescending(x => x.CreatedDate).First();
            Assert.Equal(-3, stockLog.StockQuantity);
        }

        /// <summary>
        /// 客戶不存在_測試案例
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task InsertPurchaseHistory_CustomerNotFound()
        {
            // Arrange: 建立 InMemory DB
            var db = CreateInMemoryDbContext();
            // 建立DB資料
            SeedData(db);
            // 建立邏輯Service
            var _service = CreateService(db);

            var input = new PharmacyOrdersInput
            {
                CustomerId = 999,
                PharmacyOrders = new List<PharmacyOrder>()
            };

            var result = await _service.InsertPurchaseHistory(input);

            Assert.False(result.Result);
            Assert.Equal("客戶不存在", result.Message);
        }

        /// <summary>
        /// 藥局不存在_測試案例
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task InsertPurchaseHistory_PharmacyNotFound()
        {
            // Arrange: 建立 InMemory DB
            var db = CreateInMemoryDbContext();
            // 建立DB資料
            SeedData(db);
            // 建立邏輯Service
            var _service = CreateService(db);

            var input = new PharmacyOrdersInput
            {
                CustomerId = 1,
                PharmacyOrders = new List<PharmacyOrder>
                {
                    new PharmacyOrder
                    {
                        PharmacyId = 2,
                        Items = new List<MaskOrderDetail>()
                    }
                }
            };

            var result = await _service.InsertPurchaseHistory(input);

            Assert.False(result.Result);
            Assert.Equal($"沒有對應藥局，PharmacyId：2", result.Message);
        }

        /// <summary>
        /// 藥局口罩不存在_測試案例
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task InsertPurchaseHistory_MaskNotFound()
        {
            // Arrange: 建立 InMemory DB
            var db = CreateInMemoryDbContext();
            // 建立DB資料
            SeedData(db);
            // 建立邏輯Service
            var _service = CreateService(db);

            var input = new PharmacyOrdersInput
            {
                CustomerId = 1,
                PharmacyOrders = new List<PharmacyOrder>
                {
                    new PharmacyOrder
                    {
                        PharmacyId = 1,
                        Items = new List<MaskOrderDetail>
                        {
                            new MaskOrderDetail { MasksId = 999, Quantity = 1 }
                        }
                    }
                }
            };

            var result = await _service.InsertPurchaseHistory(input);

            Assert.False(result.Result);
            Assert.Contains("對應藥局沒有此口罩", result.Message);
        }

        /// <summary>
        /// 庫存不足_測試案例
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task InsertPurchaseHistory_InsufficientStock()
        {
            // Arrange: 建立 InMemory DB
            var db = CreateInMemoryDbContext();
            // 建立DB資料
            SeedData(db);
            // 建立邏輯Service
            var _service = CreateService(db);

            var input = new PharmacyOrdersInput
            {
                CustomerId = 1,
                PharmacyOrders = new List<PharmacyOrder>
                {
                    new PharmacyOrder
                    {
                        PharmacyId = 1,
                        Items = new List<MaskOrderDetail>
                        {
                            new MaskOrderDetail { MasksId = 1, Quantity = 22 } // 超過庫存
                        }
                    }
                }
            };

            var result = await _service.InsertPurchaseHistory(input);

            Assert.False(result.Result);
            Assert.Contains("口罩數量不足", result.Message);
        }
    }
}
