using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T4U_Pharmacy_Common_Model;
using T4U_Pharmacy_Common_Model.ViewModel;
using T4U_Pharmacy_Repository.DBModels;

namespace T4U_Pharmacy_Service
{
    public class MaskHandleService
    {
        private PharmacyService _pharmacyService;
        private PharmacyMasksService _pharmacyMasksService;
        private PharmacyMasksStockLogService _pharmacyMasksStockLogService;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="pharmacyService"></param>
        /// <param name="pharmacyMasksService"></param>
        /// <param name="pharmacyMasksStockLogService"></param>
        public MaskHandleService(PharmacyService pharmacyService, PharmacyMasksService pharmacyMasksService, PharmacyMasksStockLogService pharmacyMasksStockLogService)
        {
            _pharmacyService = pharmacyService;
            _pharmacyMasksService = pharmacyMasksService;
            _pharmacyMasksStockLogService = pharmacyMasksStockLogService;
        }    

        /// <summary>
        /// 更新口罩庫存
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<MaskDetailViewModel> UpdateMaskQuantity(UpdateMaskInput input)
        {
            MaskDetailViewModel result = new MaskDetailViewModel();
            result.Message = ValidUpdateMaskQuantity(input);
            result.Result = true;
            if(string.IsNullOrEmpty(result.Message))
            {
                var pharmacyMask = _pharmacyMasksService.GetAll()
                    .Include(n => n.Pharmacy).Include(n => n.Masks)
                    .FirstOrDefault(n => n.PharmacyId == input.PharmacyId && n.MasksId == input.MasksId);
                var stock = new PharmacyMasksStockLog()
                {
                    PharmacyMasksId = pharmacyMask.PharmacyMasksId,
                    StockQuantity = input.Delta,
                    Price = pharmacyMask.Price,
                    CreatedDate = DateTime.Now
                };
                _pharmacyMasksStockLogService.Add(stock);

                var nowStock = await _pharmacyMasksStockLogService.GetAll().Where(n => n.PharmacyMasksId == pharmacyMask.PharmacyMasksId).ToListAsync();
                
                result.Data = new List<MaskDetail>();
                result.Data.Add(new MaskDetail 
                {
                    PharmacyId = input.PharmacyId,
                    PharmacyName = pharmacyMask.Pharmacy.Name,
                    MasksId = input.MasksId,
                    MasksName = pharmacyMask.Masks.Name,
                    Price = pharmacyMask.Price,
                    StockQuantity = nowStock.Sum(n => n.StockQuantity)
                });
            }
            else
            {
                result.Result = false;
            }
            return result;
        }

        /// <summary>
        /// 更新口罩庫存檢驗
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string ValidUpdateMaskQuantity(UpdateMaskInput input)
        {
            string errorMessage = string.Empty;
            //檢查藥局是否存在
            var pharmacy = _pharmacyService.Get(n => n.PharmacyId == input.PharmacyId);
            if(pharmacy == null)
            {
                return string.Format("無此藥局，PharmacyId：{0}", input.PharmacyId);
            }

            //檢查藥局是否有此對應口罩
            var pharmacyMask = _pharmacyMasksService.Get(n => n.PharmacyId == input.PharmacyId && n.MasksId == input.MasksId);
            if(pharmacyMask == null)
            {
                return string.Format("此藥局無對應口罩，PharmacyId：{0}，MasksId：{1}", input.PharmacyId, input.MasksId);
            }

            //檢查口罩數量是否被扣除到負數
            if (input.Delta < 0)
            {
                long nowQuantity = GetPharmacyMaskQuantity(input.PharmacyId, input.MasksId);
                if(-input.Delta > nowQuantity)
                {
                    return string.Format("此藥局口罩庫存目前為{0}，無法刪除到負數", nowQuantity);
                }
            }

            return errorMessage;
        }

        /// <summary>
        /// 取得對應藥局口罩庫存
        /// </summary>
        /// <param name="pharmacyId">藥局ID</param>
        /// <param name="masksId">口罩ID</param>
        /// <returns></returns>
        public long GetPharmacyMaskQuantity(long pharmacyId, long masksId)
        {
            long result = 0;
            var pharmacyMask = _pharmacyMasksService.Get(n => n.PharmacyId == pharmacyId && n.MasksId == masksId);
            if(pharmacyMask != null) 
            {
                result = _pharmacyMasksStockLogService.GetAll().Where(n => n.PharmacyMasksId == pharmacyMask.PharmacyMasksId).Sum(n => n.StockQuantity); 
            }
            return result;
        }
    }
}
