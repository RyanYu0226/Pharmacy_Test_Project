using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using T4U_Pharmacy_Common_Model;
using T4U_Pharmacy_Common_Model.ViewModel;
using T4U_Pharmacy_Service;
using T4U_Pharmacy_Service.BaseService;

namespace T4U_Pharmacy_Web_API.Controllers
{
    public class PurchaseController : BaseController
    {
        private PurchaseHandleService _purchaseHandleService;

        public PurchaseController(PurchaseHandleService purchaseHandelService)
        {
            _purchaseHandleService = purchaseHandelService;
        }

        /// <summary>
        /// 處理一筆購買交易，讓使用者可同時向多間藥局購買口罩 API
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("v1/[controller]/InsertPurchaseHistory")]
        public async Task<ActionResult<ResultViewModel>> InsertPurchaseHistory([FromBody][Required] PharmacyOrdersInput input)
        {
            return await _purchaseHandleService.InsertPurchaseHistory(input);
        }
    }
}
