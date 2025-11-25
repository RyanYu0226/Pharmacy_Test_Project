using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4U_Pharmacy_Common_Model.ViewModel
{
    /// <summary>
    /// 結果列表
    /// </summary>
    public class ListResultViewModel : ResultViewModel
    {
        /// <summary>
        /// 分頁資訊
        /// </summary>
        [Required]
        public PaginationInfo pageInfo { get; set; }

        /// <summary>
        /// 取得分頁資訊
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalCount"></param>
        /// <returns></returns>
        public PaginationInfo GetPageInfo(int pageIndex, int pageSize, int totalCount)
        {
            int totalPage = totalCount / pageSize;
            if (totalCount % pageSize > 0) totalPage += 1;
            PaginationInfo result = new PaginationInfo
            {
                Page = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPage = totalPage
            };
            return result;
        }
    }

    /// <summary>
    /// 分頁資訊
    /// </summary>
    public class PaginationInfo
    {
        /// <summary>
        /// 目前第幾頁
        /// </summary>
        [Required]
        public int Page { get; set; }
        /// <summary>
        /// 每頁幾筆
        /// </summary>
        [Required]
        public int PageSize { get; set; }
        /// <summary>
        /// 總筆數
        /// </summary>
        [Required]
        public int TotalCount { get;set; }
        /// <summary>
        /// 總頁數
        /// </summary>
        [Required]
        public int TotalPage { get; set; }
    }
}
