using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4U_Pharmacy_Service.BaseService
{
    public interface IBaseService<TEntity>
    {
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns></returns>
        int Add(TEntity entity);

        int AddRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// 取得單筆
        /// </summary>
        /// <param name="predicate">查詢條件</param>
        /// <returns></returns>
        TEntity Get(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 取得全部
        /// </summary>
        /// <returns></returns>
        IQueryable<TEntity> GetAll();

        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns></returns>
        int Remove(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns></returns>
        int Update(TEntity entity);

        int UpdateRange(IEnumerable<TEntity> entities);

    }
}
