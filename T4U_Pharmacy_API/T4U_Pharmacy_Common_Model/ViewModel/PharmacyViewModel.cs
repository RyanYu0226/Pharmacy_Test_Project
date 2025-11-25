using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4U_Pharmacy_Common_Model.ViewModel
{
    public class PharmacyViewModel: ListResultViewModel
    {
        public List<PharmacyDetail> Data { get; set; }
    }

    /// <summary>
    /// 藥局資料
    /// </summary>
    public class PharmacyDetail
    {
        /// <summary>
        /// 序號
        /// </summary>
        public long PharmacyId { get; set; }

        /// <summary>
        /// 藥局名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;

        public List<PharmacyOpenTimeDetail>? OpenTimeList { get; set; }
    }

    public class PharmacyOpenTimeDetail
    {
        /// <summary>
        /// 營業日
        /// </summary>
        public string WeekDaya { get; set; } = string.Empty;
        /// <summary>
        /// 開門時間
        /// </summary>
        public string OpenTime { get; set; } = string.Empty;
        /// <summary>
        /// 關門時間
        /// </summary>
        public string CloseTime { get; set; } = string.Empty;
    }
}
