using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using T4U_Pharmacy_Common_Model.ViewModel;
using T4U_Pharmacy_Service;

namespace T4U_Pharmacy_Web_API.Controllers
{
    public class PharmacyController : BaseController
    {
        private PharmacyService _pharmacyService;

        public PharmacyController(PharmacyService pharmacyService)
        {
            _pharmacyService = pharmacyService;
        }

        /// <summary>
        /// 取得藥局資訊
        /// </summary>
        /// <param name="dayOfWeek">營業日，0~6，EX: 0 => 星期日、1 => 星期一...</param>
        /// <param name="hourTime">營業時間，格式HH:mm，24小時制，EX: 09:05</param>
        /// <returns></returns>
        [HttpGet("v1/[controller]/GetPharmacies")]
        public async Task<ActionResult<PharmacyViewModel>> GetPharmacies([FromQuery] string dayOfWeek = "", [FromQuery]string hourTime = "")
        {
            var data = await _pharmacyService.GetPharmacies(dayOfWeek, hourTime);
            return Ok(data);
        }
    }
}
