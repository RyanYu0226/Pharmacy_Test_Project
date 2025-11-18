using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4U_Pharmacy_Common_Model
{
    /// <summary>
    /// 更新口罩庫存Input
    /// </summary>
    public class UpdateMaskInput
    {
        /// <summary>
        /// 藥局ID
        /// </summary>
        [Required]
        public long PharmacyId { get; set; }
        /// <summary>
        /// 口罩ID
        /// </summary>
        [Required]
        public long MasksId { get; set; }
        /// <summary>
        /// 調整數量 => 正數=增加，負數=減少
        /// </summary>
        [Required]
        public int Delta { get; set; }
    }
}
