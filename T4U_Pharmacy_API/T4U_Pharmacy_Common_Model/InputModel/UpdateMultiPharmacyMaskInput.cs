using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4U_Pharmacy_Common_Model
{
    /// <summary>
    /// 更新口罩內容Input
    /// </summary>
    public class UpdateMultiPharmacyMaskInput
    {
        /// <summary>
        /// 更新口罩內容
        /// </summary>
        [Required]
        public List<UpdateMultiPharmacyMask> Masks { get; set; }
    }

    /// <summary>
    /// 更新口罩內容
    /// </summary>
    public class UpdateMultiPharmacyMask
    {
        /// <summary>
        /// 口罩名稱
        /// </summary>
        [Required]
        public string MaskName { get; set; }
        /// <summary>
        /// 價格
        /// </summary>
        [Required]
        public decimal Price { get; set; }
        /// <summary>
        /// 庫存
        /// </summary>
        [Required]
        public long Stock { get; set; }
    }
}
