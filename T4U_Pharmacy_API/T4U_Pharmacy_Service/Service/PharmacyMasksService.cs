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
        /// 取得指定藥局下的口罩資料
        /// </summary>
        /// <param name="pharmacyId">藥局ID</param>
        /// <param name="sortBy">排序欄位，只接受name、price</param>
        /// <param name="sortOrder">排序方式，只接受asc、desc</param>
        /// <returns></returns>
        public async Task<PharmacyMasksViewModel> GetPharmacyMasksList(long pharmacyId, PharmacyMasksSortBy sortBy = PharmacyMasksSortBy.Name, SortOrderEnum sortOrder = SortOrderEnum.Asc)
        {
            PharmacyMasksViewModel obj = new PharmacyMasksViewModel();
            obj.Result = true;
            try
            {
                var pharmacies = this.repository.GetAll().Where(n => n.PharmacyId == pharmacyId);
                var list = await pharmacies.Select(n => new { n.Masks, n.Pharmacy, n.PharmacyMasksStockLogs, n.Price }).ToListAsync();

                if(list.Count > 0)
                {
                    obj.PharmacyId = list.FirstOrDefault().Pharmacy.PharmacyId;
                    obj.Name = list.FirstOrDefault().Pharmacy.Name;
                    var listDetail = list.Select(
                        n => new PharmacyMasksDetail 
                        {
                            MasksId = n.Masks.MasksId,
                            Name = n.Masks.Name,
                            Price = n.Price,
                            StockQuantity = n.PharmacyMasksStockLogs.Sum(s => s.StockQuantity)
                        }
                        ).ToList();
                    switch (sortBy)
                    {
                        case PharmacyMasksSortBy.Price:
                            listDetail = sortOrder == SortOrderEnum.Desc
                                ? listDetail.OrderByDescending(n => n.Price).ToList()
                                : listDetail.OrderBy(n => n.Price).ToList();
                            break;

                        case PharmacyMasksSortBy.Name:
                            listDetail = sortOrder == SortOrderEnum.Desc
                                ? listDetail.OrderByDescending(n => n.Name).ToList()
                                : listDetail.OrderBy(n => n.Name).ToList();
                            break;
                    }
                    obj.Data = listDetail;
                }
            }
            catch (Exception ex)
            {
                string message = "發生意外錯誤";
                obj.Result = false;
                obj.Message = message;
            }
            return obj;
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
        public async Task<MaskDetailViewModel> GetPharmaciesByMaskStock(decimal? priceMin, decimal? priceMax, int? quantityMin, int? quantityMax, QuantityThresholdType quantityThresholdType = QuantityThresholdType.Above)
        {
            MaskDetailViewModel obj = new MaskDetailViewModel();
            obj.Result = true;
            try
            {
                var pharmacies = this.repository.GetAll();
                if (priceMin.HasValue)
                {
                    pharmacies = pharmacies.Where(n =>  n.Price >= priceMin.Value);
                }
                if (priceMax.HasValue)
                {
                    pharmacies = pharmacies.Where(n => n.Price <= priceMax.Value);
                }
                switch(quantityThresholdType)
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
                    ).ToListAsync();
                obj.Data = details;
            }
            catch (Exception ex)
            {
                string message = "發生意外錯誤";
                obj.Result = false;
                obj.Message = message;
            }
            return obj;
        }

    }
}
