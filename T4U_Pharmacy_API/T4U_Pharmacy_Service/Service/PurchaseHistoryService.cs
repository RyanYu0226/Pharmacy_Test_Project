using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using T4U_Pharmacy_Common_Model;
using T4U_Pharmacy_Common_Model.ViewModel;
using T4U_Pharmacy_Repository.DBModels;
using T4U_Pharmacy_Repository.Infrastructure;
using T4U_Pharmacy_Repository.Interface;
using T4U_Pharmacy_Service.BaseService;

namespace T4U_Pharmacy_Service
{
    /// <summary>
    /// 交易紀錄Service
    /// </summary>
    public class PurchaseHistoryService: BaseService<PurchaseHistory>
    {
        private IUnitOfWork _unitOfWork;
        public PurchaseHistoryService(IUnitOfWork unitOfWork, IGenericRepository<PurchaseHistory> repository)
             : base(unitOfWork, repository)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 取得指定時間內的購買金額前幾名客戶
        /// </summary>
        /// <param name="startDate">開始時間，格式：yyyy/MM/dd</param>
        /// <param name="endDate">結束時間，格式：yyyy/MM/dd</param>
        /// <param name="limit">前幾名</param>
        /// <returns></returns>
        public async Task<CustomerPurchaseHistoryViewModel> GetTopCustomerPurchaseHistory(string startDate, string endDate, int limit)
        {
            CustomerPurchaseHistoryViewModel result = new CustomerPurchaseHistoryViewModel();
            result.Message = GetTopCustomerPurchaseHistoryValidate(startDate, endDate, limit);
            result.Result = string.IsNullOrEmpty(result.Message);
            try 
            {
                if(result.Result)
                {
                    DateTime dtStart = DateTime.Parse(startDate);
                    DateTime dtEnd = DateTime.Parse(endDate).AddDays(1);
                    result.StartDate = dtStart;
                    result.EndDate = dtEnd.AddDays(-1);
                    result.Limit = limit;
                    var purchaseHistories = this.repository.GetAll().Where(n => n.TransactionDatetime >= dtStart && n.TransactionDatetime < dtEnd);
                    var listPurchaseHistories = await purchaseHistories
                        .GroupBy(p => new
                        {
                            p.CustomerId,
                            p.Customer.Name,
                        })
                        .Select(g => new CustomerPurchaseHistory
                        {
                            CustomerId = g.Key.CustomerId,
                            Name = g.Key.Name,
                            TransactionAmount = g.Sum(x => x.TransactionAmount * x.TransactionQuantity)
                        })
                        .OrderByDescending(x => x.TransactionAmount)
                        .Take(limit)
                        .ToListAsync();
                    var listCustomerId = listPurchaseHistories.Select(x => x.CustomerId).ToList();
                    var listDetail = purchaseHistories
                        .Where(n => listCustomerId.Contains(n.CustomerId))
                        .Select(n =>
                        new TransactionDetail 
                        {
                            CustomerId = n.CustomerId,
                            PharmacyId = n.PharmacyMasks.PharmacyId,
                            PharmacyName = n.PharmacyMasks.Pharmacy.Name,
                            MasksId = n.PharmacyMasks.MasksId,
                            MasksName = n.PharmacyMasks.Masks.Name,
                            PharmacyMasksId = n.PharmacyMasksId,
                            TransactionAmount = n.TransactionAmount,
                            TransactionQuantity = n.TransactionQuantity,
                            TransactionDatetime = n.TransactionDatetime
                        })
                        .ToList();
                    foreach(var p in listPurchaseHistories)
                    {
                        p.Details = listDetail.Where(n => n.CustomerId == p.CustomerId).ToList();
                    }
                    result.Data = listPurchaseHistories;
                }
            }
            catch(Exception ex) { }
            return result;
        }

        /// <summary>
        /// 取得指定時間內的購買金額前幾名客戶檢驗
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        private string GetTopCustomerPurchaseHistoryValidate(string startDate, string endDate, int limit)
        {
            string errorMessage = string.Empty;
            if(!DateTime.TryParse(startDate, out DateTime dtStart))
            {
                errorMessage = "startDate格式不符，格式：yyyy/MM/dd";
            }
            else if (!DateTime.TryParse(endDate, out DateTime dtEnd))
            {
                errorMessage = "endDate格式不符，格式：yyyy/MM/dd";
            }
            else if(dtStart > dtEnd)
            {
                errorMessage = "startDate必須小於等於endDate";
            }
            else if(limit <= 0)
            {
                errorMessage = "limit必須為大於0的正整數";
            }
            return errorMessage;
        }
    }
}
