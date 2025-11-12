using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using T4U_Pharmacy_Common_Model.ViewModel;
using T4U_Pharmacy_Service;
using static T4U_Pharmacy_Common_Model.Common.CommonStruct;

namespace T4U_Pharmacy_Web_API.Controllers
{
    public class MasksController : BaseController
    {
        private PharmacyMasksService _pharmacyMasksService;

        public MasksController(PharmacyMasksService pharmacyMasksService)
        {
            _pharmacyMasksService = pharmacyMasksService;
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
    }
}
