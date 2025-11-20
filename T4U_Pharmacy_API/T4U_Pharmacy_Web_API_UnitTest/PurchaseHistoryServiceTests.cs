using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T4U_Pharmacy_Repository.DBModels;
using T4U_Pharmacy_Repository.Implement;
using T4U_Pharmacy_Repository.Infrastructure;
using T4U_Pharmacy_Service;

namespace T4U_Pharmacy_Web_API_UnitTest
{
    public class PurchaseHistoryServiceTests
    {
        private KDAN_TESTContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<KDAN_TESTContext>()
                .UseInMemoryDatabase("TestDb_" + Guid.NewGuid())
                .Options;
            return new KDAN_TESTContext(options);
        }

        private void SeedData(KDAN_TESTContext context)
        {
            // 建立客戶、藥局、口罩、購買紀錄
            var customer1 = new Customer { CustomerId = 1, Name = "Alice" };
            var customer2 = new Customer { CustomerId = 2, Name = "Bob" };

            var pharmacy = new Pharmacy { PharmacyId = 1, Name = "PharmacyA" };
            var mask = new Mask { MasksId = 1, Name = "MaskA" };

            var pharmacyMask = new PharmacyMask
            {
                PharmacyMasksId = 1,
                Pharmacy = pharmacy,
                PharmacyId = pharmacy.PharmacyId,
                Masks = mask,
                MasksId = mask.MasksId
            };

            var transactions = new List<PurchaseHistory>
        {
            new PurchaseHistory
            {
                Customer = customer1,
                CustomerId = customer1.CustomerId,
                PharmacyMasks = pharmacyMask,
                PharmacyMasksId = pharmacyMask.PharmacyMasksId,
                TransactionAmount = 10,
                TransactionQuantity = 2,
                TransactionDatetime = DateTime.Parse("2025-11-20")
            },
            new PurchaseHistory
            {
                Customer = customer2,
                CustomerId = customer2.CustomerId,
                PharmacyMasks = pharmacyMask,
                PharmacyMasksId = pharmacyMask.PharmacyMasksId,
                TransactionAmount = 5,
                TransactionQuantity = 1,
                TransactionDatetime = DateTime.Parse("2025-11-20")
            }
        };

            context.Customers.AddRange(customer1, customer2);
            context.Pharmacies.Add(pharmacy);
            context.Masks.Add(mask);
            context.PharmacyMasks.Add(pharmacyMask);
            context.PurchaseHistories.AddRange(transactions);
            context.SaveChanges();
        }

        private PurchaseHistoryService CreateService(KDAN_TESTContext context)
        {
            var unitOfWork = new UnitOfWork(context);
            var purchaseHistoryRepository = new GenericRepository<PurchaseHistory>(context);
            return new PurchaseHistoryService(unitOfWork, purchaseHistoryRepository);
        }

        // ============================
        // 1. 成功取得前幾名客戶
        // ============================
        [Fact]
        public async Task GetTopCustomerPurchaseHistory_ReturnsTopCustomers()
        {
            var context = CreateInMemoryDbContext();
            SeedData(context);
            var service = CreateService(context);

            var result = await service.GetTopCustomerPurchaseHistory("2025/11/20", "2025/11/20", 2);

            Assert.True(result.Result);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal("Alice", result.Data[0].Name); // Alice 金額最大
            Assert.Equal(20, result.Data[0].TransactionAmount); // 10 * 2
            Assert.Equal(5, result.Data[1].TransactionAmount);  // Bob 5*1
        }

        // ============================
        // 2. 查無資料 → 空集合
        // ============================
        [Fact]
        public async Task GetTopCustomerPurchaseHistory_NoData_ReturnsEmpty()
        {
            var context = CreateInMemoryDbContext();
            var service = CreateService(context);

            var result = await service.GetTopCustomerPurchaseHistory("2025/11/20", "2025/11/20", 5);

            Assert.True(result.Result);
            Assert.Empty(result.Data);
        }

        // ============================
        // 3. 非法輸入 → Result = false
        // ============================
        [Fact]
        public async Task GetTopCustomerPurchaseHistory_InvalidDate_ReturnsFalse()
        {
            var context = CreateInMemoryDbContext();
            var service = CreateService(context);

            var result = await service.GetTopCustomerPurchaseHistory("invalid", "2025/11/20", 5);

            Assert.False(result.Result);
        }
    }
}
