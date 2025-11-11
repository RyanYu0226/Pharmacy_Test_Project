using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using T4U_Pharmacy_Repository.Infrastructure;
using T4U_Pharmacy_Repository.Interface;

namespace T4U_Pharmacy_Service.BaseService
{
    public abstract class BaseService<TEntity> : IBaseService<TEntity> where TEntity : class
    {
        private readonly IUnitOfWork unitofwork;
        protected IGenericRepository<TEntity> repository { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlogService"/> class.
        /// </summary>
        public BaseService(IUnitOfWork unitofwork, IGenericRepository<TEntity> repository)
        {
            this.unitofwork = unitofwork;
            this.repository = repository;
        }

        /// <summary>
        /// 新增 
        /// </summary>
        /// <param name="blogDto">Blog Dto</param>
        /// <returns></returns>
        public int Add(TEntity entity)
        {
            this.repository.Add(entity);
            return this.unitofwork.SaveChange();
        }

        public int AddRange(IEnumerable<TEntity> entity)
        {
            this.repository.AddAll(entity);
            return this.unitofwork.SaveChange();
        }

        /// <summary>
        /// 取得單筆
        /// </summary>
        /// <param name="blogQueryDto">查詢條件</param>
        /// <returns></returns>
        public TEntity Get(Expression<Func<TEntity, bool>> predicate)
        {
            var result = this.repository.Get(predicate);
            return result;
        }

        /// <summary>
        /// 取得全部
        /// </summary>
        /// <param name="blogQueryDto">查詢條件</param>
        /// <returns></returns>
        public IQueryable<TEntity> GetAll()
        {
            var result = this.repository.GetAll();
            return result;
        }

        /// <summary>
        /// 刪除 
        /// </summary>
        /// <param name="id">news Id</param>
        /// <returns></returns>
        public int Remove(Expression<Func<TEntity, bool>> predicate)
        {
            var result = this.repository.Get(predicate);
            this.repository.Remove(result);
            return this.unitofwork.SaveChange();
        }

        public int RemoveAll(Expression<Func<TEntity, bool>> predicate)
        {
            var result = this.repository.Get(predicate);
            this.repository.Remove(result);
            return this.unitofwork.SaveChange();
        }

        /// <summary>
        /// 修改 
        /// </summary>
        /// <param name="blogDto">Blog Dto</param>
        /// <returns></returns>
        public int Update(TEntity entity)
        {
            this.repository.Update(entity);
            return this.unitofwork.SaveChange();
        }

        public int UpdateRange(IEnumerable<TEntity> entity)
        {
            this.repository.UpdateAll(entity);
            return this.unitofwork.SaveChange();
        }
    }
}
