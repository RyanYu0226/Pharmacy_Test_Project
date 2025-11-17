using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4U_Pharmacy_Common_Model.ViewModel
{
    public class MaskDetailViewModel : ResultViewModel
    {
        public List<MaskDetail>? Data { get; set; }
    }

    public class MaskDetail
    {
        /// <summary>
        /// 藥局ID
        /// </summary>
        public long PharmacyId { get; set; }

        /// <summary>
        /// 藥局名稱
        /// </summary>
        public string PharmacyName { get; set; } = string.Empty;

        /// <summary>
        /// 口罩ID(外鍵)MASKS的MASKS_ID
        /// </summary>
        public long MasksId { get; set; }

        /// <summary>
        /// 口罩名稱
        /// </summary>
        public string MasksName { get; set; } = string.Empty;

        /// <summary>
        /// 單價
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 口罩數量
        /// </summary>
        public long StockQuantity { get; set; }
    }
}
