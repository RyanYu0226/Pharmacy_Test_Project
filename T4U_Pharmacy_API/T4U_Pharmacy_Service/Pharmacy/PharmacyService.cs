using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T4U_Pharmacy_Common_Model.ViewModel;
using T4U_Pharmacy_Repository.DBModels;
using T4U_Pharmacy_Repository.Infrastructure;
using T4U_Pharmacy_Repository.Interface;
using T4U_Pharmacy_Service.BaseService;

namespace T4U_Pharmacy_Service
{
    public class PharmacyService: BaseService<Pharmacy>
    {
        private IUnitOfWork _unitOfWork;
        public PharmacyService(IUnitOfWork unitOfWork, IGenericRepository<Pharmacy> repository)
             : base(unitOfWork, repository)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 取得藥局資料
        /// </summary>
        /// <param name="dayOfWeek">營業日</param>
        /// <param name="hourTime">營業時間</param>
        /// <returns></returns>
        public async Task<PharmacyViewModel> GetPharmacies(string dayOfWeek, string hourTime)
        {
            PharmacyViewModel obj = new PharmacyViewModel();
            var pharmacies = this.repository.GetAll();
            if (!string.IsNullOrEmpty(dayOfWeek))
            {
                var iDayOfWeek = int.Parse(dayOfWeek);
                pharmacies = pharmacies.Where(n => n.PharmacyOpeningHours.Any(o => o.Weekday == iDayOfWeek));
            }

            if(!string.IsNullOrEmpty(hourTime))
            {
                pharmacies = pharmacies.Where(
                    n => n.PharmacyOpeningHours.Any(
                        o => 
                        (o.OpenTime.CompareTo(o.CloseTime) <= 0 && o.OpenTime.CompareTo(hourTime) <= 0 && o.CloseTime.CompareTo(hourTime) >= 0)
                        ||
                        (o.OpenTime.CompareTo(o.CloseTime) > 0 && (o.OpenTime.CompareTo(hourTime) <= 0 || o.CloseTime.CompareTo(hourTime) >= 0))
                        )
                );
            }

            var listDetail = pharmacies.Select(n => new { n.PharmacyId, n.Name, n.PharmacyOpeningHours });

            var list = await listDetail.ToListAsync();
            var details = new List<PharmacyDetail>();
            foreach (var item in list)
            {
                var objDetail = new PharmacyDetail 
                {
                    PharmacyId = item.PharmacyId,
                    Name = item.Name,
                    OpenTimeList = item.PharmacyOpeningHours.Select(
                        n => new PharmacyOpenTimeDetail
                        {
                            WeekDaya = WeekDayMap[n.Weekday],
                            OpenTime = n.OpenTime,
                            CloseTime = n.CloseTime
                        })
                    .ToList()
                };
                details.Add(objDetail);
            }
            obj.Data = details;

            return obj;
        }

        private readonly Dictionary<int, string> WeekDayMap = new()
        {
            { (int)DayOfWeek.Sunday,"Sun" },
            { (int)DayOfWeek.Monday,"Mon" },
            { (int)DayOfWeek.Tuesday,"Tue" },
            { (int)DayOfWeek.Wednesday,"Wed" },
            { (int)DayOfWeek.Thursday,"Thur" },
            { (int)DayOfWeek.Friday,"Fri" },
            { (int)DayOfWeek.Saturday,"Sat" }
        };
    }
}
