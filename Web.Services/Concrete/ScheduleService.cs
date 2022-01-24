using ElmahCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
        private readonly IOptions<ApplicationSettings> _appSettings;

        private readonly IRepository<UsersSchedule> _scheduleRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<UserRole> _userRoleRepo;
        private readonly IRepository<UsersRelation> _userRelationRepo;
        private readonly IRepository<ServiceLine> _serviceRepo;


        public ScheduleService(RAQ_DbContext dbContext,
            IConfiguration configuration,
            IOptions<ApplicationSettings> appSettings,
            IHostingEnvironment environment,
            IRepository<UsersSchedule> scheduleRepo,
            IRepository<User> userRepo,
            IRepository<UserRole> userRoleRepo,
            IRepository<UsersRelation> userRelationRepo,
            IRepository<ServiceLine> serviceRepo)
        {
            this._dbContext = dbContext;
            this._config = configuration;
            this._appSettings = appSettings;
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
            if (param.ShowAllSchedule == "false")
            {
                var schedule = (from us in _scheduleRepo.Table
                                join u in _userRepo.Table on us.UserIdFk equals u.UserId
                                where us.UserIdFk == param.CreatedBy && us.ScheduleDate >= DateTime.UtcNow.Date && !us.IsDeleted && !u.IsDeleted
                                select new ScheduleEventData()
                                {
                                    id = us.UsersScheduleId,
                                    subject = u.FirstName + " " + u.LastName,
                                    startTime = us.ScheduleDateStart,
                                    endTime = us.ScheduleDateEnd,
                                    scheduleUserId = u.UserId.ToString(),
                                    userId = u.UserId.ToString(),
                                    roleId = us.RoleIdFk.ToString(),
                                    serviceLineId = us.ServiceLineIdFk.ToString()
                                }).ToList();
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = schedule };
            }

            var scheduleList = this._dbContext.LoadStoredProcedure("raq_getSchedule")
                .WithSqlParam("@startDate", param.StartDate.AddDays(-1))
                .WithSqlParam("@endDate", param.EndDate)
                .WithSqlParam("@organizationId", param.OrganizationId)
                .WithSqlParam("@departmentIds", param.departmentIds)
                .WithSqlParam("@serviceLineIds", param.ServiceLineIds)
                .WithSqlParam("@roleIds", param.RoleIds)
                .WithSqlParam("@userIds", param.UserIds)
            .ExecuteStoredProc<ScheduleEventData>();

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = scheduleList };
        }

        public BaseResponse GetScheduleList(ScheduleVM schedule)
        {
            var scheduleList = this._dbContext.LoadStoredProcedure("raq_getScheduleListByFilterIds")
                            .WithSqlParam("@orgId", schedule.selectedOrganizationId)
                            .WithSqlParam("@serviceLineIds", schedule.selectedService)
                            .WithSqlParam("@roleIds", schedule.selectedRole)
                            .WithSqlParam("@userIds", schedule.selectedUser)
                            .WithSqlParam("@fromDate", schedule.selectedFromDate.Date.ToUniversalTime().ToString("yyyy-MM-dd"))
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
                                        ScheduleDate = StartDateTime.Date.ToUniversalTimeZone(),
                                        ScheduleDateStart = StartDateTime.ToUniversalTimeZone(),
                                        ScheduleDateEnd = EndDateTime.ToUniversalTimeZone(),
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

        public BaseResponse AddUpdateUserSchedule(EditParams param)
        {
            UsersSchedule schedule = null;
            List<UsersSchedule> scheduleList = new List<UsersSchedule>();

            BaseResponse response = new BaseResponse();
            if (param.action == "batch" && param.added != null && param.added.Count > 0) // this block of code will execute while inserting the appointments
            {

                scheduleList = (from u in param.added
                                select new UsersSchedule
                                {
                                    ScheduleDate = u.startTime,
                                    ScheduleDateStart = u.startTime,
                                    ScheduleDateEnd = u.endTime,
                                    UserIdFk = param.selectedUserId.ToInt(),
                                    RoleIdFk = u.roleId.ToInt(),
                                    ServiceLineIdFk = u.serviceLineId.ToInt(),
                                    CreatedBy = param.CreatedBy,
                                    CreatedDate = DateTime.UtcNow,
                                    IsDeleted = false
                                }).ToList();
                this._scheduleRepo.Insert(scheduleList);

                response.Status = HttpStatusCode.OK;
                response.Message = "Schedule Saved";
                response.Body = scheduleList;
            }
            if (param.action == "batch" && param.changed != null && param.changed.Count > 0) // this block of code will execute while updating the appointment
            {
                var changedSchedule = param.changed.FirstOrDefault();
                schedule = this._scheduleRepo.Table.FirstOrDefault(s => s.UsersScheduleId == changedSchedule.id);
                if (schedule != null)
                {
                    schedule.ScheduleDate = changedSchedule.startTime;
                    schedule.ScheduleDateStart = changedSchedule.startTime;
                    schedule.ScheduleDateEnd = changedSchedule.endTime;

                    schedule.UserIdFk = Convert.ToInt32(changedSchedule.userId);
                    schedule.RoleIdFk = changedSchedule.roleId.ToInt();

                    schedule.ServiceLineIdFk = Convert.ToInt32(changedSchedule.serviceLineId);
                    schedule.ModifiedBy = param.ModifiedBy;
                    schedule.ModifiedDate = DateTime.UtcNow;
                    schedule.IsDeleted = false;
                    this._scheduleRepo.Update(schedule);

                    response.Status = HttpStatusCode.OK;
                    response.Message = "Schedule Updated";
                    response.Body = schedule;

                }
                else
                {
                    response.Status = HttpStatusCode.NotFound;
                    response.Message = "Schedule Not Found";
                }


            }
            if (param.action == "batch" && param.deleted != null && param.deleted.Count > 0) // this block of code will execute while removing the appointment
            {
                var deletedSchedule = param.deleted.FirstOrDefault();
                schedule = this._scheduleRepo.Table.FirstOrDefault(s => s.UsersScheduleId == deletedSchedule.id);
                if (schedule != null)
                {
                    schedule.ModifiedBy = param.ModifiedBy;
                    schedule.ModifiedDate = DateTime.UtcNow;
                    schedule.IsDeleted = true;
                    this._scheduleRepo.Update(schedule);

                    response.Status = HttpStatusCode.OK;
                    response.Message = "Schedule Deleted";
                    response.Body = schedule;
                }
                else
                {
                    response.Status = HttpStatusCode.NotFound;
                    response.Message = "Schedule Not Found";
                    response.Body = schedule;

                }
            }

            return response;
        }

        public BaseResponse SaveSchedule(ScheduleVM schedule)
        {
            if (schedule.ScheduleId > 0)
            {
                var row = _scheduleRepo.Table.Where(x => !x.IsDeleted && x.UsersScheduleId == schedule.ScheduleId).FirstOrDefault();
                if (row != null)
                {
                    if (_scheduleRepo.Table.Any(x => x.ScheduleDate.Value.Date == schedule.FromDate.Date && !x.IsDeleted))
                    {
                        string startDateTimeStr = schedule.FromDate.ToString("MM-dd-yyyy") + " " + schedule.StartTime.ToString("hh:mm:ss tt");
                        string endDateTimeStr = schedule.ToDate.ToString("MM-dd-yyyy") + " " + schedule.EndTime.ToString("hh:mm:ss tt");

                        DateTime? startDateTime = Convert.ToDateTime(startDateTimeStr);
                        DateTime? endDateTime = Convert.ToDateTime(endDateTimeStr);

                        if (startDateTime.Value.TimeOfDay > endDateTime.Value.TimeOfDay)
                        {
                            endDateTime.Value.AddDays(1);
                        }

                        row.ScheduleDate = startDateTime.Value.ToUniversalTimeZone();
                        row.ScheduleDateStart = startDateTime.Value.ToUniversalTimeZone();
                        row.ScheduleDateEnd = endDateTime.Value.ToUniversalTimeZone();
                        row.ModifiedBy = schedule.ModifiedBy;
                        row.ModifiedDate = DateTime.UtcNow;
                        row.IsDeleted = false;

                        _scheduleRepo.Update(row);

                        return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Schedule Updated" };
                    }
                    else 
                    {
                        return new BaseResponse() { Status = HttpStatusCode.NotModified, Message = "Schedule Already Exist on this date" };
                    }
                }
                else
                {
                    return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Schedule Not Found" };
                }
            }
            else
            {
                List<UsersSchedule> usersSchedules = new();
                var userIds = schedule.UserIdFk.ToIntList();
                var roleIds = schedule.RoleIdFk.ToIntList();
                int count = 0;
                var weekDays = schedule.WeekDays.Split(",");
                if (schedule.DateRangeId == 2)
                {
                    DateTime loopFirstDate = schedule.FromDate;
                    DateTime startDate = schedule.FromDate;
                    DateTime endDate = schedule.ToDate;

                    while (true)
                    {
                        if (startDate <= endDate)
                        {
                            if (loopFirstDate.Date != startDate.Date && schedule.RepeatEvery > 1)
                            {
                                startDate = startDate.AddDays((schedule.RepeatEvery - 1));
                                if (startDate > endDate)
                                {
                                    break;
                                }
                            }

                            string startDateTimeStr = startDate.ToString("MM-dd-yyyy") + " " + schedule.StartTime.ToString("hh:mm:ss tt");
                            string endDateTimeStr = startDate.ToString("MM-dd-yyyy") + " " + schedule.EndTime.ToString("hh:mm:ss tt");

                            DateTime? StartDateTime = Convert.ToDateTime(startDateTimeStr);
                            DateTime? EndDateTime = Convert.ToDateTime(endDateTimeStr);

                            if (StartDateTime.Value.TimeOfDay > EndDateTime.Value.TimeOfDay)
                            {
                                EndDateTime = EndDateTime.Value.AddDays(1);
                            }
                            foreach (var user in userIds)
                            {
                                foreach (var role in roleIds)
                                {
                                    var alreadyExist = _scheduleRepo.Table.Where(x => x.UserIdFk == user && x.RoleIdFk == role && x.ServiceLineIdFk == schedule.ServiceLineIdFk && x.ScheduleDate.Value.Date == StartDateTime.Value.Date && !x.IsDeleted).ToList();
                                    if (alreadyExist.Count > 0)
                                    {
                                        alreadyExist.ForEach(x => { x.IsDeleted = true; x.ModifiedBy = schedule.CreatedBy; x.ModifiedDate = DateTime.UtcNow; });
                                        _scheduleRepo.Update(alreadyExist);
                                    }
                                    if (_userRelationRepo.Table.Any(x => x.UserIdFk == user && x.ServiceLineIdFk == schedule.ServiceLineIdFk) && _userRoleRepo.Table.Any(x => x.UserIdFk == user && x.RoleIdFk == role))
                                    {
                                        var userSchedule = new UsersSchedule()
                                        {
                                            ScheduleDate = StartDateTime.Value.ToUniversalTimeZone(),
                                            ScheduleDateStart = StartDateTime.Value.ToUniversalTimeZone(),
                                            ScheduleDateEnd = EndDateTime.Value.ToUniversalTimeZone(),
                                            ServiceLineIdFk = schedule.ServiceLineIdFk,
                                            UserIdFk = user,
                                            RoleIdFk = role,
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
                else if (schedule.DateRangeId == 3)
                {
                    //DateTime startOfWeek = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek)).AddDays(1);
                    var loopFirstDate = schedule.FromDate;
                    var startDate = schedule.FromDate;
                    var endDate = schedule.ToDate; //schedule.FromDate.AddDays(DayOfWeek.Saturday - schedule.FromDate.DayOfWeek).Date;


                    while (true)
                    {
                        count++;
                        if (startDate <= endDate)
                        {
                            if (weekDays.Contains(startDate.DayOfWeek.ToString()))
                            {

                                if (loopFirstDate.Date != startDate.Date && schedule.RepeatEvery > 1 && startDate.DayOfWeek.ToString() == weekDays[0])
                                {
                                    startDate = startDate.AddDays((schedule.RepeatEvery - 1) * 7);
                                    if (startDate > endDate)
                                    {
                                        break;
                                    }
                                    count = 0;
                                }
                                string startDateTimeStr = startDate.ToString("MM-dd-yyyy") + " " + schedule.StartTime.ToString("hh:mm:ss tt");
                                string endDateTimeStr = startDate.ToString("MM-dd-yyyy") + " " + schedule.EndTime.ToString("hh:mm:ss tt");

                                DateTime? StartDateTime = Convert.ToDateTime(startDateTimeStr);
                                DateTime? EndDateTime = Convert.ToDateTime(endDateTimeStr);

                                if (StartDateTime.Value.TimeOfDay > EndDateTime.Value.TimeOfDay)
                                {
                                    EndDateTime = EndDateTime.Value.AddDays(1);
                                }
                                foreach (var user in userIds)
                                {
                                    foreach (var role in roleIds)
                                    {
                                        var alreadyExist = _scheduleRepo.Table.Where(x => x.UserIdFk == user && x.RoleIdFk == role && x.ServiceLineIdFk == schedule.ServiceLineIdFk && x.ScheduleDate.Value.Date == StartDateTime.Value.Date && !x.IsDeleted).ToList();
                                        if (alreadyExist.Count > 0)
                                        {
                                            alreadyExist.ForEach(x => { x.IsDeleted = true; x.ModifiedBy = schedule.CreatedBy; x.ModifiedDate = DateTime.UtcNow; });
                                            _scheduleRepo.Update(alreadyExist);
                                        }

                                        if (_userRelationRepo.Table.Any(x => x.UserIdFk == user && x.ServiceLineIdFk == schedule.ServiceLineIdFk) && _userRoleRepo.Table.Any(x => x.UserIdFk == user && x.RoleIdFk == role))
                                        {
                                            var userSchedule = new UsersSchedule()
                                            {
                                                ScheduleDate = StartDateTime.Value.ToUniversalTimeZone(),
                                                ScheduleDateStart = StartDateTime.Value.ToUniversalTimeZone(),
                                                ScheduleDateEnd = EndDateTime.Value.ToUniversalTimeZone(),
                                                ServiceLineIdFk = schedule.ServiceLineIdFk,
                                                UserIdFk = user,
                                                RoleIdFk = role,
                                                CreatedBy = schedule.CreatedBy,
                                                CreatedDate = DateTime.UtcNow,
                                                IsDeleted = false,
                                            };

                                            usersSchedules.Add(userSchedule);
                                        }
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
                else if (schedule.DateRangeId == 4)
                {
                    if (schedule.SelectiveDates.Count > 0)
                    {
                        foreach (var item in schedule.SelectiveDates)
                        {
                            string startDateTimeStr = item.ToString("MM-dd-yyyy") + " " + schedule.StartTime.ToString("hh:mm:ss tt");
                            string endDateTimeStr = item.ToString("MM-dd-yyyy") + " " + schedule.EndTime.ToString("hh:mm:ss tt");

                            DateTime? StartDateTime = Convert.ToDateTime(startDateTimeStr);
                            DateTime? EndDateTime = Convert.ToDateTime(endDateTimeStr);

                            if (StartDateTime.Value.TimeOfDay > EndDateTime.Value.TimeOfDay)
                            {
                                EndDateTime = EndDateTime.Value.AddDays(1);
                            }

                            foreach (var user in userIds)
                            {
                                foreach (var role in roleIds)
                                {
                                    var alreadyExist = _scheduleRepo.Table.Where(x => x.UserIdFk == user && x.RoleIdFk == role && x.ServiceLineIdFk == schedule.ServiceLineIdFk && x.ScheduleDate.Value.Date == StartDateTime.Value.Date && !x.IsDeleted).ToList();
                                    if (alreadyExist.Count > 0)
                                    {
                                        alreadyExist.ForEach(x => { x.IsDeleted = true; x.ModifiedBy = schedule.CreatedBy; x.ModifiedDate = DateTime.UtcNow; });
                                        _scheduleRepo.Update(alreadyExist);
                                    }
                                    if (_userRelationRepo.Table.Any(x => x.UserIdFk == user && x.ServiceLineIdFk == schedule.ServiceLineIdFk) && _userRoleRepo.Table.Any(x => x.UserIdFk == user && x.RoleIdFk == role))
                                    {
                                        var userSchedule = new UsersSchedule()
                                        {
                                            ScheduleDate = StartDateTime.Value.ToUniversalTimeZone(),
                                            ScheduleDateStart = StartDateTime.Value.ToUniversalTimeZone(),
                                            ScheduleDateEnd = EndDateTime.Value.ToUniversalTimeZone(),
                                            ServiceLineIdFk = schedule.ServiceLineIdFk,
                                            UserIdFk = user,
                                            RoleIdFk = role,
                                            CreatedBy = schedule.CreatedBy,
                                            CreatedDate = DateTime.UtcNow,
                                            IsDeleted = false,
                                        };

                                        usersSchedules.Add(userSchedule);
                                    }
                                }
                            }
                        }
                    }
                    else 
                    {
                        //DateTime now = DateTime.UtcNow;
                        var startDate = schedule.FromDate;//new DateTime(now.Year, now.Month, 1);
                        var endDate = schedule.ToDate;


                        while (true)
                        {
                            if (startDate <= endDate)
                            {
                                string startDateTimeStr = startDate.ToString("MM-dd-yyyy") + " " + schedule.StartTime.ToString("hh:mm:ss tt");
                                string endDateTimeStr = startDate.ToString("MM-dd-yyyy") + " " + schedule.EndTime.ToString("hh:mm:ss tt");

                                DateTime? StartDateTime = Convert.ToDateTime(startDateTimeStr);
                                DateTime? EndDateTime = Convert.ToDateTime(endDateTimeStr);

                                if (StartDateTime.Value.TimeOfDay > EndDateTime.Value.TimeOfDay)
                                {
                                    EndDateTime = EndDateTime.Value.AddDays(1);
                                }

                                foreach (var user in userIds)
                                {
                                    foreach (var role in roleIds)
                                    {
                                        var alreadyExist = _scheduleRepo.Table.Where(x => x.UserIdFk == user && x.RoleIdFk == role && x.ServiceLineIdFk == schedule.ServiceLineIdFk && x.ScheduleDate.Value.Date == StartDateTime.Value.Date && !x.IsDeleted).ToList();
                                        if (alreadyExist.Count > 0)
                                        {
                                            alreadyExist.ForEach(x => { x.IsDeleted = true; x.ModifiedBy = schedule.CreatedBy; x.ModifiedDate = DateTime.UtcNow; });
                                            _scheduleRepo.Update(alreadyExist);
                                        }
                                        if (_userRelationRepo.Table.Any(x => x.UserIdFk == user && x.ServiceLineIdFk == schedule.ServiceLineIdFk) && _userRoleRepo.Table.Any(x => x.UserIdFk == user && x.RoleIdFk == role))
                                        {
                                            var userSchedule = new UsersSchedule()
                                            {
                                                ScheduleDate = StartDateTime.Value.ToUniversalTimeZone(),
                                                ScheduleDateStart = StartDateTime.Value.ToUniversalTimeZone(),
                                                ScheduleDateEnd = EndDateTime.Value.ToUniversalTimeZone(),
                                                ServiceLineIdFk = schedule.ServiceLineIdFk,
                                                UserIdFk = user,
                                                RoleIdFk = role,
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
                }
                else if (schedule.DateRangeId == 5)
                {
                    //int year = DateTime.Now.Year;
                    DateTime startDate = schedule.FromDate; //new DateTime(year, 1, 1);
                    DateTime endDate = new DateTime(schedule.FromDate.Year, 12, 31);

                    while (true)
                    {
                        if (startDate <= endDate)
                        {
                            string startDateTimeStr = startDate.ToString("MM-dd-yyyy") + " " + schedule.StartTime.ToString("hh:mm:ss tt");
                            string endDateTimeStr = startDate.ToString("MM-dd-yyyy") + " " + schedule.EndTime.ToString("hh:mm:ss tt");

                            DateTime? StartDateTime = Convert.ToDateTime(startDateTimeStr);
                            DateTime? EndDateTime = Convert.ToDateTime(endDateTimeStr);
                            if (StartDateTime.Value.TimeOfDay > EndDateTime.Value.TimeOfDay)
                            {
                                EndDateTime = EndDateTime.Value.AddDays(1);
                            }
                            foreach (var user in userIds)
                            {
                                foreach (var role in roleIds)
                                {
                                    var alreadyExist = _scheduleRepo.Table.Where(x => x.UserIdFk == user && x.RoleIdFk == role && x.ServiceLineIdFk == schedule.ServiceLineIdFk && x.ScheduleDate.Value.Date == StartDateTime.Value.Date && !x.IsDeleted).ToList();
                                    if (alreadyExist.Count > 0)
                                    {
                                        alreadyExist.ForEach(x => { x.IsDeleted = true; x.ModifiedBy = schedule.CreatedBy; x.ModifiedDate = DateTime.UtcNow; });
                                        _scheduleRepo.Update(alreadyExist);
                                    }
                                    if (_userRelationRepo.Table.Any(x => x.UserIdFk == user && x.ServiceLineIdFk == schedule.ServiceLineIdFk) && _userRoleRepo.Table.Any(x => x.UserIdFk == user && x.RoleIdFk == role))
                                    {
                                        var userSchedule = new UsersSchedule()
                                        {
                                            ScheduleDate = StartDateTime.Value.ToUniversalTimeZone(),
                                            ScheduleDateStart = StartDateTime.Value.ToUniversalTimeZone(),
                                            ScheduleDateEnd = EndDateTime.Value.ToUniversalTimeZone(),
                                            ServiceLineIdFk = schedule.ServiceLineIdFk,
                                            UserIdFk = user,
                                            RoleIdFk = role,
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

                    string startDateTimeStr = startDate.ToString("MM-dd-yyyy") + " " + schedule.StartTime.ToString("hh:mm:ss tt");
                    string endDateTimeStr = startDate.ToString("MM-dd-yyyy") + " " + schedule.EndTime.ToString("hh:mm:ss tt");

                    DateTime? StartDateTime = Convert.ToDateTime(startDateTimeStr);
                    DateTime? EndDateTime = Convert.ToDateTime(endDateTimeStr);

                    if (StartDateTime.Value.TimeOfDay > EndDateTime.Value.TimeOfDay)
                    {
                        EndDateTime = EndDateTime.Value.AddDays(1);
                    }
                    foreach (var user in userIds)
                    {
                        foreach (var role in roleIds)
                        {
                            var alreadyExist = _scheduleRepo.Table.Where(x => x.UserIdFk == user && x.RoleIdFk == role && x.ServiceLineIdFk == schedule.ServiceLineIdFk && x.ScheduleDate.Value.Date == StartDateTime.Value.Date && !x.IsDeleted).ToList();
                            if (alreadyExist.Count > 0)
                            {
                                alreadyExist.ForEach(x => { x.IsDeleted = true; x.ModifiedBy = schedule.CreatedBy; x.ModifiedDate = DateTime.UtcNow; });
                                _scheduleRepo.Update(alreadyExist);
                            }
                            if (_userRelationRepo.Table.Any(x => x.UserIdFk == user && x.ServiceLineIdFk == schedule.ServiceLineIdFk) && _userRoleRepo.Table.Any(x => x.UserIdFk == user && x.RoleIdFk == role))
                            {
                                var userSchedule = new UsersSchedule()
                                {
                                    ScheduleDate = StartDateTime.Value.ToUniversalTimeZone(),
                                    ScheduleDateStart = StartDateTime.Value.ToUniversalTimeZone(),
                                    ScheduleDateEnd = EndDateTime.Value.ToUniversalTimeZone(),
                                    ServiceLineIdFk = schedule.ServiceLineIdFk,
                                    UserIdFk = user,
                                    RoleIdFk = role,
                                    CreatedBy = schedule.CreatedBy,
                                    CreatedDate = DateTime.UtcNow,
                                    IsDeleted = false,
                                };

                                usersSchedules.Add(userSchedule);
                            }
                        }
                    }

                }

                if (usersSchedules.Count > 0)
                {
                    _scheduleRepo.Insert(usersSchedules);
                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Schedule Created" };
                }
                else
                {
                    return new BaseResponse() { Status = HttpStatusCode.NotModified, Message = "Schedule Not Created" };
                }
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
