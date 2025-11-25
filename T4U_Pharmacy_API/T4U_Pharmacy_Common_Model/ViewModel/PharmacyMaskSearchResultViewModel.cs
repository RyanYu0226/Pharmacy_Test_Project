using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4U_Pharmacy_Common_Model.ViewModel
{
    /// <summary>
    /// 藥局口罩搜尋結果ViewModel
    /// </summary>
    public class PharmacyMaskSearchResultViewModel : ListResultViewModel
    {
        /// <summary>
        /// 資料
        /// </summary>
        public List<PharmacyMaskSearchResult> Data { get; set; }
    }

    /// <summary>
    /// 藥局口罩搜尋結果內容
    /// </summary>
    public class PharmacyMaskSearchResult
    {
        /// <summary>
        /// 類型，Pharmacy：藥局，Mask：口罩
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// ID
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 名稱
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 關聯性
        /// </summary>
        public double Relevance { get; set; }
    }
}
