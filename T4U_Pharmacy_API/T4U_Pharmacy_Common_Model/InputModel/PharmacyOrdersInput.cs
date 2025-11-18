using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4U_Pharmacy_Common_Model
{
    /// <summary>
    /// 購買藥局口罩Input
    /// </summary>
    public class PharmacyOrdersInput
    {
        /// <summary>
        /// 客戶ID
        /// </summary>
        [Required]
        public long CustomerId { get; set; }
        /// <summary>
        /// 購買內容
        /// </summary>
        [Required]
        public required List<PharmacyOrder> PharmacyOrders { get; set; }

    }

    /// <summary>
    /// 購買內容
    /// </summary>
    public class PharmacyOrder
    {
        /// <summary>
        /// 藥局ID
        /// </summary>
        [Required]
        public long PharmacyId { get; set; }
        /// <summary>
        /// 購買口罩內容
        /// </summary>
        [Required]
        public required List<MaskOrderDetail> Items { get; set; }
    }

    /// <summary>
    /// 購買口罩內容
    /// </summary>
    public class MaskOrderDetail
    {
        /// <summary>
        /// 口罩ID
        /// </summary>
        [Required]
        public long MasksId { get; set; }
        /// <summary>
        /// 購買數量
        /// </summary>
        [Required]
        public int Quantity { get; set; }
    }
}
