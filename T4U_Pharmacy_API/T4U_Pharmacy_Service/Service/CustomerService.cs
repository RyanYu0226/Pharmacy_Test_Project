using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T4U_Pharmacy_Repository.DBModels;
using T4U_Pharmacy_Repository.Infrastructure;
using T4U_Pharmacy_Repository.Interface;
using T4U_Pharmacy_Service.BaseService;

namespace T4U_Pharmacy_Service
{
    public class CustomerService: BaseService<Customer>
    {
        private IUnitOfWork _unitOfWork; 
        public CustomerService(IUnitOfWork unitOfWork, IGenericRepository<Customer> repository)
             : base(unitOfWork, repository)
        {
            _unitOfWork = unitOfWork;
        }
    }
}
