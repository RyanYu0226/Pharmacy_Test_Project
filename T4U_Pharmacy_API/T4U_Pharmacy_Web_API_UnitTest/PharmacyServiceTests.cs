using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using T4U_Pharmacy_Repository.DBModels;
using T4U_Pharmacy_Repository.Infrastructure;
using T4U_Pharmacy_Repository.Interface;
using T4U_Pharmacy_Service;
using Microsoft.EntityFrameworkCore;
using T4U_Pharmacy_Repository.Implement;

namespace T4U_Pharmacy_Web_API_UnitTest
{
    public class PharmacyServiceTests
    {
        private KDAN_TESTContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<KDAN_TESTContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
                .Options;
            return new KDAN_TESTContext(options);
        }

        private PharmacyService _pharmacyService;

        #region Core Test Cases

        [Fact]
        public async Task GetPharmacies_NoFilters_ReturnsAllPharmacies()
        {
            using var context = CreateInMemoryDbContext();
            context.Pharmacies.AddRange(GetTestPharmacies());
            context.SaveChanges();

            var repository = new GenericRepository<Pharmacy>(context);
            var unitOfWork = new UnitOfWork(context);
            _pharmacyService = new PharmacyService(unitOfWork, repository);

            var result = await _pharmacyService.GetPharmacies("", "");

            Assert.True(result.Result);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPharmacies_FilterByTime_ReturnsCorrectPharmacies()
        {
            using var context = CreateInMemoryDbContext();
            context.Pharmacies.AddRange(GetTestPharmacies());
            context.SaveChanges();

            var repository = new GenericRepository<Pharmacy>(context);
            var unitOfWork = new UnitOfWork(context);
            _pharmacyService = new PharmacyService(unitOfWork, repository);

            // 上午10:00
            var morningResult = await _pharmacyService.GetPharmacies("", "10:00");
            Assert.True(morningResult.Result);
            Assert.Equal(2, morningResult.Data.Count);

            // 夜間22:30
            var nightResult = await _pharmacyService.GetPharmacies("", "22:30");
            Assert.True(nightResult.Result);
            Assert.Single(nightResult.Data);
            Assert.Equal("藥局B", nightResult.Data[0].Name);

            // 凌晨01:00
            var earlyResult = await _pharmacyService.GetPharmacies("", "01:00");
            Assert.True(earlyResult.Result);
            Assert.Single(earlyResult.Data);
            Assert.Equal("藥局B", earlyResult.Data[0].Name);
        }

        [Fact]
        public async Task GetPharmacies_NoMatchingPharmacies_ReturnsEmptyList()
        {
            using var context = CreateInMemoryDbContext();
            context.Pharmacies.AddRange(GetTestPharmacies());
            context.SaveChanges();

            var repository = new GenericRepository<Pharmacy>(context);
            var unitOfWork = new UnitOfWork(context);
            _pharmacyService = new PharmacyService(unitOfWork, repository);

            var result = await _pharmacyService.GetPharmacies("6", "23:00"); // 星期六夜晚
            Assert.True(result.Result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPharmacies_EmptyDatabase_ReturnsEmptyList()
        {
            using var context = CreateInMemoryDbContext();
            context.Pharmacies.AddRange(new List<Pharmacy>());
            context.SaveChanges();

            var repository = new GenericRepository<Pharmacy>(context);
            var unitOfWork = new UnitOfWork(context);
            _pharmacyService = new PharmacyService(unitOfWork, repository);

            var result = await _pharmacyService.GetPharmacies("", "");
            Assert.True(result.Result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPharmacies_RepositoryThrowsException_ReturnsFalseResult()
        {
            using var context = CreateInMemoryDbContext();
            var repository = new GenericRepository<Pharmacy>(context);
            var unitOfWork = new UnitOfWork(context);
            _pharmacyService = new PharmacyService(unitOfWork, repository);

            // 模擬異常
            var mockRepo = new Mock<IGenericRepository<Pharmacy>>();
            mockRepo.Setup(r => r.GetAll()).Throws(new Exception("DB Error"));
            _pharmacyService = new PharmacyService(unitOfWork, mockRepo.Object);

            var result = await _pharmacyService.GetPharmacies("", "");
            Assert.False(result.Result);
            Assert.Equal("發生意外錯誤", result.Message);
        }

        #endregion

        #region Helper Methods

        private List<Pharmacy> GetTestPharmacies()
        {
            return new List<Pharmacy>
        {
            new Pharmacy
            {
                PharmacyId = 1,
                Name = "藥局A",
                CashBalance = 10000,
                CurrentCashBalance = 10000,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                PharmacyOpeningHours = new List<PharmacyOpeningHour>
                {
                    new PharmacyOpeningHour
                    {
                        OpingHoursId = 1,
                        PharmacyId = 1,
                        Weekday = 1,
                        OpenTime = "09:00",
                        CloseTime = "17:59"
                    },
                    new PharmacyOpeningHour
                    {
                        OpingHoursId = 2,
                        PharmacyId = 1,
                        Weekday = 2,
                        OpenTime = "09:00",
                        CloseTime = "17:59"
                    }
                },
                PharmacyMasks = new List<PharmacyMask>()
            },
            new Pharmacy
            {
                PharmacyId = 2,
                Name = "藥局B",
                CashBalance = 20000,
                CurrentCashBalance = 20000,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                PharmacyOpeningHours = new List<PharmacyOpeningHour>
                {
                    new PharmacyOpeningHour
                    {
                        OpingHoursId = 3,
                        PharmacyId = 2,
                        Weekday = 1,
                        OpenTime = "09:00",
                        CloseTime = "18:00"
                    },
                    new PharmacyOpeningHour
                    {
                        OpingHoursId = 4,
                        PharmacyId = 2,
                        Weekday = 2,
                        OpenTime = "22:00",
                        CloseTime = "05:00"
                    }
                },
                PharmacyMasks = new List<PharmacyMask>()
            }
        };
        }

        #endregion
    }
}