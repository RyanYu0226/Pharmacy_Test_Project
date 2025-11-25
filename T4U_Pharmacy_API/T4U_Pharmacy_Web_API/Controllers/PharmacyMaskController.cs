using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Formats.Asn1;
using T4U_Pharmacy_Common_Model;
using T4U_Pharmacy_Common_Model.Common;
using T4U_Pharmacy_Common_Model.ViewModel;
using T4U_Pharmacy_Service;

namespace T4U_Pharmacy_Web_API.Controllers
{
    /// <summary>
    /// 藥局口罩
    /// </summary>
    public class PharmacyMaskController : BaseController
    {
        private PharmacyMaskHandleService _pharmacyMaskHandleService;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="pharmacyMaskHandleService"></param>
        public PharmacyMaskController(PharmacyMaskHandleService pharmacyMaskHandleService)
        {
            _pharmacyMaskHandleService = pharmacyMaskHandleService;
        }

        /// <summary>
        /// 透過關鍵字搜尋藥局和口罩
        /// </summary>
        /// <param name="keyword">關鍵字</param>
        /// <param name="pharmacyMaskSearchType">
        /// 類型
        /// 全部，All = 0
        /// 藥局，Pharmacy = 1
        /// 口罩，Mask = 2
        /// </param>
        /// <param name="page">第幾頁</param>
        /// <param name="pageSize">每頁幾筆</param>
        /// <returns></returns>
        [HttpGet("v1/[controller]/SearchPharmacyMaskByKeyword")]
        public async Task<ActionResult<PharmacyMaskSearchResultViewModel>> SearchPharmacyMaskByKeyword([FromQuery][Required]string keyword, [FromQuery][Required]PharmacyMaskSearchType pharmacyMaskSearchType = PharmacyMaskSearchType.All,[FromQuery][Required] int page = 1, [FromQuery][Required] int pageSize = 5)
        {
            var result = await _pharmacyMaskHandleService.SearchPharmacyMaskByKeyword(keyword, pharmacyMaskSearchType, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// 更新藥局口罩價格庫存
        /// </summary>
        /// <param name="pharmacyId">藥局ID</param>
        /// <param name="input">口罩資訊</param>
        /// <returns></returns>
        [HttpPost("v1/[controller]/UpdateMaskData")]
        public async Task<ActionResult<PharmacyMasksViewModel>> UpdateMaskData([FromQuery][Required]long pharmacyId, [FromBody][Required]UpdateMultiPharmacyMaskInput input)
        {
            var result = await _pharmacyMaskHandleService.UpdateMaskData(pharmacyId, input);
            return Ok(result);
        }
    }
}
