using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T4U_Pharmacy_Common_Model.ViewModel;
using T4U_Pharmacy_Repository.DBModels;
using T4U_Pharmacy_Repository.Infrastructure;
using T4U_Pharmacy_Repository.Interface;
using T4U_Pharmacy_Service.BaseService;

namespace T4U_Pharmacy_Service
{
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
        public async Task<PharmacyMasksViewModel> GetPharmacyMasksList(long pharmacyId, string sortBy = "name", string sortOrder = "asc")
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
                    switch (sortBy?.ToLower())
                    {
                        case "price":
                            listDetail = sortOrder?.ToLower() == "desc"
                                ? listDetail.OrderByDescending(n => n.Price).ToList()
                                : listDetail.OrderBy(n => n.Price).ToList();
                            break;

                        case "name":
                        default:
                            listDetail = sortOrder?.ToLower() == "desc"
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

        private List<string> _sortNameList = new List<string>() { "name", "price"};

    }
}
