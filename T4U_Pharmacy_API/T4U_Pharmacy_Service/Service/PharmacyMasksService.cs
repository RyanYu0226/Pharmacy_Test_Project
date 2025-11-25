using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T4U_Pharmacy_Common_Model.Common;
using T4U_Pharmacy_Common_Model.ViewModel;
using T4U_Pharmacy_Repository.DBModels;
using T4U_Pharmacy_Repository.Infrastructure;
using T4U_Pharmacy_Repository.Interface;
using T4U_Pharmacy_Service.BaseService;
using static T4U_Pharmacy_Common_Model.Common.CommonStruct;

namespace T4U_Pharmacy_Service
{
    /// <summary>
    /// 藥局口罩Service
    /// </summary>
    public class PharmacyMasksService: BaseService<PharmacyMask>
    {
        private IUnitOfWork _unitOfWork;
        public PharmacyMasksService(IUnitOfWork unitOfWork, IGenericRepository<PharmacyMask> repository)
             : base(unitOfWork, repository)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 查詢符合查詢條件的藥局+口罩資料
        /// </summary>
        /// <param name="priceMin">口罩價格區間下限</param>
        /// <param name="priceMax">口罩價格區間上限</param>
        /// <param name="quantityThresholdType">口罩數量門檻類型</param>
        /// <param name="quantityMin">口罩數量下限</param>
        /// <param name="quantityMax">口罩數量上限</param>
        /// <returns></returns>
        public async Task<MaskDetailViewModel> GetPharmaciesByMaskStock(decimal? priceMin, decimal? priceMax, int? quantityMin, int? quantityMax, QuantityThresholdType quantityThresholdType = QuantityThresholdType.Above, int page = 1, int pageSize = 5)
        {
            MaskDetailViewModel obj = new MaskDetailViewModel();
            obj.Result = true;
            obj.Message = VaildGetPharmaciesByMaskStock(quantityMin, quantityMax, quantityThresholdType);
            if (string.IsNullOrEmpty(obj.Message))
            {
                try
                {
                    var pharmacies = this.repository.GetAll();
                    if (priceMin.HasValue)
                    {
                        pharmacies = pharmacies.Where(n => n.Price >= priceMin.Value);
                    }
                    if (priceMax.HasValue)
                    {
                        pharmacies = pharmacies.Where(n => n.Price <= priceMax.Value);
                    }
                    switch (quantityThresholdType)
                    {
                        case QuantityThresholdType.Above:
                            pharmacies = pharmacies.Where(n => n.PharmacyMasksStockLogs.Sum(s => s.StockQuantity) >= quantityMin.Value);
                            break;
                        case QuantityThresholdType.Below:
                            pharmacies = pharmacies.Where(n => n.PharmacyMasksStockLogs.Sum(s => s.StockQuantity) <= quantityMax.Value);
                            break;
                        case QuantityThresholdType.Between:
                            pharmacies = pharmacies.Where(n => n.PharmacyMasksStockLogs.Sum(s => s.StockQuantity) >= quantityMin.Value && n.PharmacyMasksStockLogs.Sum(s => s.StockQuantity) <= quantityMax.Value);
                            break;
                    }

                    var totalCount = pharmacies.Count();

                    var details = await pharmacies.Select(
                        n => new MaskDetail
                        {
                            PharmacyId = n.PharmacyId,
                            PharmacyName = n.Pharmacy.Name,
                            MasksId = n.MasksId,
                            MasksName = n.Masks.Name,
                            Price = n.Price,
                            StockQuantity = n.PharmacyMasksStockLogs.Sum(s => s.StockQuantity)
                        }
                        ).Skip((page -1) * pageSize).Take(pageSize)
                        .ToListAsync();
                    obj.Data = details;
                    obj.pageInfo = new ListResultViewModel().GetPageInfo(page,pageSize,totalCount);
                }
                catch (Exception ex)
                {
                    string message = "發生意外錯誤";
                    obj.Result = false;
                    obj.Message = message;
                }
            }
            else
            {
                obj.Result = false;
            }
            return obj;
        }

        /// <summary>
        /// 檢驗GetPharmaciesByMaskStock Input
        /// </summary>
        /// <param name="quantityMin"></param>
        /// <param name="quantityMax"></param>
        /// <param name="quantityThresholdType"></param>
        /// <returns></returns>
        private string VaildGetPharmaciesByMaskStock(int? quantityMin, int? quantityMax, QuantityThresholdType quantityThresholdType = QuantityThresholdType.Above)
        {
            string result = string.Empty;
            if (quantityThresholdType == QuantityThresholdType.Above && !quantityMin.HasValue)
            {
                return "quantityMin為必填";
            }
            if (quantityThresholdType == QuantityThresholdType.Below && !quantityMax.HasValue)
            {
                return "quantityMax為必填";
            }
            if (quantityThresholdType == QuantityThresholdType.Between && !quantityMin.HasValue && !quantityMax.HasValue)
            {
                return "quantityMin和quantityMax為必填";
            }
            return result;
        }
    }
}
