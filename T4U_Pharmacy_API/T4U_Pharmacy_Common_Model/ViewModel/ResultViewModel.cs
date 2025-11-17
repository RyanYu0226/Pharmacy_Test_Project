using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4U_Pharmacy_Common_Model.ViewModel
{
    /// <summary>
    /// 回傳用ViewModel
    /// </summary>
    public class ResultViewModel
    {
        /// <summary>
        /// 結果
        /// </summary>
        public bool Result { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string Message { get; set; }
    }
}
