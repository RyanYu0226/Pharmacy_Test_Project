using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4U_Pharmacy_Common_Model.ViewModel
{
    /// <summary>
    /// 客戶交易紀錄ViewModel
    /// </summary>
    public class CustomerPurchaseHistoryViewModel : ResultViewModel
    {
        /// <summary>
        /// 開始時間
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// 結束時間
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// 回傳購買金額前幾名 
        /// </summary>
        public int Limit { get; set; }
        /// <summary>
        /// 客戶資料
        /// </summary>
        public List<CustomerPurchaseHistory>? Data { get; set; }
    }

    /// <summary>
    /// 客戶交易紀錄
    /// </summary>
    public class CustomerPurchaseHistory
    {
        /// <summary>
        /// 序號
        /// </summary>
        public long CustomerId { get; set; }

        /// <summary>
        /// 客戶名稱
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 時間範圍內交易總金額
        /// </summary>
        public decimal TransactionAmount { get; set; }

        /// <summary>
        /// 交易紀錄內容
        /// </summary>
        public List<TransactionDetail> Details { get; set; }
    }

    /// <summary>
    /// 交易紀錄內容
    /// </summary>
    public class TransactionDetail
    {
        /// <summary>
        /// 客戶ID
        /// </summary>
        public long CustomerId { get; set; }
        /// <summary>
        /// 藥局ID
        /// </summary>
        public long PharmacyId { get; set; }

        /// <summary>
        /// 藥局名稱
        /// </summary>
        public string PharmacyName { get; set; }

        /// <summary>
        /// 口罩ID
        /// </summary>
        public long MasksId { get; set; }

        /// <summary>
        /// 口罩名稱
        /// </summary>
        public string MasksName { get; set; }

        /// <summary>
        /// 藥局口罩ID
        /// </summary>
        public long PharmacyMasksId { get; set; }

        /// <summary>
        /// 交易金額
        /// </summary>
        public decimal TransactionAmount { get; set; }

        /// <summary>
        /// 交易數量
        /// </summary>
        public long TransactionQuantity { get; set; }

        /// <summary>
        /// 交易時間
        /// </summary>
        public DateTime TransactionDatetime { get; set; }
    }
}
