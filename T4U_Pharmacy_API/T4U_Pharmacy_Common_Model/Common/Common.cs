using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4U_Pharmacy_Common_Model.Common
{
    /// <summary>
    /// 常用資訊
    /// </summary>
    public class CommonStruct
    {
        /// <summary>
        /// 藥局口罩排序欄位
        /// </summary>
        public enum PharmacyMasksSortBy
        {
            /// <summary>
            /// 口罩名稱
            /// </summary>
            Name = 0,
            /// <summary>
            /// 單價
            /// </summary>
            Price = 1
        }

        /// <summary>
        /// 排序方式
        /// </summary>
        public enum SortOrderEnum
        {
            /// <summary>
            /// 升序排序
            /// </summary>
            Asc = 0,
            /// <summary>
            /// 降序排序
            /// </summary>
            Desc = 1
        }
    }
    /// <summary>
    /// 系統設定KEY
    /// </summary>
    public class SystemConfigKey
    {
        /// <summary>
        /// 使用者餘額結算時間
        /// </summary>
        public const string USER_CASH_BALANCE_SETTLEMENT_TIME = "USER_CASH_BALANCE_SETTLEMENT_TIME";
        /// <summary>
        /// 藥局餘額結算時間
        /// </summary>
        public const string PHARMACY_STOCK_SETTLEMENT_TIME = "PHARMACY_STOCK_SETTLEMENT_TIME";
    }
}
