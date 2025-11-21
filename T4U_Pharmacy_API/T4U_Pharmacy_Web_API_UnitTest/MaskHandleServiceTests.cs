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
    public class MaskHandleServiceTests
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
        private MaskHandleService CreateService(KDAN_TESTContext context)
        {
            var unitOfWork = new UnitOfWork(context);
            var pharmacyRepository = new GenericRepository<Pharmacy>(context);
            var pharmacyService = new PharmacyService(unitOfWork, pharmacyRepository);
            var pharmacyMasksRepository = new GenericRepository<PharmacyMask>(context);
            var pharmacyMasksService = new PharmacyMasksService(unitOfWork, pharmacyMasksRepository);
            var pharmacyMasksStockLogRepository = new GenericRepository<PharmacyMasksStockLog>(context);
            var pharmacyMasksStockLogService = new PharmacyMasksStockLogService(unitOfWork, pharmacyMasksStockLogRepository);
            return new MaskHandleService( pharmacyService, pharmacyMasksService, pharmacyMasksStockLogService);
        }

        /// <summary>
        /// 建立種子資料
        /// </summary>
        /// <param name="context"></param>
        private void SeedData(KDAN_TESTContext context)
        {
            var pharmacy = new Pharmacy { PharmacyId = 1, Name = "Test Pharmacy" };
            context.Pharmacies.Add(pharmacy);
            var mask = new Mask { MasksId = 1, Name = "Test Mask" };
            context.Masks.Add(mask);

            var item = new PharmacyMask
            {
                PharmacyMasksId = 1,
                PharmacyId = 1,
                MasksId = 1,
                Price = 25
            };
            context.PharmacyMasks.Add(item);

            var log = new PharmacyMasksStockLog
            {
                PharmacyMasksId = 1,
                StockQuantity = 5,
                Price = 25
            };
            context.PharmacyMasksStockLogs.Add(log);

            context.SaveChanges();
        }

        /// <summary>
        /// 新增成功_測試案例
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateMaskQuantity_Success()
        {
            // Arrange: 建立 InMemory DB
            var db = CreateInMemoryDbContext();
            // 建立DB資料
            SeedData(db);
            // 建立邏輯Service
            var service = CreateService(db);

            var input = new UpdateMaskInput
            {
                PharmacyId = 1,
                MasksId = 1,
                Delta = 3 // 增加 3
            };

            // Act
            var result = await service.UpdateMaskQuantity(input);

            // Assert
            Assert.True(result.Result);
            Assert.True(string.IsNullOrEmpty(result.Message));

            var logs = db.PharmacyMasksStockLogs
            .Where(n => n.PharmacyMasksId == 1)
            .ToList();

            Assert.Equal(2, logs.Count);     // 原本 1 + 新增 1
            Assert.Equal(8, logs.Sum(x => x.StockQuantity)); // 5 + 3

            var detail = result.Data.First();
            Assert.Equal(8, detail.StockQuantity);
            Assert.Equal("Test Pharmacy", detail.PharmacyName);
            Assert.Equal("Test Mask", detail.MasksName);
        }

        /// <summary>
        /// 客戶不存在_測試案例
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateMaskQuantity_Fail_NoPharmacy()
        {
            // Arrange: 建立 InMemory DB
            var db = CreateInMemoryDbContext();
            // 建立DB資料
            SeedData(db);
            // 建立邏輯Service
            var service = CreateService(db);

            var input = new UpdateMaskInput
            {
                PharmacyId = 99,
                MasksId = 10,
                Delta = 3
            };

            var result = await service.UpdateMaskQuantity(input);

            Assert.False(result.Result);
            Assert.Contains("無此藥局", result.Message);
        }

        /// <summary>
        /// 無此藥局口罩_測試案例
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateMaskQuantity_Fail_NoMaskInPharmacy()
        {
            // Arrange: 建立 InMemory DB
            var db = CreateInMemoryDbContext();
            // 建立DB資料
            SeedData(db);
            // 建立邏輯Service
            var service = CreateService(db);

            var input = new UpdateMaskInput
            {
                PharmacyId = 1,
                MasksId = 10,
                Delta = 3
            };

            var result = await service.UpdateMaskQuantity(input);

            Assert.False(result.Result);
            Assert.Contains("無對應口罩", result.Message);
        }

        /// <summary>
        /// 刪除庫存，但庫存不足_測試案例
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateMaskQuantity_Fail_InsufficientStock()
        {
            // Arrange: 建立 InMemory DB
            var db = CreateInMemoryDbContext();
            // 建立DB資料
            SeedData(db);
            // 建立邏輯Service
            var service = CreateService(db);

            var input = new UpdateMaskInput
            {
                PharmacyId = 1,
                MasksId = 1,
                Delta = -6 // 想扣 6 但只有 5
            };

            // Act
            var result = await service.UpdateMaskQuantity(input);

            // Assert
            Assert.False(result.Result);
            Assert.Contains("無法刪除到負數", result.Message);
        }
    }
}
