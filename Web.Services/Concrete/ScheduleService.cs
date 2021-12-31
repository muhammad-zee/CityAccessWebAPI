using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using Web.Data.Models;
using Web.DLL;
using Web.DLL.Generic_Repository;
using Web.Model;
using Web.Model.Common;
using Web.Services.Interfaces;

namespace Web.Services.Concrete
{
    public class ScheduleService : IScheduleService
    {
        IConfiguration _config;
        private readonly UnitOfWork unitorWork;
        private IHostingEnvironment _environment;

        private readonly IRepository<UsersSchedule> _scheduleRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<UsersRelation> _userRelationRepo;
        private readonly IRepository<ServiceLine> _serviceRepo;


        public ScheduleService(IConfiguration configuration,
            IHostingEnvironment environment,
            IRepository<UsersSchedule> scheduleRepo,
            IRepository<User> userRepo,
            IRepository<UsersRelation> userRelationRepo,
            IRepository<ServiceLine> serviceRepo)
        {
            this._config = configuration;
            this._environment = environment;
            this._scheduleRepo = scheduleRepo;
            this._userRepo = userRepo;
            this._userRelationRepo = userRelationRepo;
            this._serviceRepo = serviceRepo;
        }

        public BaseResponse ImportCSV(ImportCSVFileVM fileVM)
        {
            var dt = new Helper.CSVReader().GetCSVAsDataTable(fileVM.FilePath);

            List<UsersSchedule> listToBeRemoved = new List<UsersSchedule>();
            List<UsersSchedule> listToBeInsert = new List<UsersSchedule>();
            List<int> ids = new List<int>();
            foreach (var item in dt.Rows)
            {
                var row = (DataRow)item;
                if (!string.IsNullOrEmpty(row[3].ToString()))
                {
                    ids.Add(Convert.ToInt32(row[3]));
                }
            }


            var userIds = (from ur in _userRelationRepo.Table
                           join u in _userRepo.Table on ur.UserIdFk equals u.UserId
                           where ur.ServiceLineIdFk == fileVM.ServiceLineId && !u.IsDeleted && ids.Distinct().Contains(u.UserId)
                           select u.UserId).ToList();


            foreach (var item in dt.Rows)
            {
                var row = (DataRow)item;
                if (!string.IsNullOrEmpty(row[0].ToString()) || !string.IsNullOrEmpty(row[1].ToString()) || !string.IsNullOrEmpty(row[2].ToString()) || !string.IsNullOrEmpty(row[3].ToString()))
                {

                    var ScheduleDate = Convert.ToDateTime(row[0]);
                    var alreadyExist = _scheduleRepo.Table.Where(x => userIds.Contains(x.UserIdFk) && x.ScheduleDate.Date == ScheduleDate.Date && !x.IsDeleted).ToList();
                    if (alreadyExist.Count > 0)
                    {
                        listToBeRemoved.AddRange(alreadyExist);
                    }

                    var StartDateTimeStr = row[0].ToString() + " " + row[1].ToString();
                    var EndDateTimeStr = row[0].ToString() + " " + row[2].ToString();

                    var StartDateTime = Convert.ToDateTime(StartDateTimeStr);
                    var EndDateTime = Convert.ToDateTime(EndDateTimeStr);
                    if (StartDateTime.TimeOfDay > EndDateTime.TimeOfDay)
                    {
                        EndDateTime = EndDateTime.AddDays(1);
                    }

                    foreach (var u in userIds)
                    {
                        var obj = new UsersSchedule()
                        {
                            ScheduleDate = ScheduleDate,
                            ScheduleTimeStart = StartDateTime,
                            ScheduleTimeEnd = EndDateTime,
                            UserIdFk = u,
                            CreatedBy = fileVM.LoggedinUserId,
                            CreatedDate = DateTime.UtcNow,
                            IsDeleted = false
                        };
                        listToBeInsert.Add(obj);
                    }
                }
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = new { dt = listToBeInsert } };
        }

    }
}
