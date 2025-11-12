using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4U_Pharmacy_Common_Model.ViewModel
{
    public class PharmacyMasksViewModel : ResultViewModel
    {
        /// <summary>
        /// 序號
        /// </summary>
        public long PharmacyId { get; set; }

        /// <summary>
        /// 藥局名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 列表資料
        /// </summary>
        public List<PharmacyMasksDetail>? Data { get; set; }
    }

    public class PharmacyMasksDetail
    {
        /// <summary>
        /// 口罩ID(外鍵)MASKS的MASKS_ID
        /// </summary>
        public long MasksId { get; set; }

        /// <summary>
        /// 口罩名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;

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
