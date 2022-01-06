using ElmahCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using Web.Data.Models;
using Web.DLL;
using Web.DLL.Generic_Repository;
using Web.Model;
using Web.Model.Common;
using Web.Services.Extensions;
using Web.Services.Helper;
using Web.Services.Interfaces;

namespace Web.Services.Concrete
{
    public class ScheduleService : IScheduleService
    {
        IConfiguration _config;
        private string conStr;
        private readonly UnitOfWork unitorWork;
        private IHostingEnvironment _environment;
        private RAQ_DbContext _dbContext;

        private readonly IRepository<UsersSchedule> _scheduleRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<UserRole> _userRoleRepo;
        private readonly IRepository<UsersRelation> _userRelationRepo;
        private readonly IRepository<ServiceLine> _serviceRepo;


        public ScheduleService(RAQ_DbContext dbContext,
            IConfiguration configuration,
            IHostingEnvironment environment,
            IRepository<UsersSchedule> scheduleRepo,
            IRepository<User> userRepo,
            IRepository<UserRole> userRoleRepo,
            IRepository<UsersRelation> userRelationRepo,
            IRepository<ServiceLine> serviceRepo)
        {
            this._dbContext = dbContext;
            this._config = configuration;
            this._environment = environment;
            this.conStr = _config["ConnectionStrings:DefaultConnection"].ToString();


            this._scheduleRepo = scheduleRepo;
            this._userRepo = userRepo;
            this._userRoleRepo = userRoleRepo;
            this._userRelationRepo = userRelationRepo;
            this._serviceRepo = serviceRepo;
        }

        public BaseResponse getSchedule(EditParams param)
        {
            var scheduleList = this._dbContext.LoadStoredProcedure("raq_getSchedule")
                .WithSqlParam("@startDate", param.StartDate.ToString("yyyy-MM-dd"))
                .WithSqlParam("@enddate", param.EndDate.ToString("yyyy-MM-dd"))
            .ExecuteStoredProc<ScheduleEventData>();

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = scheduleList };
        }

        public BaseResponse GetScheduleList(ScheduleVM schedule)
        {
            //var scheduleList = this._dbContext.LoadStoredProc("raq_getScheduleListByFilterIds")
            //    .WithSqlParam("@orgId", schedule.selectedOrganizationId)
            //    .WithSqlParam("@serviceLineIds", schedule.selectedService)
            //    .WithSqlParam("@roleIds", schedule.selectedRole)
            //    .WithSqlParam("@userIds", schedule.selectedUser)
            //    .WithSqlParam("@fromDate", schedule.selectedFromDate.ToString("yyyy-MM-dd"))
            //    .WithSqlParam("@toDate", schedule.selectedToDate.ToString("yyyy-MM-dd"))
            //.ExecuteStoredProc<ScheduleListVM>().Result.ToList();

            var scheduleList = this._dbContext.LoadStoredProcedure("raq_getScheduleListByFilterIds")
                        .WithSqlParam("@orgId", schedule.selectedOrganizationId)
                        .WithSqlParam("@serviceLineIds", schedule.selectedService)
                        .WithSqlParam("@roleIds", schedule.selectedRole)
                        .WithSqlParam("@userIds", schedule.selectedUser)
                        .WithSqlParam("@fromDate", schedule.selectedFromDate.ToString("yyyy-MM-dd"))
                        .WithSqlParam("@toDate", schedule.selectedToDate.ToString("yyyy-MM-dd"))
                        .ExecuteStoredProc<ScheduleListVM>();

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = scheduleList };
        }

        public BaseResponse ImportCSV(ImportCSVFileVM fileVM)
        {
            var dt = new CSVReader().GetCSVAsDataTable(fileVM.FilePath);

            List<UsersSchedule> listToBeRemoved = new();
            List<UsersSchedule> listToBeInsert = new();

            List<string> error = new();
            int index = 0;
            int colIndex = 0;

            foreach (var col in dt.Columns)
            {
                if (colIndex > 0)
                {
                    var initials = col.ToString();
                    var userId = (from ur in _userRelationRepo.Table
                                  join u in _userRepo.Table on ur.UserIdFk equals u.UserId
                                  where ur.ServiceLineIdFk == fileVM.ServiceLineId && !u.IsDeleted && u.Initials == initials
                                  select u.UserId).FirstOrDefault();

                    foreach (var item in dt.Rows)
                    {
                        try
                        {
                            var row = (DataRow)item;
                            index++;

                            if (!string.IsNullOrEmpty(row[0].ToString()) && !string.IsNullOrEmpty(row[1].ToString()))
                            {
                                if (userId > 0)
                                {
                                    var ScheduleDate = Convert.ToDateTime(row[0]);
                                    var alreadyExist = _scheduleRepo.Table.Where(x => x.UserIdFk == userId && x.ScheduleDateStart.Date == ScheduleDate.Date && !x.IsDeleted).ToList();
                                    if (alreadyExist.Count > 0)
                                    {
                                        listToBeRemoved.AddRange(alreadyExist);
                                    }

                                    var timeRange = row[1].ToString().Split("-").ToArray();

                                    var StartDateTimeStr = row[0].ToString() + " " + timeRange[0];
                                    var EndDateTimeStr = row[0].ToString() + " " + timeRange[1];

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
                }
                colIndex++;
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

        public BaseResponse GetScheduleTemplate(int serviceLine)
        {
            var serviceLineUsers = (from u in this._userRepo.Table
                                    join ur in this._userRelationRepo.Table on u.UserId equals ur.UserIdFk
                                    join s in this._serviceRepo.Table on ur.ServiceLineIdFk equals s.ServiceLineId
                                    where s.ServiceLineId == serviceLine && !u.IsDeleted && !s.IsDeleted
                                    select new
                                    {
                                        u.Initials,
                                        s.ServiceName
                                    }).Distinct().ToList();
            if (serviceLineUsers.Count > 0)
            {
                int count = 0;
                DataTable tbl = new DataTable();
                foreach (var item in serviceLineUsers)
                {
                    if (count == 0)
                    {
                        DataColumn col = new DataColumn("Date", typeof(string));
                        tbl.Columns.Add(col);
                        col = new DataColumn(item.Initials, typeof(string));
                        tbl.Columns.Add(col);
                    }
                    else
                    {
                        DataColumn col = new DataColumn(item.Initials, typeof(string));
                        tbl.Columns.Add(col);
                    }
                    count++;
                }

                for (int i = 0; i < 7; i++)
                {
                    DataRow row = tbl.NewRow();
                    row[0] = DateTime.Now.AddDays(i).ToString("dd/MM/yyyy");
                    row[1] = "8:00-18:00";
                    tbl.Rows.Add(row);
                }

                var folderName = Path.Combine("ScheduleTemplates");
                var pathToSave = Path.Combine(this._environment.WebRootPath, folderName);
                if (!Directory.Exists(pathToSave))
                {
                    Directory.CreateDirectory(pathToSave);
                }
                var fileName = $"Schedule Template For {serviceLineUsers.FirstOrDefault().ServiceName}.csv"; //ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                var fullPath = Path.Combine(pathToSave, fileName);
                new CSVReader().WriteDataTableAsCSV(tbl, fullPath);


                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Template ready", Body = new { path = fullPath.Replace(this._environment.WebRootPath, ""), fileName = fileName } };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "No user found in selected service line." };
            }
        }

        public BaseResponse SaveSchedule(ScheduleVM schedule)
        {
            if (schedule.ScheduleId > 0)
            {
                var row = _scheduleRepo.Table.Where(x => !x.IsDeleted && x.UsersScheduleId == schedule.ScheduleId).FirstOrDefault();
                if (row != null)
                {
                    string startDateTimeStr = schedule.FromDate.ToString("dd-MM-yyyy") + schedule.StartTime.ToString("hh:mm:ss");
                    string endDateTimeStr = schedule.ToDate.ToString("dd-MM-yyyy") + schedule.EndTime.ToString("hh:mm:ss");

                    DateTime startDateTime = Convert.ToDateTime(startDateTimeStr);
                    DateTime endDateTime = Convert.ToDateTime(endDateTimeStr);

                    row.ScheduleDateStart = startDateTime;
                    row.ScheduleDateEnd = endDateTime;
                    row.ModifiedBy = schedule.ModifiedBy;
                    row.ModifiedDate = DateTime.UtcNow;
                    row.IsDeleted = false;

                    _scheduleRepo.Update(row);

                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Schedule Updated" };
                }
                else
                {
                    return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Schedule Not Found" };
                }
            }
            else
            {
                List<UsersSchedule> usersSchedules = new();
                var userIds = schedule.UserId.ToIntList();
                var roleIds = schedule.RoleId.ToIntList();
                if (schedule.DateRangeId == 1)
                {
                    DateTime now = DateTime.UtcNow;
                    var startDate = new DateTime(now.Year, now.Month, 1);
                    var endDate = startDate.AddMonths(1).AddDays(-1);


                    while (true)
                    {
                        if (startDate <= endDate)
                        {
                            string startDateTimeStr = startDate.ToString("MM-dd-yyyy") + " " + schedule.StartTime.ToString("hh:mm:ss tt");
                            string endDateTimeStr = startDate.ToString("MM-dd-yyyy") + " " + schedule.EndTime.ToString("hh:mm:ss tt");

                            DateTime StartDateTime = Convert.ToDateTime(startDateTimeStr);
                            DateTime EndDateTime = Convert.ToDateTime(endDateTimeStr);

                            if (StartDateTime.TimeOfDay > EndDateTime.TimeOfDay)
                            {
                                EndDateTime = EndDateTime.AddDays(1);
                            }

                            foreach (var user in userIds)
                            {
                                foreach (var role in roleIds)
                                {
                                    if (_userRelationRepo.Table.Any(x => x.UserIdFk == user && x.ServiceLineIdFk == schedule.ServiceLineId) && _userRoleRepo.Table.Any(x => x.UserIdFk == user && x.RoleIdFk == role))
                                    {
                                        var userSchedule = new UsersSchedule()
                                        {
                                            ScheduleDateStart = StartDateTime,
                                            ScheduleDateEnd = EndDateTime,
                                            ServiceLineIdFk = schedule.ServiceLineId,
                                            DateRangeId = schedule.DateRangeId,
                                            UserIdFk = user,
                                            CreatedBy = schedule.CreatedBy,
                                            CreatedDate = DateTime.UtcNow,
                                            IsDeleted = false,
                                        };

                                        usersSchedules.Add(userSchedule);
                                    }
                                }
                            }
                            startDate = startDate.AddDays(1);
                        }
                        else
                        {
                            break;
                        }
                    }

                }
                else if (schedule.DateRangeId == 2)
                {
                    int year = DateTime.Now.Year;
                    DateTime startDate = new DateTime(year, 1, 1);
                    DateTime endDate = new DateTime(year, 12, 31);

                    while (true)
                    {
                        if (startDate <= endDate)
                        {
                            string startDateTimeStr = startDate.ToString("MM-dd-yyyy") + " " + schedule.StartTime.ToString("hh:mm:ss tt");
                            string endDateTimeStr = endDate.ToString("MM-dd-yyyy") + " " + schedule.EndTime.ToString("hh:mm:ss tt");

                            DateTime StartDateTime = Convert.ToDateTime(startDateTimeStr);
                            DateTime EndDateTime = Convert.ToDateTime(endDateTimeStr);

                            foreach (var user in userIds)
                            {
                                foreach (var role in roleIds)
                                {
                                    if (_userRelationRepo.Table.Any(x => x.UserIdFk == user && x.ServiceLineIdFk == schedule.ServiceLineId) && _userRoleRepo.Table.Any(x => x.UserIdFk == user && x.RoleIdFk == role))
                                    {
                                        var userSchedule = new UsersSchedule()
                                        {
                                            ScheduleDateStart = StartDateTime,
                                            ScheduleDateEnd = EndDateTime,
                                            ServiceLineIdFk = schedule.ServiceLineId,
                                            DateRangeId = schedule.DateRangeId,
                                            UserIdFk = user,
                                            CreatedBy = schedule.CreatedBy,
                                            CreatedDate = DateTime.UtcNow,
                                            IsDeleted = false,
                                        };

                                        usersSchedules.Add(userSchedule);
                                    }
                                }
                            }
                            startDate = startDate.AddDays(1);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    DateTime startDate = schedule.FromDate;
                    DateTime endDate = schedule.ToDate;

                    while (true)
                    {
                        if (startDate <= endDate)
                        {
                            string startDateTimeStr = startDate.ToString("MM-dd-yyyy") + " " + schedule.StartTime.ToString("hh:mm:ss tt");
                            string endDateTimeStr = endDate.ToString("MM-dd-yyyy") + " " + schedule.EndTime.ToString("hh:mm:ss tt");

                            DateTime StartDateTime = Convert.ToDateTime(startDateTimeStr);
                            DateTime EndDateTime = Convert.ToDateTime(endDateTimeStr);

                            foreach (var user in userIds)
                            {
                                foreach (var role in roleIds)
                                {
                                    if (_userRelationRepo.Table.Any(x => x.UserIdFk == user && x.ServiceLineIdFk == schedule.ServiceLineId) && _userRoleRepo.Table.Any(x => x.UserIdFk == user && x.RoleIdFk == role))
                                    {
                                        var userSchedule = new UsersSchedule()
                                        {
                                            ScheduleDateStart = StartDateTime,
                                            ScheduleDateEnd = EndDateTime,
                                            ServiceLineIdFk = schedule.ServiceLineId,
                                            DateRangeId = schedule.DateRangeId,
                                            UserIdFk = user,
                                            CreatedBy = schedule.CreatedBy,
                                            CreatedDate = DateTime.UtcNow,
                                            IsDeleted = false,
                                        };

                                        usersSchedules.Add(userSchedule);
                                    }
                                }
                            }
                            startDate = startDate.AddDays(1);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                _scheduleRepo.Insert(usersSchedules);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Schedule Created" };

            }

        }

        public BaseResponse DeleteSchedule(int scheduleId, int userId)
        {
            var schedule = _scheduleRepo.Table.Where(x => x.UsersScheduleId == scheduleId && !x.IsDeleted).FirstOrDefault();
            if (schedule != null)
            {
                schedule.IsDeleted = true;
                schedule.ModifiedBy = userId;
                schedule.ModifiedDate = DateTime.UtcNow;

                _scheduleRepo.Update(schedule);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted." };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "No Schedule Found" };
            }
        }
    }
}
