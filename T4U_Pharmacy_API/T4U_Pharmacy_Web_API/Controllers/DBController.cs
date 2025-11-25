using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using T4U_Pharmacy_Common_Model.ViewModel;
using T4U_Pharmacy_Service;

namespace T4U_Pharmacy_Web_API.Controllers
{
    public class DBController : BaseController
    {
        private DBService _dbService;

        public DBController(DBService dbService)
        {
            _dbService = dbService;
        }

        /// <summary>
        /// 初始化資料庫資料
        /// </summary>
        /// <returns></returns>
        [HttpPost("v1/[controller]/InitDBData")]
        public async Task<ActionResult<ResultViewModel>> InitDBData()
        {
            string path = "DbJsonData";
            string pathForSaving = Path.Combine(AppContext.BaseDirectory, path);
            string pharmaciesJsonPath = Path.Combine(pathForSaving, "pharmacies.json");
            string usersJsonPath = Path.Combine(pathForSaving, "users.json");
            var result = await _dbService.ImportDataAsync(pharmaciesJsonPath, usersJsonPath, true);
            return Ok(result);
        }
    }
}
