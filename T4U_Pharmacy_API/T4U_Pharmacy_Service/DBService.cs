using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using T4U_Pharmacy_Common_Model.Common;
using T4U_Pharmacy_Common_Model.ViewModel;
using T4U_Pharmacy_Repository.DBModels;

namespace T4U_Pharmacy_Service
{
    public class DBService
    {
        private record MaskInput(string name, decimal price, int stockQuantity);
        private record PharmacyInput(string name, decimal cashBalance, string openingHours, List<MaskInput> masks);
        private record PurchaseHistoryInput(string pharmacyName, string maskName, decimal transactionAmount, int transactionQuantity, string transactionDatetime);
        private record CustomerInput(string name, decimal cashBalance, List<PurchaseHistoryInput> purchaseHistories);

        private readonly KDAN_TESTContext _context;

        public DBService(KDAN_TESTContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 匯入資料
        /// </summary>
        /// <param name="_context">EF Core Db_context</param>
        /// <param name="pharmaciesJsonPath">藥局 JSON 路徑</param>
        /// <param name="usersJsonPath">使用者 JSON 路徑</param>
        /// <param name="clearExistingData">是否清空舊資料</param>
        public async Task<ResultViewModel> ImportDataAsync(
            string pharmaciesJsonPath,
            string usersJsonPath,
            bool clearExistingData = false)
        {
            var result = new ResultViewModel();
            if (clearExistingData)
            {
                Console.WriteLine("⚠️ 清空舊資料中...");
                await using var tranClear = await _context.Database.BeginTransactionAsync();

                try
                {
                    // TRUNCATE 順序：先子表，再父表
                    await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"PURCHASE_HISTORIES\" CASCADE;");
                    await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"PHARMACY_MASKS_STOCK_LOG\" CASCADE;");
                    await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"PHARMACY_OPENING_HOURS\" CASCADE;");
                    await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"PHARMACY_MASKS\" CASCADE;");
                    await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"MASKS\" CASCADE;");
                    await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"CUSTOMER\" CASCADE;");
                    await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"PHARMACY\" CASCADE;");
                    await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"SYSTEM_CONFIG\" CASCADE;");

                    await _context.Database.ExecuteSqlRawAsync("ALTER SEQUENCE \"PURCHASE_HISTORIES_PURCHASE_HISTORIES_ID_seq\" RESTART WITH 1;");
                    await _context.Database.ExecuteSqlRawAsync("ALTER SEQUENCE \"PHARMACY_MASKS_STOCK_LOG_PHARMACY_MASKS_STOCK_LOG_ID_seq\" RESTART WITH 1;");
                    await _context.Database.ExecuteSqlRawAsync("ALTER SEQUENCE \"PHARMACY_OPENING_HOURS_OPING_HOURS_ID_seq\" RESTART WITH 1;");
                    await _context.Database.ExecuteSqlRawAsync("ALTER SEQUENCE \"PHARMACY_MASKS_PHARMACY_MASKS_ID_seq\" RESTART WITH 1;");
                    await _context.Database.ExecuteSqlRawAsync("ALTER SEQUENCE \"MASKS_MASKS_ID_seq\" RESTART WITH 1;");
                    await _context.Database.ExecuteSqlRawAsync("ALTER SEQUENCE \"CUSTOMER_CUSTOMER_ID_seq\" RESTART WITH 1;");
                    await _context.Database.ExecuteSqlRawAsync("ALTER SEQUENCE \"PHARMACY_PHARMACY_ID_seq\" RESTART WITH 1;");
                    await _context.Database.ExecuteSqlRawAsync("ALTER SEQUENCE \"SYSTEM_CONFIG_SYSTEM_CONFIG_ID_seq\" RESTART WITH 1;");

                    await tranClear.CommitAsync();
                    Console.WriteLine("✅ 舊資料已清空");
                }
                catch (Exception ex)
                {
                    await tranClear.RollbackAsync();
                    result.Result = false;
                    result.Message = "❌ 清空資料失敗：" + ex.Message;
                    return result;
                }
            }

            // 讀取 JSON
            var pharmaciesData = JsonSerializer.Deserialize<List<PharmacyInput>>(await File.ReadAllTextAsync(pharmaciesJsonPath))!;
            var usersData = JsonSerializer.Deserialize<List<CustomerInput>>(await File.ReadAllTextAsync(usersJsonPath))!;

            await using var tran = await _context.Database.BeginTransactionAsync();

            try
            {
                // ------------------------
                // 匯入藥局、口罩、營業時間、庫存
                // ------------------------
                foreach (var p in pharmaciesData)
                {
                    var pharmacy = await _context.Pharmacies
                        .FirstOrDefaultAsync(x => x.Name == p.name)
                        ?? new Pharmacy
                        {
                            Name = p.name,
                            CashBalance = p.cashBalance,
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now
                        };

                    if (pharmacy.PharmacyId == 0)
                        _context.Pharmacies.Add(pharmacy);

                    var hours = Regex.Split(p.openingHours, @"\s*,\s*");
                    foreach (var h in hours)
                    {
                        var match = Regex.Match(h, @"(\w+)\s+(\d{2}:\d{2})\s*-\s*(\d{2}:\d{2})");
                        if (match.Success)
                        {
                            var weekday = (int)(WeekDayMap[match.Groups[1].Value]);
                            var openTime = match.Groups[2].Value;
                            var closeTime = match.Groups[3].Value;

                            if (!_context.PharmacyOpeningHours.Any(x =>
                                x.PharmacyId == pharmacy.PharmacyId &&
                                x.Weekday == weekday &&
                                x.OpenTime == openTime &&
                                x.CloseTime == closeTime))
                            {
                                _context.PharmacyOpeningHours.Add(new PharmacyOpeningHour
                                {
                                    Pharmacy = pharmacy,
                                    Weekday = weekday,
                                    OpenTime = openTime,
                                    CloseTime = closeTime
                                });
                            }
                        }
                    }

                    foreach (var m in p.masks)
                    {
                        var mask = await _context.Masks.FirstOrDefaultAsync(x => x.Name == m.name)
                                   ?? new Mask { Name = m.name };
                        if (mask.MasksId == 0)
                            _context.Masks.Add(mask);

                        await _context.SaveChangesAsync();

                        var pharmacyMask = await _context.PharmacyMasks.FirstOrDefaultAsync(x =>
                            x.PharmacyId == pharmacy.PharmacyId && x.MasksId == mask.MasksId)
                            ?? new PharmacyMask
                            {
                                Pharmacy = pharmacy,
                                Masks = mask,
                                Price = m.price,
                                CreatedDate = DateTime.Now,
                                ModifiedDate = DateTime.Now
                            };

                        if (pharmacyMask.PharmacyMasksId == 0)
                            _context.PharmacyMasks.Add(pharmacyMask);

                        _context.PharmacyMasksStockLogs.Add(new PharmacyMasksStockLog
                        {
                            PharmacyMasks = pharmacyMask,
                            StockQuantity = m.stockQuantity,
                            Price = m.price,
                            CreatedDate = DateTime.Now
                        });

                        await _context.SaveChangesAsync();
                    }
                }

                // ------------------------
                // 匯入使用者與購買紀錄
                // ------------------------
                foreach (var u in usersData)
                {
                    var customer = await _context.Customers
                        .FirstOrDefaultAsync(x => x.Name == u.name)
                        ?? new Customer
                        {
                            Name = u.name,
                            CashBalance = u.cashBalance,
                            CurrentCashBalance = u.cashBalance,
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now
                        };
                    if (customer.CustomerId == 0)
                        _context.Customers.Add(customer);

                    await _context.SaveChangesAsync();

                    foreach (var ph in u.purchaseHistories)
                    {
                        // 找到對應的 PharmacyMask
                        var pharmacyMask = await _context.PharmacyMasks
                            .Include(pm => pm.Pharmacy)
                            .Include(pm => pm.Masks)
                            .FirstOrDefaultAsync(pm =>
                                pm.Pharmacy.Name == ph.pharmacyName &&
                                pm.Masks.Name == ph.maskName);

                        if (pharmacyMask != null)
                        {
                            _context.PurchaseHistories.Add(new PurchaseHistory
                            {
                                Customer = customer,
                                PharmacyMasks = pharmacyMask,  // ✅ 對應 PharmacyMask
                                TransactionAmount = ph.transactionAmount,
                                TransactionQuantity = ph.transactionQuantity,
                                TransactionDatetime = DateTime.Parse(ph.transactionDatetime),
                                CreatedDate = DateTime.Now
                            });
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ 找不到對應 PharmacyMask: {ph.pharmacyName} / {ph.maskName}");
                        }
                    }
                }

                // ------------------------
                // 匯入系統設定
                // ------------------------
                _context.SystemConfigs.Add(new SystemConfig
                {
                    Key = SystemConfigKey.USER_CASH_BALANCE_SETTLEMENT_TIME,
                    Value = DateTime.Today.AddDays(1).ToString("yyyy/MM/dd"),
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                });
                _context.SystemConfigs.Add(new SystemConfig
                {
                    Key = SystemConfigKey.PHARMACY_STOCK_SETTLEMENT_TIME,
                    Value = DateTime.Today.AddDays(1).ToString("yyyy/MM/dd"),
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                });

                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                Console.WriteLine("✅ 匯入完成！");
                result.Result = true;
                return result;
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                result.Result = false;
                result.Message = "❌ 清空資料失敗：" + ex.Message;
                return result;
            }
        }

        private readonly Dictionary<string, DayOfWeek> WeekDayMap = new()
        {
            { "Sun", DayOfWeek.Sunday },
            { "Mon", DayOfWeek.Monday },
            { "Tue", DayOfWeek.Tuesday },
            { "Wed", DayOfWeek.Wednesday },
            { "Thur", DayOfWeek.Thursday },
            { "Fri", DayOfWeek.Friday },
            { "Sat", DayOfWeek.Saturday }
        };
    }
}
