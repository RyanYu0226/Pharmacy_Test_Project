using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using T4U_Pharmacy_Common_Model.ViewModel;
using T4U_Pharmacy_Service;

namespace T4U_Pharmacy_Web_API.Controllers
{
    /// <summary>
    /// 客戶
    /// </summary>
    public class CustomerController : BaseController
    {
        private PurchaseHistoryService _purchaseHistoryService;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="purchaseHistoryService"></param>
        public CustomerController(PurchaseHistoryService purchaseHistoryService)
        {
            _purchaseHistoryService = purchaseHistoryService;
        }

        /// <summary>
        /// 顯示在指定日期區間內，購買口罩金額最高的前 N 名使用者 API
        /// </summary>
        /// <param name="startDate">開始日期</param>
        /// <param name="endDate">結束日期</param>
        /// <param name="limit">購買口罩金額最高的前 N 名使用者</param>
        /// <returns></returns>
        [HttpGet("v1/[controller]/GetTopCustomerPurchaseHistory")]
        public async Task<ActionResult<CustomerPurchaseHistoryViewModel>> GetTopCustomerPurchaseHistory([FromQuery][Required] string startDate, [FromQuery][Required] string endDate, [FromQuery][Required] int limit)
        {
            var data = await _purchaseHistoryService.GetTopCustomerPurchaseHistory(startDate, endDate, limit);
            return Ok(data);
        }
    }
}
