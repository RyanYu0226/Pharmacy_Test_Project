using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using T4U_Pharmacy_Repository.Interface;

namespace T4U_Pharmacy_Repository.Infrastructure
{
    /// <summary>
    /// UnitOfWork 介面
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// DB Context
        /// </summary>
        DbContext Context { get; }

        /// <summary>
        /// Saves the change.
        /// </summary>
        /// <returns></returns>
        int SaveChange();

        //This Method will Start the database Transaction
        void CreateTransaction();
        //This Method will Commit the database Transaction
        void Commit();
        //This Method will Rollback the database Transaction
        void Rollback();
    }
}
