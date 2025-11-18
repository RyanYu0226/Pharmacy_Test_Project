using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T4U_Pharmacy_Common_Model.ViewModel;
using T4U_Pharmacy_Common_Model;
using T4U_Pharmacy_Repository.DBModels;
using Microsoft.EntityFrameworkCore;

namespace T4U_Pharmacy_Service
{
    public class PurchaseHandleService
    {
        private CustomerService _customerService;
        private PharmacyService _pharmacyService;
        private MasksService _masksService;
        private PharmacyMasksService _pharmacyMasksService;
        private PurchaseHistoryService _purchaseHistoryService;
        private SystemConfigService _systemConfigService;
        private PharmacyMasksStockLogService _pharmacyMasksStockLogService;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="customerService"></param>
        /// <param name="pharmacyService"></param>
        /// <param name="masksService"></param>
        /// <param name="pharmacyMasksService"></param>
        /// <param name="purchaseHistoryService"></param>
        public PurchaseHandleService(CustomerService customerService, PharmacyService pharmacyService, MasksService masksService, PharmacyMasksService pharmacyMasksService, PurchaseHistoryService purchaseHistoryService, SystemConfigService systemConfigService, PharmacyMasksStockLogService pharmacyMasksStockLogService)
        {
            _customerService = customerService;
            _pharmacyService = pharmacyService;
            _masksService = masksService;
            _pharmacyMasksService = pharmacyMasksService;
            _purchaseHistoryService = purchaseHistoryService;
            _systemConfigService = systemConfigService;
            _pharmacyMasksStockLogService = pharmacyMasksStockLogService;
        }

        /// <summary>
        /// 購買藥局口罩
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<ResultViewModel> InsertPurchaseHistory(PharmacyOrdersInput input)
        {
            ResultViewModel result = new ResultViewModel();
            result.Message = ValidPharmacyOrdersInput(input);
            result.Result = true;
            if(string.IsNullOrEmpty(result.Message))
            {
                var customer = _customerService.Get(n => n.CustomerId == input.CustomerId);
                if (customer != null)
                {
                    //寫入購買紀錄
                    var listPharmacyId = input.PharmacyOrders.Select(n => n.PharmacyId).Distinct().ToList();
                    var listPharmacyMask = await _pharmacyMasksService.GetAll()
                        .Where(n => listPharmacyId.Contains(n.PharmacyId)).Select(n => new { n.PharmacyMasksId ,n.PharmacyId, n.MasksId, n.Price })
                        .ToListAsync();
                    foreach(var order in input.PharmacyOrders)
                    {
                        foreach(var item in order.Items)
                        {
                            var pharmacyMask = listPharmacyMask.FirstOrDefault(n => n.PharmacyId == order.PharmacyId && n.MasksId == item.MasksId);
                            var insertPurchase = new PurchaseHistory();
                            insertPurchase.PharmacyMasksId = pharmacyMask.PharmacyMasksId;
                            insertPurchase.TransactionQuantity = item.Quantity;
                            insertPurchase.TransactionAmount = pharmacyMask.Price;
                            insertPurchase.CustomerId = input.CustomerId;
                            insertPurchase.CreatedDate = DateTime.Now;
                            insertPurchase.TransactionDatetime = DateTime.Now;
                            _purchaseHistoryService.Add(insertPurchase);
                        }
                    }

                    //寫入庫存紀錄
                    foreach (var order in input.PharmacyOrders)
                    {
                        foreach (var item in order.Items)
                        {
                            var pharmacyMask = listPharmacyMask.FirstOrDefault(n => n.PharmacyId == order.PharmacyId && n.MasksId == item.MasksId);
                            var insertPharmacyMasksStockLog = new PharmacyMasksStockLog();
                            insertPharmacyMasksStockLog.PharmacyMasksId = pharmacyMask.PharmacyMasksId;
                            insertPharmacyMasksStockLog.StockQuantity = -item.Quantity;
                            insertPharmacyMasksStockLog.Price = pharmacyMask.Price;
                            insertPharmacyMasksStockLog.CreatedDate = DateTime.Now;
                            _pharmacyMasksStockLogService.Add(insertPharmacyMasksStockLog);
                        }
                    }
                }
            }
            else
            {
                result.Result = false;
            }
            return result;
        }

        /// <summary>
        /// 檢核PharmacyOrdersInput內容
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string ValidPharmacyOrdersInput(PharmacyOrdersInput input)
        {
            string errorMessage = string.Empty;
            var customer = this._customerService.Get(n => n.CustomerId == input.CustomerId);
            //確認客戶存在
            if (customer == null)
            {
                return "客戶不存在";
            }
            //確認藥局存在
            var listPharmacyId = input.PharmacyOrders.Select(n => n.PharmacyId).Distinct().ToList();
            var pharmacies = _pharmacyService.GetAll().Where(n => listPharmacyId.Contains(n.PharmacyId)).Select(n => n.PharmacyId).ToList();
            foreach(var po in input.PharmacyOrders)
            {
                if(!pharmacies.Any(n => n == po.PharmacyId))
                {
                    return string.Format("沒有對應藥局，PharmacyId：{0}", po.PharmacyId);
                }
            }

            //確認購買的藥局口罩存在
            var listPharmacyMask = this._pharmacyMasksService.GetAll()
                .Where(n => listPharmacyId.Contains(n.PharmacyId)).Select(n => new { n.PharmacyId,n.MasksId, n.Price, TotalQuantity = n.PharmacyMasksStockLogs.Sum(s => s.StockQuantity)})
                .ToList();
            foreach(var po in input.PharmacyOrders)
            {
                foreach(var mod in po.Items)
                {
                    var inst = listPharmacyMask.FirstOrDefault(n => n.PharmacyId == po.PharmacyId && n.MasksId == mod.MasksId);
                    if (inst == null)
                    {
                        return string.Format("對應藥局沒有此口罩，PharmacyId：{0}，MasksId：{1}", po.PharmacyId, mod.MasksId);
                    }
                }
            }

            //確認藥局口罩數量足夠
            foreach(var pId in listPharmacyId)
            {
                var pharmacyOrders = input.PharmacyOrders.Where(n => n.PharmacyId == pId).ToList();
                Dictionary<long, int> dicMaskQuantity = new Dictionary<long, int>();
                foreach(var order in pharmacyOrders)
                {
                    foreach(var item in order.Items) 
                    {
                        if(dicMaskQuantity.ContainsKey(item.MasksId))
                        {
                            dicMaskQuantity[item.MasksId] += item.Quantity;
                        }
                        else
                        {
                            dicMaskQuantity.Add(item.MasksId, item.Quantity);
                        }
                    }
                }
                foreach(var key in  dicMaskQuantity.Keys)
                {
                    var inst = listPharmacyMask.FirstOrDefault(n => n.PharmacyId == pId && n.MasksId == key);
                    if (dicMaskQuantity[key] > inst.TotalQuantity)
                    {
                        return string.Format("此藥局口罩數量不足，PharmacyId：{0}，MasksId：{1}，目前數量：{2}，購買數量：{3}", pId, key, inst.TotalQuantity, dicMaskQuantity[key]);
                    }
                }
            }

            //計算總價是否超過客戶剩餘金額
            var currentCash = GetCustomerCurrentCashBalance(input.CustomerId);
            decimal totalAmount = 0;
            foreach (var po in input.PharmacyOrders)
            {
                foreach (var mod in po.Items)
                {
                    var inst = listPharmacyMask.FirstOrDefault(n => n.PharmacyId == po.PharmacyId && n.MasksId == mod.MasksId);
                    if (inst != null)
                    {
                        totalAmount += inst.Price * mod.Quantity;
                    }
                }
            }
            if(totalAmount > currentCash)
            {
                return string.Format("購買金額超過客戶剩餘金額，客戶金額：{0}，購買款項：{1}", currentCash, totalAmount);
            }


            return errorMessage;
        }

        /// <summary>
        /// 取得客戶目前剩餘金額
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public decimal GetCustomerCurrentCashBalance(long customerId)
        {
            decimal currentCash = 0;
            var customer = this._customerService.Get(n => n.CustomerId == customerId);
            string strUserCashEndDate = _systemConfigService.GetValue(SystemConfigService.USER_CASH_BALANCE_SETTLEMENT_TIME);
            if (!string.IsNullOrEmpty(strUserCashEndDate))
            {
                var dtUserCashEndDate = DateTime.Parse(strUserCashEndDate).AddDays(1);
                var purchaseHistories = _purchaseHistoryService.GetAll().Where(n => n.CustomerId == customerId && n.TransactionDatetime > dtUserCashEndDate).ToList();
                var sumTotal = purchaseHistories.Sum(n => n.TransactionAmount * n.TransactionQuantity);
                currentCash = customer.CurrentCashBalance - sumTotal;
            }
            else
            {
                currentCash = customer.CurrentCashBalance;
            }
            return currentCash;
        }
    }
}
