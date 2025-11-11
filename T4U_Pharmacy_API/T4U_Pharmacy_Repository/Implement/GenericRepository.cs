using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using T4U_Pharmacy_Repository.DBModels;
using T4U_Pharmacy_Repository.Interface;

namespace T4U_Pharmacy_Repository.Implement
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// UnitOfWork 實體
        /// </summary>
        private readonly KDAN_TESTContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRepository{TEntity}"/> class.
        /// </summary>
        /// <param name="context">db context.</param>
        public GenericRepository(KDAN_TESTContext context)
        {
            this._context = context;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="entity">實體</param>
        /// <exception cref="ArgumentNullException">entity</exception>
        public void Add(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            _context.Set<TEntity>().Add(entity);
        }

        public void AddAll(IEnumerable<TEntity> entities)
        {
            if (entities == null || entities.Count() == 0)
            {
                throw new ArgumentNullException("entities");
            }

            _context.Set<TEntity>().AddRange(entities);
        }

        /// <summary>
        /// 取得全部
        /// </summary>
        /// <returns></returns>
        public IQueryable<TEntity> GetAll()
        {
            return this._context.Set<TEntity>();
        }



        /// <summary>
        /// 取得單筆
        /// </summary>
        /// <param name="predicate">查詢條件</param>
        /// <returns></returns>
        public TEntity Get(Expression<Func<TEntity, bool>> predicate)
        {
            return this._context.Set<TEntity>().FirstOrDefault(predicate);
        }

        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="entity">實體</param>
        /// <exception cref="ArgumentNullException">entity</exception>
        public void Remove(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            this._context.Entry(entity).State = EntityState.Deleted;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity">實體</param>
        /// <exception cref="ArgumentNullException">entity</exception>
        public void Update(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            this._context.Entry(entity).State = EntityState.Modified;
        }

        public void UpdateAll(IEnumerable<TEntity> entities)
        {
            if (entities == null || entities.Count() == 0)
            {
                throw new ArgumentNullException("entities");
            }

            _context.Set<TEntity>().UpdateRange(entities);
        }
    }
}
