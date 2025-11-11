using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace T4U_Pharmacy_Repository.Interface
{
    /// <summary>
    /// Interface Repository
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="entity">實體</param>
        void Add(TEntity entity);
        /// <summary>
        /// 新增多筆
        /// </summary>
        /// <param name="entities"></param>
        void AddAll(IEnumerable<TEntity> entities);

        /// <summary>
        /// 取得全部
        /// </summary>
        /// <returns></returns>
        IQueryable<TEntity> GetAll();

        /// <summary>
        /// 取得單筆
        /// </summary>
        /// <param name="predicate">查詢條件</param>
        /// <returns></returns>
        TEntity Get(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="entity">實體</param>
        void Remove(TEntity entity);

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity">實體</param>
        void Update(TEntity entity);

        /// <summary>
        /// 更新多筆
        /// </summary>
        /// <param name="entities"></param>
        void UpdateAll(IEnumerable<TEntity> entities);
    }
}
