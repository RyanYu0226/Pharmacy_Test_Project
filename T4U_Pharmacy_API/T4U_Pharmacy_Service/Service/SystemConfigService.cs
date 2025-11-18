using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T4U_Pharmacy_Repository.DBModels;
using T4U_Pharmacy_Repository.Infrastructure;
using T4U_Pharmacy_Repository.Interface;
using T4U_Pharmacy_Service.BaseService;

namespace T4U_Pharmacy_Service
{
    public class SystemConfigService : BaseService<SystemConfig>
    {
        private IUnitOfWork _unitOfWork;
        public SystemConfigService(IUnitOfWork unitOfWork, IGenericRepository<SystemConfig> repository)
             : base(unitOfWork, repository)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 客戶金額結算日期
        /// </summary>
        public const string USER_CASH_BALANCE_SETTLEMENT_TIME = "USER_CASH_BALANCE_SETTLEMENT_TIME";
        /// <summary>
        /// 藥局金額結算日期
        /// </summary>
        public const string PHARMACY_STOCK_SETTLEMENT_TIME = "PHARMACY_STOCK_SETTLEMENT_TIME";

        /// <summary>
        /// 取得設定值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(string key)
        {
            string value = string.Empty;
            var inst = this.Get(n => n.Key == key);
            if (inst != null)
            {
                value = inst.Value;
            }
            return value;
        }
    }
}
