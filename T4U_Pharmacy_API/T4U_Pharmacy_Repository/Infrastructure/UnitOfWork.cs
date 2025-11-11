    using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using T4U_Pharmacy_Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using T4U_Pharmacy_Repository.DBModels;

namespace T4U_Pharmacy_Repository.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private bool disposed = false;
        private IDbContextTransaction? _objTran = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="blogRepository">The blog repository.</param>
        public UnitOfWork(
            KDAN_TESTContext context)
        {
            this.Context = context;
        }

        /// <summary>
        /// Context
        /// </summary>
        public DbContext Context { get; private set; }


        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// SaveChange
        /// </summary>
        /// <returns></returns>
        public int SaveChange()
        {
            return this.Context.SaveChanges();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.Context.Dispose();
                    this.Context = null;
                }
            }
            this.disposed = true;
        }

        public void CreateTransaction()
        {
            _objTran = this.Context.Database.BeginTransaction();
        }
        public void Commit()
        {
            _objTran?.Commit();
        }

        public void Rollback()
        {
            _objTran?.Rollback();
            _objTran?.Dispose();
        }
    }
}
