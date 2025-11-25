using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using T4U_Pharmacy_Common_Model.ViewModel;
using T4U_Pharmacy_Service;
using static T4U_Pharmacy_Common_Model.Common.CommonStruct;

namespace T4U_Pharmacy_Web_API.Controllers
{
    /// <summary>
    /// 藥局
    /// </summary>
    public class PharmacyController : BaseController
    {
        private PharmacyService _pharmacyService;
        private PharmacyMasksService _pharmacyMasksService;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="pharmacyService"></param>
        /// <param name="pharmacyMasksService"></param>
        public PharmacyController(PharmacyService pharmacyService, PharmacyMasksService pharmacyMasksService)
        {
            _pharmacyService = pharmacyService;
            _pharmacyMasksService = pharmacyMasksService;
        }

        /// <summary>
        /// 取得藥局資訊
        /// </summary>
        /// <param name="dayOfWeek">營業日，0~6，EX: 0 => 星期日、1 => 星期一...</param>
        /// <param name="hourTime">營業時間，格式HH:mm，24小時制，EX: 09:05</param>
        /// <param name="queryType">查詢條件類型，0：or，1：and</param>
        /// <param name="page">目前顯示第幾頁</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns></returns>
        [HttpGet("v1/[controller]/GetPharmacies")]
        public async Task<ActionResult<PharmacyViewModel>> GetPharmacies([FromQuery] string dayOfWeek = "", [FromQuery] string hourTime = "", [FromQuery] QueryType queryType = QueryType.Or, [FromQuery][Required] int page = 1, [FromQuery][Required] int pageSize = 5)
        {
            var data = await _pharmacyService.GetPharmacies(dayOfWeek, hourTime, queryType, page, pageSize);
            return Ok(data);
        }

        /// <summary>
        /// 取得販售口罩數量在特定價格區間內的所有藥局
        /// </summary>
        /// <param name="priceMin">最低價錢</param>
        /// <param name="priceMax">最高價錢</param>
        /// <param name="quantityMin">口罩數量下限</param>
        /// <param name="quantityMax">口罩數量上限</param>
        /// <param name="quantityThresholdType">
        /// 口罩數量，0：高於、1：低於、2：之間
        /// 選擇高於，則quantityMin必填
        /// 選擇低於，則quantityMax必填
        /// 選擇之間，則quantityMin和quantityMax必填
        /// </param>
        /// <param name="page">第幾頁</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns></returns>
        [HttpGet("v1/[controller]/GetPharmaciesByMaskStock")]
        public async Task<ActionResult<MaskDetailViewModel>>GetPharmaciesByMaskStock([FromQuery][Required] decimal priceMin, [FromQuery][Required] decimal priceMax, [FromQuery] int? quantityMin, [FromQuery] int? quantityMax, [FromQuery][Required] QuantityThresholdType quantityThresholdType = QuantityThresholdType.Above, int page = 1, int pageSize = 5)
        {
            var data = await _pharmacyMasksService.GetPharmaciesByMaskStock(priceMin, priceMax, quantityMin, quantityMax, quantityThresholdType, page, pageSize);
            return Ok(data);
        }
    }
}
