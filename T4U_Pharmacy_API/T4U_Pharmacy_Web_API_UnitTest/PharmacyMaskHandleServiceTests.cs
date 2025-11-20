using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T4U_Pharmacy_Common_Model;
using T4U_Pharmacy_Common_Model.Common;
using T4U_Pharmacy_Repository.DBModels;
using T4U_Pharmacy_Repository.Implement;
using T4U_Pharmacy_Repository.Infrastructure;
using T4U_Pharmacy_Service;

namespace T4U_Pharmacy_Web_API_UnitTest
{
    public class PharmacyMaskHandleServiceTests
    {
        private KDAN_TESTContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<KDAN_TESTContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
                .Options;
            return new KDAN_TESTContext(options);
        }

        /// <summary>
        /// 取得指定藥局下的口罩資料
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPharmacyMasksList_ReturnsData_WhenPharmacyHasMasks()
        {
            using var context = CreateInMemoryDbContext();

            // Arrange
            var pharmacy = new Pharmacy { PharmacyId = 1, Name = "TestPharmacy", CurrentCashBalance = 1000 };
            var mask = new Mask { MasksId = 1, Name = "MaskA" };
            context.Pharmacies.Add(pharmacy);
            context.Masks.Add(mask);
            context.SaveChanges();

            var pharmacyMask = new PharmacyMask
            {
                PharmacyMasksId = 1,
                PharmacyId = pharmacy.PharmacyId,
                MasksId = mask.MasksId,
                Price = 10,
                PharmacyMasksStockLogs = new List<PharmacyMasksStockLog>
                {
                    new PharmacyMasksStockLog { StockQuantity = 5 }
                }
            };
            context.PharmacyMasks.Add(pharmacyMask);
            context.SaveChanges();

            var pharmacyRepository = new GenericRepository<Pharmacy>(context);
            var masksRepository = new GenericRepository<Mask>(context);
            var pharmacyMasksRepository = new GenericRepository<PharmacyMask>(context);
            var pharmacyMasksStockLogRepository = new GenericRepository<PharmacyMasksStockLog>(context);
            var systemConfigRepository = new GenericRepository<SystemConfig>(context);
            var unitOfWork = new UnitOfWork(context);

            var service = new PharmacyMaskHandleService(
                new PharmacyService(unitOfWork, pharmacyRepository),
                new MasksService(unitOfWork, masksRepository),
                new PharmacyMasksService(unitOfWork, pharmacyMasksRepository),
                new PharmacyMasksStockLogService(unitOfWork, pharmacyMasksStockLogRepository),
                new SystemConfigService(unitOfWork, systemConfigRepository)
            );

            // Act
            var result = await service.GetPharmacyMasksList(pharmacy.PharmacyId);

            // Assert
            Assert.True(result.Result);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal("MaskA", result.Data.First().Name);
            Assert.Equal(5, result.Data.First().StockQuantity);
        }

        /// <summary>
        /// 取得指定藥局下的口罩資料_回傳無資料
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetPharmacyMasksList_ReturnsEmptyData()
        {
            using var context = CreateInMemoryDbContext();

            // Arrange
            context.Pharmacies.AddRange(new List<Pharmacy>());
            context.Masks.AddRange(new List<Mask>());
            context.PharmacyMasks.AddRange(new List<PharmacyMask>());
            context.SaveChanges();

            var pharmacyRepository = new GenericRepository<Pharmacy>(context);
            var masksRepository = new GenericRepository<Mask>(context);
            var pharmacyMasksRepository = new GenericRepository<PharmacyMask>(context);
            var pharmacyMasksStockLogRepository = new GenericRepository<PharmacyMasksStockLog>(context);
            var systemConfigRepository = new GenericRepository<SystemConfig>(context);
            var unitOfWork = new UnitOfWork(context);

            var service = new PharmacyMaskHandleService(
                new PharmacyService(unitOfWork, pharmacyRepository),
                new MasksService(unitOfWork, masksRepository),
                new PharmacyMasksService(unitOfWork, pharmacyMasksRepository),
                new PharmacyMasksStockLogService(unitOfWork, pharmacyMasksStockLogRepository),
                new SystemConfigService(unitOfWork, systemConfigRepository)
            );

            // Act
            var result = await service.GetPharmacyMasksList(1);

            // Assert
            Assert.True(result.Result);
            Assert.Empty(result.Data);
        }

        /// <summary>
        /// 透過關鍵字搜尋藥局和口罩_回傳有資料
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SearchPharmacyMaskByKeyword_ReturnsResults()
        {
            using var context = CreateInMemoryDbContext();

            // Arrange
            var pharmacy = new Pharmacy { PharmacyId = 1, Name = "MaskPharmacy" };
            var mask = new Mask { MasksId = 1, Name = "MaskA" };
            context.Pharmacies.Add(pharmacy);
            context.Masks.Add(mask);
            context.SaveChanges();

            var pharmacyRepository = new GenericRepository<Pharmacy>(context);
            var masksRepository = new GenericRepository<Mask>(context);
            var pharmacyMasksRepository = new GenericRepository<PharmacyMask>(context);
            var pharmacyMasksStockLogRepository = new GenericRepository<PharmacyMasksStockLog>(context);
            var systemConfigRepository = new GenericRepository<SystemConfig>(context);
            var unitOfWork = new UnitOfWork(context);

            var service = new PharmacyMaskHandleService(
                new PharmacyService(unitOfWork, pharmacyRepository),
                new MasksService(unitOfWork, masksRepository),
                new PharmacyMasksService(unitOfWork, pharmacyMasksRepository),
                new PharmacyMasksStockLogService(unitOfWork, pharmacyMasksStockLogRepository),
                new SystemConfigService(unitOfWork, systemConfigRepository)
            );

            // Act
            var result = await service.SearchPharmacyMaskByKeyword("Mask", PharmacyMaskSearchType.All);

            // Assert
            Assert.True(result.Result);
            Assert.NotEmpty(result.Data);
            Assert.Contains(result.Data, x => x.Name.Contains("Mask"));
        }

        /// <summary>
        /// 透過關鍵字搜尋藥局和口罩_回傳無資料
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SearchPharmacyMaskByKeyword_ReturnsEmpty()
        {
            using var context = CreateInMemoryDbContext();

            // Arrange
            var pharmacy = new Pharmacy { PharmacyId = 1, Name = "MaskPharmacy" };
            var mask = new Mask { MasksId = 1, Name = "MaskA" };
            context.Pharmacies.Add(pharmacy);
            context.Masks.Add(mask);
            context.SaveChanges();

            var pharmacyRepository = new GenericRepository<Pharmacy>(context);
            var masksRepository = new GenericRepository<Mask>(context);
            var pharmacyMasksRepository = new GenericRepository<PharmacyMask>(context);
            var pharmacyMasksStockLogRepository = new GenericRepository<PharmacyMasksStockLog>(context);
            var systemConfigRepository = new GenericRepository<SystemConfig>(context);
            var unitOfWork = new UnitOfWork(context);

            var service = new PharmacyMaskHandleService(
                new PharmacyService(unitOfWork, pharmacyRepository),
                new MasksService(unitOfWork, masksRepository),
                new PharmacyMasksService(unitOfWork, pharmacyMasksRepository),
                new PharmacyMasksStockLogService(unitOfWork, pharmacyMasksStockLogRepository),
                new SystemConfigService(unitOfWork, systemConfigRepository)
            );

            // Act
            var result = await service.SearchPharmacyMaskByKeyword("Default", PharmacyMaskSearchType.All);

            // Assert
            Assert.True(result.Result);
            Assert.Empty(result.Data);
        }

        /// <summary>
        /// 更新藥局多個口罩價格與庫存_新增藥局口罩
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateMaskData_AddNewMaskAndStock_WorksCorrectly()
        {
            using var context = CreateInMemoryDbContext();

            // Arrange
            var pharmacy = new Pharmacy { PharmacyId = 1, Name = "TestPharmacy", CurrentCashBalance = 2000 };
            context.Pharmacies.Add(pharmacy);
            context.SaveChanges();

            var input = new UpdateMultiPharmacyMaskInput
            {
                Masks = new List<UpdateMultiPharmacyMask>
                {
                    new UpdateMultiPharmacyMask { MaskName = "MaskA", Price = 10, Stock = 5 }
                }
            };

            var pharmacyRepository = new GenericRepository<Pharmacy>(context);
            var masksRepository = new GenericRepository<Mask>(context);
            var pharmacyMasksRepository = new GenericRepository<PharmacyMask>(context);
            var pharmacyMasksStockLogRepository = new GenericRepository<PharmacyMasksStockLog>(context);
            var systemConfigRepository = new GenericRepository<SystemConfig>(context);
            var unitOfWork = new UnitOfWork(context);

            var service = new PharmacyMaskHandleService(
                new PharmacyService(unitOfWork, pharmacyRepository),
                new MasksService(unitOfWork, masksRepository),
                new PharmacyMasksService(unitOfWork, pharmacyMasksRepository),
                new PharmacyMasksStockLogService(unitOfWork, pharmacyMasksStockLogRepository),
                new SystemConfigService(unitOfWork, systemConfigRepository)
            );

            // Act
            var result = await service.UpdateMaskData(1, input);

            // Assert
            Assert.True(result.Result);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);

            var mask = result.Data.First();
            Assert.Equal("MaskA", mask.Name);
            Assert.Equal(5, mask.StockQuantity);
            Assert.Equal(10, mask.Price);

            Assert.Equal(1, context.Masks.Count());
            Assert.Equal(1, context.PharmacyMasks.Count());
            Assert.Equal(1, context.PharmacyMasksStockLogs.Count());
        }

        /// <summary>
        /// 更新藥局多個口罩價格與庫存_更新庫存
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateMaskData_UpdateExistingMask_AccumulatesStock()
        {
            using var context = CreateInMemoryDbContext();

            // Arrange
            var pharmacy = new Pharmacy { PharmacyId = 1, Name = "TestPharmacy", CurrentCashBalance = 2000 };
            var mask = new Mask { MasksId = 1, Name = "MaskA" };
            context.Pharmacies.Add(pharmacy);
            context.Masks.Add(mask);
            context.SaveChanges();

            var pharmacyMask = new PharmacyMask
            {
                PharmacyMasksId = 1,
                PharmacyId = 1,
                MasksId = 1,
                Price = 8
            };
            context.PharmacyMasks.Add(pharmacyMask);
            context.SaveChanges();

            // 初始 5 庫存
            context.PharmacyMasksStockLogs.Add(new PharmacyMasksStockLog
            {
                PharmacyMasksId = 1,
                StockQuantity = 5,
                Price = 8
            });
            context.SaveChanges();

            var input = new UpdateMultiPharmacyMaskInput
            {
                Masks = new List<UpdateMultiPharmacyMask>
                {
                    new UpdateMultiPharmacyMask { MaskName = "MaskA", Price = 12, Stock = 10 }
                }
            };

            var pharmacyRepository = new GenericRepository<Pharmacy>(context);
            var masksRepository = new GenericRepository<Mask>(context);
            var pharmacyMasksRepository = new GenericRepository<PharmacyMask>(context);
            var pharmacyMasksStockLogRepository = new GenericRepository<PharmacyMasksStockLog>(context);
            var systemConfigRepository = new GenericRepository<SystemConfig>(context);
            var unitOfWork = new UnitOfWork(context);

            var service = new PharmacyMaskHandleService(
                new PharmacyService(unitOfWork, pharmacyRepository),
                new MasksService(unitOfWork, masksRepository),
                new PharmacyMasksService(unitOfWork, pharmacyMasksRepository),
                new PharmacyMasksStockLogService(unitOfWork, pharmacyMasksStockLogRepository),
                new SystemConfigService(unitOfWork, systemConfigRepository)
            );

            // Act
            var result = await service.UpdateMaskData(1, input);

            // Assert
            Assert.True(result.Result);

            var updated = result.Data.First();
            Assert.Equal("MaskA", updated.Name);
            Assert.Equal(10, updated.StockQuantity);
            Assert.Equal(12, updated.Price);

            Assert.Equal(2, context.PharmacyMasksStockLogs.Count());
            var lastLog = context.PharmacyMasksStockLogs.OrderByDescending(x => x.PharmacyMasksStockLogId).First();
            Assert.Equal(5, lastLog.StockQuantity);
        }

        /// <summary>
        /// 更新藥局多個口罩價格與庫存_Pharmacy 不存在 → 應回傳失敗
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateMaskData_Fails_WhenPharmacyNotFound()
        {
            using var context = CreateInMemoryDbContext();

            var input = new UpdateMultiPharmacyMaskInput
            {
                Masks = new List<UpdateMultiPharmacyMask>
                {
                    new UpdateMultiPharmacyMask{ MaskName = "MaskA", Price = 10, Stock = 5 }
                }
            };

            var pharmacyRepository = new GenericRepository<Pharmacy>(context);
            var masksRepository = new GenericRepository<Mask>(context);
            var pharmacyMasksRepository = new GenericRepository<PharmacyMask>(context);
            var pharmacyMasksStockLogRepository = new GenericRepository<PharmacyMasksStockLog>(context);
            var systemConfigRepository = new GenericRepository<SystemConfig>(context);
            var unitOfWork = new UnitOfWork(context);

            var service = new PharmacyMaskHandleService(
                new PharmacyService(unitOfWork, pharmacyRepository),
                new MasksService(unitOfWork, masksRepository),
                new PharmacyMasksService(unitOfWork, pharmacyMasksRepository),
                new PharmacyMasksStockLogService(unitOfWork, pharmacyMasksStockLogRepository),
                new SystemConfigService(unitOfWork, systemConfigRepository)
            );

            var result = await service.UpdateMaskData(999, input);

            Assert.False(result.Result);
            Assert.NotNull(result.Message);
        }

        /// <summary>
        /// 更新藥局多個口罩價格與庫存_ Masks 清單為空 → 應失敗
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateMaskData_Fails_WhenMaskListEmpty()
        {
            using var context = CreateInMemoryDbContext();

            context.Pharmacies.Add(new Pharmacy { PharmacyId = 1, Name = "Test" });
            context.SaveChanges();

            var input = new UpdateMultiPharmacyMaskInput { Masks = new List<UpdateMultiPharmacyMask>() };

            var pharmacyRepository = new GenericRepository<Pharmacy>(context);
            var masksRepository = new GenericRepository<Mask>(context);
            var pharmacyMasksRepository = new GenericRepository<PharmacyMask>(context);
            var pharmacyMasksStockLogRepository = new GenericRepository<PharmacyMasksStockLog>(context);
            var systemConfigRepository = new GenericRepository<SystemConfig>(context);
            var unitOfWork = new UnitOfWork(context);

            var service = new PharmacyMaskHandleService(
                new PharmacyService(unitOfWork, pharmacyRepository),
                new MasksService(unitOfWork, masksRepository),
                new PharmacyMasksService(unitOfWork, pharmacyMasksRepository),
                new PharmacyMasksStockLogService(unitOfWork, pharmacyMasksStockLogRepository),
                new SystemConfigService(unitOfWork, systemConfigRepository)
            );

            var result = await service.UpdateMaskData(1, input);

            Assert.False(result.Result);
        }

        /// <summary>
        /// 更新藥局多個口罩價格與庫存_同名 mask 重複 → 應失敗，同口罩名稱只能放一筆
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateMaskData_IgnoresDuplicateMaskNames()
        {
            using var context = CreateInMemoryDbContext();

            context.Pharmacies.Add(new Pharmacy { PharmacyId = 1, Name = "TestPharmacy", CurrentCashBalance = 2000 });
            context.SaveChanges();

            var input = new UpdateMultiPharmacyMaskInput
            {
                Masks = new List<UpdateMultiPharmacyMask>
        {
            new UpdateMultiPharmacyMask{ MaskName = "MaskA", Price = 10, Stock = 5 },
            new UpdateMultiPharmacyMask{ MaskName = "MaskA", Price = 10, Stock = 5 } // duplicate
        }
            };

            var pharmacyRepository = new GenericRepository<Pharmacy>(context);
            var masksRepository = new GenericRepository<Mask>(context);
            var pharmacyMasksRepository = new GenericRepository<PharmacyMask>(context);
            var pharmacyMasksStockLogRepository = new GenericRepository<PharmacyMasksStockLog>(context);
            var systemConfigRepository = new GenericRepository<SystemConfig>(context);
            var unitOfWork = new UnitOfWork(context);

            var service = new PharmacyMaskHandleService(
                new PharmacyService(unitOfWork, pharmacyRepository),
                new MasksService(unitOfWork, masksRepository),
                new PharmacyMasksService(unitOfWork, pharmacyMasksRepository),
                new PharmacyMasksStockLogService(unitOfWork, pharmacyMasksStockLogRepository),
                new SystemConfigService(unitOfWork, systemConfigRepository)
            );

            var result = await service.UpdateMaskData(1, input);

            Assert.False(result.Result);
        }

        /// <summary>
        /// 更新藥局多個口罩價格與庫存_Price 或 Stock 為負 → ValidUpdateMaskData 應擋下（前置邏輯）
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateMaskData_Fails_WhenInvalidPriceOrStock()
        {
            using var context = CreateInMemoryDbContext();

            context.Pharmacies.Add(new Pharmacy { PharmacyId = 1, Name = "TestPharmacy" });
            context.SaveChanges();

            var input = new UpdateMultiPharmacyMaskInput
            {
                Masks = new List<UpdateMultiPharmacyMask>
        {
            new UpdateMultiPharmacyMask{ MaskName = "MaskA", Price = -1, Stock = 5 }
        }
            };

            var pharmacyRepository = new GenericRepository<Pharmacy>(context);
            var masksRepository = new GenericRepository<Mask>(context);
            var pharmacyMasksRepository = new GenericRepository<PharmacyMask>(context);
            var pharmacyMasksStockLogRepository = new GenericRepository<PharmacyMasksStockLog>(context);
            var systemConfigRepository = new GenericRepository<SystemConfig>(context);
            var unitOfWork = new UnitOfWork(context);

            var service = new PharmacyMaskHandleService(
                new PharmacyService(unitOfWork, pharmacyRepository),
                new MasksService(unitOfWork, masksRepository),
                new PharmacyMasksService(unitOfWork, pharmacyMasksRepository),
                new PharmacyMasksStockLogService(unitOfWork, pharmacyMasksStockLogRepository),
                new SystemConfigService(unitOfWork, systemConfigRepository)
            );

            var result = await service.UpdateMaskData(1, input);

            Assert.False(result.Result);
            Assert.False(string.IsNullOrEmpty(result.Message));
        }

        /// <summary>
        /// 更新藥局多個口罩價格與庫存_多筆混合：新增 + 更新同時發生
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateMaskData_MixedAddAndUpdate_Works()
        {
            using var context = CreateInMemoryDbContext();

            // 已存在資料（1筆）
            context.Pharmacies.Add(new Pharmacy { PharmacyId = 1, Name = "Test", CurrentCashBalance = 5000 });
            context.Masks.Add(new Mask { MasksId = 1, Name = "MaskA" });
            context.SaveChanges();

            context.PharmacyMasks.Add(new PharmacyMask
            {
                PharmacyMasksId = 1,
                PharmacyId = 1,
                MasksId = 1,
                Price = 8
            });
            context.PharmacyMasksStockLogs.Add(new PharmacyMasksStockLog
            {
                PharmacyMasksId = 1,
                StockQuantity = 5,
                Price = 8
            });
            context.SaveChanges();

            // 新增 MaskB、更新 MaskA
            var input = new UpdateMultiPharmacyMaskInput
            {
                Masks = new List<UpdateMultiPharmacyMask>
        {
            new UpdateMultiPharmacyMask{ MaskName = "MaskA", Price = 12, Stock = 10 }, // existing
            new UpdateMultiPharmacyMask{ MaskName = "MaskB", Price = 20, Stock = 3 }   // new
        }
            };

            var pharmacyRepository = new GenericRepository<Pharmacy>(context);
            var masksRepository = new GenericRepository<Mask>(context);
            var pharmacyMasksRepository = new GenericRepository<PharmacyMask>(context);
            var pharmacyMasksStockLogRepository = new GenericRepository<PharmacyMasksStockLog>(context);
            var systemConfigRepository = new GenericRepository<SystemConfig>(context);
            var unitOfWork = new UnitOfWork(context);

            var service = new PharmacyMaskHandleService(
                new PharmacyService(unitOfWork, pharmacyRepository),
                new MasksService(unitOfWork, masksRepository),
                new PharmacyMasksService(unitOfWork, pharmacyMasksRepository),
                new PharmacyMasksStockLogService(unitOfWork, pharmacyMasksStockLogRepository),
                new SystemConfigService(unitOfWork, systemConfigRepository)
            );

            var result = await service.UpdateMaskData(1, input);

            Assert.True(result.Result);
            Assert.Equal(2, result.Data.Count);

            Assert.Contains(result.Data, x => x.Name == "MaskA" && x.StockQuantity == 10);
            Assert.Contains(result.Data, x => x.Name == "MaskB" && x.StockQuantity == 3);
        }
    }
}
