using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using T4U_Pharmacy_Common_Model;
using T4U_Pharmacy_Common_Model.ViewModel;
using T4U_Pharmacy_Service;
using static T4U_Pharmacy_Common_Model.Common.CommonStruct;

namespace T4U_Pharmacy_Web_API.Controllers
{
    /// <summary>
    /// 口罩
    /// </summary>
    public class MasksController : BaseController
    {
        private PharmacyMasksService _pharmacyMasksService;
        private MaskHandleService _maskHandleService;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="pharmacyMasksService"></param>
        public MasksController(PharmacyMasksService pharmacyMasksService, MaskHandleService maskHandleService)
        {
            _pharmacyMasksService = pharmacyMasksService;
            _maskHandleService = maskHandleService;
        }

        /// <summary>
        /// 取得指定藥局下的口罩資料
        /// </summary>
        /// <param name="pharmacyId">藥局ID</param>
        /// <param name="sortBy">排序欄位，0:口罩名稱、1:價錢</param>
        /// <param name="sortOrder">排序方式，0:asc、1:desc</param>
        /// <returns></returns>
        [HttpGet("v1/[controller]/GetPharmacyMasksList")]
        public async Task<ActionResult<PharmacyMasksViewModel>> GetPharmacyMasksList([FromQuery][Required] long pharmacyId, [FromQuery][Required] PharmacyMasksSortBy sortBy = PharmacyMasksSortBy.Name, [FromQuery][Required] SortOrderEnum sortOrder = SortOrderEnum.Asc)
        {
            var data = await _pharmacyMasksService.GetPharmacyMasksList(pharmacyId, sortBy, sortOrder);
            return Ok(data);
        }

        /// <summary>
        /// 更新藥局口罩庫存
        /// </summary>
        /// <param name="input">更新口罩庫存Input</param>
        /// <returns></returns>
        [HttpPut("v1/[controller]/UpdateMaskQuantity")]
        public async Task<ActionResult<MaskDetailViewModel>> UpdateMaskQuantity([FromBody][Required]UpdateMaskInput input)
        {
            return await _maskHandleService.UpdateMaskQuantity(input);
        }
    }
}
