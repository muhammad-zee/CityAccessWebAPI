using ElmahCore;
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

            List<UsersSchedule> listToBeRemoved = new();
            List<UsersSchedule> listToBeInsert = new();

            List<string> error = new();
            int index = 1;

            foreach (var item in dt.Rows)
            {
                try
                {
                    var row = (DataRow)item;

                    if (string.IsNullOrEmpty(row[0].ToString()))
                    {
                        error.Add($"There is no Schedule Date at row: {index}");
                    }
                    if (string.IsNullOrEmpty(row[1].ToString()))
                    {
                        error.Add($"There is no Start Time for Schedule at row: {index}");
                    }
                    if (string.IsNullOrEmpty(row[2].ToString()))
                    {
                        error.Add($"There is no End Time for Schedule at row: {index}");
                    }
                    if (string.IsNullOrEmpty(row[3].ToString()))
                    {
                        error.Add($"User Initials not found at row: {index}");
                    }

                    index++;

                    if (!string.IsNullOrEmpty(row[0].ToString()) && !string.IsNullOrEmpty(row[1].ToString()) && !string.IsNullOrEmpty(row[2].ToString()) && !string.IsNullOrEmpty(row[3].ToString()))
                    {
                        var initials = row[3].ToString();

                        var userId = (from ur in _userRelationRepo.Table
                                      join u in _userRepo.Table on ur.UserIdFk equals u.UserId
                                      where ur.ServiceLineIdFk == fileVM.ServiceLineId && !u.IsDeleted && u.Initials == initials
                                      select u.UserId).FirstOrDefault();

                        if (userId > 0)
                        {
                            var ScheduleDate = Convert.ToDateTime(row[0]);
                            var alreadyExist = _scheduleRepo.Table.Where(x => x.UserIdFk == userId && x.ScheduleDateStart.Date == ScheduleDate.Date && !x.IsDeleted).ToList();
                            if (alreadyExist.Count > 0)
                            {
                                listToBeRemoved.AddRange(alreadyExist);
                            }

                            var StartDateTimeStr = row[0].ToString() + " " + row[1].ToString();
                            var EndDateTimeStr = row[0].ToString() + " " + row[2].ToString();

                            var StartDateTime = Convert.ToDateTime(StartDateTimeStr);
                            var EndDateTime = Convert.ToDateTime(EndDateTimeStr);
                            if (EndDateTime.TimeOfDay < StartDateTime.TimeOfDay)
                            {
                                EndDateTime = EndDateTime.AddDays(1);
                            }

                            var obj = new UsersSchedule()
                            {
                                ScheduleDateStart = StartDateTime,
                                ScheduleDateEnd = EndDateTime,
                                UserIdFk = userId,
                                ServiceLineIdFk = fileVM.ServiceLineId,
                                CreatedBy = fileVM.LoggedinUserId,
                                CreatedDate = DateTime.UtcNow,
                                IsDeleted = false
                            };
                            listToBeInsert.Add(obj);
                        }
                        else 
                        {
                            error.Add($"'{initials}' is not exist in selected service line.");
                        }

                    }
                }
                catch (Exception ex)
                {
                    error.Add($"There is an exception at row: {index} \n Exception: {ex}");
                    ElmahExtensions.RiseError(ex);
                }
            }

            if (listToBeRemoved.Count > 0)
            {
                _scheduleRepo.DeleteRange(listToBeRemoved);
            }

            if (listToBeInsert.Count > 0)
            {
                _scheduleRepo.Insert(listToBeInsert);
            }


            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Process Complete", Body = error.Distinct() };
        }

    }
}
