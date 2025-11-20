using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T4U_Pharmacy_Repository.DBModels;
using T4U_Pharmacy_Repository.Implement;
using T4U_Pharmacy_Repository.Infrastructure;
using T4U_Pharmacy_Service;
using static T4U_Pharmacy_Common_Model.Common.CommonStruct;

namespace T4U_Pharmacy_Web_API_UnitTest
{
    public class PharmacyMasksServiceTests
    {
        private KDAN_TESTContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<KDAN_TESTContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
                .Options;
            return new KDAN_TESTContext(options);
        }

        private PharmacyMasksService CreateService(KDAN_TESTContext context)
        {
            var unitOfWork = new UnitOfWork(context);
            var pharmacyMasksRepository = new GenericRepository<PharmacyMask>(context);
            return new PharmacyMasksService(unitOfWork, pharmacyMasksRepository);
        }

        private void SeedData(KDAN_TESTContext context)
        {
            var pharmacy = new Pharmacy { PharmacyId = 1, Name = "A藥局" };
            var mask = new Mask { MasksId = 1, Name = "醫療口罩" };

            var item = new PharmacyMask
            {
                PharmacyId = 1,
                MasksId = 1,
                Price = 50,
                Pharmacy = pharmacy,
                Masks = mask,
                PharmacyMasksStockLogs = new List<PharmacyMasksStockLog>
                {
                    new PharmacyMasksStockLog { StockQuantity = 100 },
                    new PharmacyMasksStockLog { StockQuantity = 20 }
                }
            };

            context.Pharmacies.Add(pharmacy);
            context.Masks.Add(mask);
            context.PharmacyMasks.Add(item);
            context.SaveChanges();
        }

        // ============================
        // 1. Above 成功案例
        // ============================
        [Fact]
        public async Task GetPharmaciesByMaskStock_Above_ReturnsData()
        {
            var context = CreateInMemoryDbContext();
            SeedData(context);
            var service = CreateService(context);

            var result = await service.GetPharmaciesByMaskStock(null, null, 50, null, QuantityThresholdType.Above);

            Assert.True(result.Result);
            Assert.Single(result.Data); // 120 >= 50
        }

        // ============================
        // 2. Below 成功案例
        // ============================
        [Fact]
        public async Task GetPharmaciesByMaskStock_Below_ReturnsData()
        {
            var context = CreateInMemoryDbContext();
            SeedData(context);
            var service = CreateService(context);

            var result = await service.GetPharmaciesByMaskStock(null, null, null, 200, QuantityThresholdType.Below);

            Assert.True(result.Result);
            Assert.Single(result.Data); // 120 <= 200
        }

        // ============================
        // 3. Between 成功案例
        // ============================
        [Fact]
        public async Task GetPharmaciesByMaskStock_Between_ReturnsData()
        {
            var context = CreateInMemoryDbContext();
            SeedData(context);
            var service = CreateService(context);

            var result = await service.GetPharmaciesByMaskStock(null, null, 100, 150, QuantityThresholdType.Between);

            Assert.True(result.Result);
            Assert.Single(result.Data); // 120 in [100,150]
        }

        // ============================
        // 4. 查無資料 → 空集合
        // ============================
        [Fact]
        public async Task GetPharmaciesByMaskStock_NoMatchingData_ReturnsEmptyList()
        {
            var context = CreateInMemoryDbContext();
            SeedData(context);
            var service = CreateService(context);

            var result = await service.GetPharmaciesByMaskStock(999, null, 999, 999);

            Assert.True(result.Result);
            Assert.Empty(result.Data); // 價格不符合
        }
    }
}
