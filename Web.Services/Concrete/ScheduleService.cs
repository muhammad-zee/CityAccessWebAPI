﻿using ElmahCore;
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
using Web.Services.Enums;
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
        private string _RootPath;
        private readonly IRepository<UsersSchedule> _scheduleRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<UserRole> _userRoleRepo;
        private readonly IRepository<UsersRelation> _userRelationRepo;
        private readonly IRepository<ServiceLine> _serviceRepo;
        private readonly IRepository<Role> _roleRepo;

        public ScheduleService(RAQ_DbContext dbContext,
            IConfiguration configuration,
            IHostingEnvironment environment,
            IRepository<UsersSchedule> scheduleRepo,
            IRepository<User> userRepo,
            IRepository<Role> roleRepo,
            IRepository<UserRole> userRoleRepo,
            IRepository<UsersRelation> userRelationRepo,
            IRepository<ServiceLine> serviceRepo)
        {
            this._dbContext = dbContext;
            this._config = configuration;
            this._environment = environment;
            this.conStr = _config["ConnectionStrings:DefaultConnection"].ToString();
            this._RootPath = _config["FilePath:Path"].ToString();

            this._scheduleRepo = scheduleRepo;
            this._userRepo = userRepo;
            this._roleRepo = roleRepo;
            this._userRoleRepo = userRoleRepo;
            this._userRelationRepo = userRelationRepo;
            this._serviceRepo = serviceRepo;
        }

        public BaseResponse getSchedule(EditParams param)
        {
            var scheduleList = this._dbContext.LoadStoredProcedure("md_getSchedule")
                .WithSqlParam("@startDate", param.StartDate.AddDays(-1).Date)
                .WithSqlParam("@endDate", param.EndDate)
                .WithSqlParam("@organizationId", param.OrganizationId)
                .WithSqlParam("@departmentIds", param.departmentIds)
                .WithSqlParam("@serviceLineIds", param.ServiceLineIds)
                .WithSqlParam("@roleIds", param.RoleIds)
                .WithSqlParam("@userIds", (param.ShowAllSchedule == "false" && param.ShowDepartmentSchedule == "false" && param.ShowServiceLineSchedule == "false") ? param.CreatedBy.ToString() : param.UserIds)
                .WithSqlParam("@showAllSchedule", param.ShowAllSchedule)
                .WithSqlParam("@ShowDepartmentSchedule", param.ShowDepartmentSchedule)
                .WithSqlParam("@ShowServiceLineSchedule", param.ShowServiceLineSchedule)
                .WithSqlParam("@ShowOnlyMySchedule", param.ShowOnlyMySchedule)
            .ExecuteStoredProc<ScheduleEventData>();

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = scheduleList };
        }

        public BaseResponse GetScheduleList(ScheduleVM schedule)
        {
            var scheduleList = this._dbContext.LoadStoredProcedure("md_getScheduleListByFilterIds_Dynamic")
                            .WithSqlParam("@orgId", schedule.selectedOrganizationId)
                            .WithSqlParam("@serviceLineIds", schedule.selectedService)
                            .WithSqlParam("@roleIds", schedule.selectedRole)
                            .WithSqlParam("@userIds", schedule.selectedUser)
                            .WithSqlParam("@fromDate", schedule.selectedFromDate.Date.ToUniversalTime().ToString("yyyy-MM-dd"))
                            .WithSqlParam("@toDate", schedule.selectedToDate.ToString("yyyy-MM-dd"))

                            .WithSqlParam("@page", schedule.PageNumber)
                            .WithSqlParam("@size", schedule.Rows)
                            .WithSqlParam("@sortOrder", schedule.SortOrder)
                            .WithSqlParam("@sortCol", schedule.SortCol)
                            .WithSqlParam("@filterVal", schedule.FilterVal)
                            .ExecuteStoredProc<ScheduleListVM>();
            int totalRecords = 0;
            if (scheduleList.Count > 0)
            {
                totalRecords = scheduleList.Select(x => x.Total_Records).FirstOrDefault();
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = new { totalRecords, scheduleList } };
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
                    var user = this._dbContext.LoadStoredProcedure("md_getUserandRoleIdForScheduleImport")
                                    .WithSqlParam("@serviceLineId", fileVM.ServiceLineId)
                                    .WithSqlParam("@roleIds", fileVM.RoleIds)
                                    .WithSqlParam("@initials", initials)
                                    .ExecuteStoredProc<UserRole>().FirstOrDefault();

                    //(from ur in _userRelationRepo.Table
                    //          join u in _userRepo.Table on ur.UserIdFk equals u.UserId
                    //          join urs in this._userRoleRepo.Table on u.UserId equals urs.UserIdFk
                    //          join r in this._roleRepo.Table on urs.RoleIdFk equals r.RoleId
                    //          where ur.ServiceLineIdFk == fileVM.ServiceLineId && fileVM.RoleIds.ToIntList().Contains(r.RoleId) && !r.IsDeleted && !u.IsDeleted && u.Initials == initials
                    //          select u.UserId).FirstOrDefault();

                    //var roleIds = (from ur in _userRelationRepo.Table
                    //               join r in _userRoleRepo.Table on ur.UserIdFk equals r.UserIdFk
                    //               where r.UserIdFk == userId && ur.ServiceLineIdFk == fileVM.ServiceLineId
                    //               select r.RoleIdFk).Distinct().ToList();

                    if (user.UserIdFk > 0)
                    {
                        foreach (var item in dt.Rows)
                        {
                            try
                            {
                                var row = (DataRow)item;
                                index++;

                                if (!string.IsNullOrEmpty(row[0].ToString()) && !string.IsNullOrEmpty(row[1].ToString()))
                                {
                                    var ScheduleDate = DateTime.Parse(row[0].ToString()); //Convert.ToDateTime(row[0]);
                                    var alreadyExist = _scheduleRepo.Table.Where(x => x.UserIdFk == user.UserIdFk && x.ScheduleDateStart.Date == ScheduleDate.Date && !x.IsDeleted).ToList();
                                    if (alreadyExist.Count > 0)
                                    {
                                        listToBeRemoved.AddRange(alreadyExist);
                                    }

                                    var timeRange = row[1].ToString().Split("-").ToArray();

                                    var StartDateTimeStr = row[0].ToString() + " " + timeRange[0];
                                    var EndDateTimeStr = row[0].ToString() + " " + timeRange[1];

                                    var StartDateTime = DateTime.Parse(StartDateTimeStr); //Convert.ToDateTime(StartDateTimeStr);
                                    var EndDateTime = DateTime.Parse(EndDateTimeStr); //Convert.ToDateTime(EndDateTimeStr);
                                    if (EndDateTime.TimeOfDay < StartDateTime.TimeOfDay)
                                    {
                                        EndDateTime = EndDateTime.AddDays(1);
                                    }

                                    var obj = new UsersSchedule()
                                    {
                                        ScheduleDate = StartDateTime.Date.ToUniversalTime(),
                                        ScheduleDateStart = StartDateTime.ToUniversalTime(),
                                        ScheduleDateEnd = EndDateTime.ToUniversalTime(),
                                        UserIdFk = user.UserIdFk,
                                        RoleIdFk = user.RoleIdFk,
                                        ServiceLineIdFk = fileVM.ServiceLineId,
                                        CreatedBy = fileVM.LoggedinUserId,
                                        CreatedDate = DateTime.UtcNow,
                                        IsDeleted = false
                                    };
                                    listToBeInsert.Add(obj);
                                }
                            }
                            catch (Exception ex)
                            {
                                error.Add($"There is an exception at row: {index} \n Exception: {ex}");
                                ElmahExtensions.RiseError(ex);
                            }
                        }

                    }
                    else
                    {
                        error.Add($"'{initials}' is not exist in selected service line.");
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

        public BaseResponse GetScheduleTemplate(int serviceLine, string roleIds)
        {
            var serviceLineUsers = this._dbContext.LoadStoredProcedure("md_getScheduleTemplateByServiceline&Roleids")
                .WithSqlParam("@serviceLine", serviceLine)
                .WithSqlParam("@roleIds", roleIds)
                .ExecuteStoredProc<ScheduleListVM>();
            /*(from u in this._userRepo.Table
                                join urs in this._userRoleRepo.Table on u.UserId equals urs.UserIdFk
                                join r in this._roleRepo.Table on urs.RoleIdFk equals r.RoleId
                                join ur in this._userRelationRepo.Table on u.UserId equals ur.UserIdFk
                                join s in this._serviceRepo.Table on ur.ServiceLineIdFk equals s.ServiceLineId
                                where s.ServiceLineId == serviceLine && roleIds.ToIntList().Contains(r.RoleId) && !r.IsDeleted && !u.IsDeleted && !s.IsDeleted
                                select new
                                {
                                    r.RoleId,
                                    u.Initials,
                                    s.ServiceName
                                }).Distinct().ToList();*/
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
                    row[0] = DateTime.Now.AddDays(i).ToString("MM/dd/yyyy");
                    row[1] = "8:00-18:00";
                    tbl.Rows.Add(row);
                }

                var folderName = Path.Combine("ScheduleTemplates");
                var pathToSave = Path.Combine(this._RootPath, folderName);
                if (!Directory.Exists(pathToSave))
                {
                    Directory.CreateDirectory(pathToSave);
                }
                else
                {
                    DirectoryInfo dir = new DirectoryInfo(pathToSave);
                    foreach (FileInfo fi in dir.GetFiles())
                    {
                        fi.Delete();
                    }
                }
                var fileName = $"Schedule Template For {serviceLineUsers.FirstOrDefault().ServiceName}.csv"; //ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                var fullPath = Path.Combine(pathToSave, fileName);
                new CSVReader().WriteDataTableAsCSV(tbl, fullPath);


                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Template ready", Body = new { path = fullPath.Replace(this._RootPath, ""), fileName = fileName } };
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

                foreach (var u in param.added)
                {
                    u.startTime = DateTime.Parse(u.startTimeStr).ToUniversalTime();
                    u.endTime = DateTime.Parse(u.endTimeStr).ToUniversalTime();
                }

                if (this._scheduleRepo.Table.Any(x => x.UserIdFk == param.selectedUserId.ToInt() && x.ServiceLineIdFk == param.added.ElementAt(0).serviceLineId.ToInt() && x.RoleIdFk == param.added.ElementAt(0).roleId.ToInt() && !x.IsDeleted
                    && ((x.ScheduleDateStart <= param.added.ElementAt(0).startTime && x.ScheduleDateEnd >= param.added.ElementAt(0).startTime) || (x.ScheduleDateStart <= param.added.ElementAt(0).endTime && x.ScheduleDateEnd >= param.added.ElementAt(0).endTime))))
                {
                    response.Status = HttpStatusCode.NotModified;
                    response.Message = "Schedule is already exist";
                    return response;
                }

                scheduleList = (from u in param.added
                                select new UsersSchedule
                                {
                                    ScheduleDate = u.startTime,
                                    ScheduleDateStart = u.startTime,
                                    ScheduleDateEnd = u.endTime,
                                    UserIdFk = param.selectedUserId.ToInt(),
                                    RoleIdFk = u.roleId.ToInt(),
                                    ServiceLineIdFk = u.serviceLineId.ToInt(),
                                    Description = u.subject,
                                    CreatedBy = param.CreatedBy,
                                    CreatedDate = DateTime.UtcNow,
                                    IsDeleted = false
                                }).ToList();
                this._scheduleRepo.Insert(scheduleList);
                //this._dbContext.Log()
                foreach (var inserttedRecord in scheduleList)
                {
                    saveScheduleLog(inserttedRecord, ActivityLogActionEnums.Create.ToInt());
                }
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
                    foreach (var u in param.changed)
                    {
                        u.startTime = DateTime.Parse(u.startTimeStr).ToUniversalTime();
                        u.endTime = DateTime.Parse(u.endTimeStr).ToUniversalTime();
                    }

                    schedule.ScheduleDate = changedSchedule.startTime;
                    schedule.ScheduleDateStart = changedSchedule.startTime;
                    schedule.ScheduleDateEnd = changedSchedule.endTime;
                    schedule.Description = changedSchedule.subject;
                    schedule.UserIdFk = Convert.ToInt32(changedSchedule.userId);
                    schedule.RoleIdFk = changedSchedule.roleId.ToInt();

                    schedule.ServiceLineIdFk = Convert.ToInt32(changedSchedule.serviceLineId);
                    schedule.ModifiedBy = param.ModifiedBy;
                    schedule.ModifiedDate = DateTime.UtcNow;
                    schedule.IsDeleted = false;
                    this._scheduleRepo.Update(schedule);

                    saveScheduleLog(schedule, ActivityLogActionEnums.Update.ToInt());

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

                    saveScheduleLog(schedule, ActivityLogActionEnums.Delete.ToInt());
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
            schedule.FromDate = DateTime.Parse(schedule.FromDateStr); //Convert.ToDateTime(schedule.FromDateStr);
            schedule.ToDate = DateTime.Parse(schedule.ToDateStr); //Convert.ToDateTime(schedule.ToDateStr);
            schedule.StartTime = DateTime.Parse(schedule.StartTimeStr); //Convert.ToDateTime(schedule.StartTimeStr);
            schedule.EndTime = DateTime.Parse(schedule.EndTimeStr); //Convert.ToDateTime(schedule.EndTimeStr);

            if (schedule.ScheduleId > 0)
            {
                var row = _scheduleRepo.Table.Where(x => !x.IsDeleted && x.UsersScheduleId == schedule.ScheduleId).FirstOrDefault();
                if (row != null)
                {

                    if (this._scheduleRepo.Table.Any(x => x.UserIdFk == row.UserIdFk && x.ServiceLineIdFk == row.ServiceLineIdFk && x.RoleIdFk == row.RoleIdFk && !x.IsDeleted
                        && ((x.ScheduleDateStart <= row.ScheduleDateStart && x.ScheduleDateEnd >= row.ScheduleDateStart) || (x.ScheduleDateStart <= row.ScheduleDateEnd && x.ScheduleDateEnd >= row.ScheduleDateEnd))))
                    {
                        string startDateTimeStr = schedule.FromDate.ToString("MM-dd-yyyy") + " " + schedule.StartTime.ToString("hh:mm:ss tt");
                        string endDateTimeStr = schedule.ToDate.ToString("MM-dd-yyyy") + " " + schedule.EndTime.ToString("hh:mm:ss tt");

                        DateTime? startDateTime = Convert.ToDateTime(startDateTimeStr);
                        DateTime? endDateTime = Convert.ToDateTime(endDateTimeStr);

                        if (startDateTime.Value.TimeOfDay > endDateTime.Value.TimeOfDay)
                        {
                            endDateTime.Value.AddDays(1);
                        }

                        row.ScheduleDate = startDateTime.Value.ToUniversalTime();
                        row.ScheduleDateStart = startDateTime.Value.ToUniversalTime();
                        row.ScheduleDateEnd = endDateTime.Value.ToUniversalTime();
                        row.ModifiedBy = schedule.ModifiedBy;
                        row.ModifiedDate = DateTime.UtcNow;
                        row.IsDeleted = false;

                        _scheduleRepo.Update(row);

                        saveScheduleLog(row, ActivityLogActionEnums.Update.ToInt());
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
                string errorMsg = string.Empty;
                var userIds = schedule.UserIdFk.ToIntList();
                var roleIds = schedule.RoleIdFk.ToIntList();
                var usersList = this._userRepo.Table.Where(u => userIds.Contains(u.UserId)).Select(u => new { u.UserId, Name = $"{u.FirstName} {u.LastName}" }).ToList();
                int count = 0;
                var weekDays = schedule.WeekDays.Split(",");
                if (schedule.DateRangeId == 2)  // For Daily
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
                                    var alreadyExist = _scheduleRepo.Table.Where(x => x.UserIdFk == user && x.RoleIdFk == role && x.ServiceLineIdFk == schedule.ServiceLineIdFk && ((x.ScheduleDateStart <= StartDateTime && x.ScheduleDateEnd >= StartDateTime) || (x.ScheduleDateStart <= EndDateTime && x.ScheduleDateEnd >= EndDateTime)) && !x.IsDeleted).ToList();
                                    if (alreadyExist.Count > 0)
                                    {
                                        //alreadyExist.ForEach(x => { x.IsDeleted = true; x.ModifiedBy = schedule.CreatedBy; x.ModifiedDate = DateTime.UtcNow; });
                                        //_scheduleRepo.Update(alreadyExist);
                                        string name = usersList.Where(x => x.UserId == user).Select(x => x.Name).FirstOrDefault();
                                        errorMsg += $"Schedule of {name} is already exist on " + StartDateTime.Value.Date.ToString("MM-dd-yyyy") + Environment.NewLine;
                                        //usersList.RemoveAll(x => x.UserId == user);
                                    }
                                    else
                                    if (_userRelationRepo.Table.Any(x => x.UserIdFk == user && x.ServiceLineIdFk == schedule.ServiceLineIdFk) && _userRoleRepo.Table.Any(x => x.UserIdFk == user && x.RoleIdFk == role))
                                    {
                                        var userSchedule = new UsersSchedule()
                                        {
                                            ScheduleDate = StartDateTime.Value.ToUniversalTime(),
                                            ScheduleDateStart = StartDateTime.Value.ToUniversalTime(),
                                            ScheduleDateEnd = EndDateTime.Value.ToUniversalTime(),
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
                else if (schedule.DateRangeId == 3) // For Weekly
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
                                        var alreadyExist = _scheduleRepo.Table.Where(x => x.UserIdFk == user && x.RoleIdFk == role && x.ServiceLineIdFk == schedule.ServiceLineIdFk && ((x.ScheduleDateStart <= StartDateTime && x.ScheduleDateEnd >= StartDateTime) || (x.ScheduleDateStart <= EndDateTime && x.ScheduleDateEnd >= EndDateTime)) && !x.IsDeleted).ToList();
                                        if (alreadyExist.Count > 0)
                                        {
                                            //alreadyExist.ForEach(x => { x.IsDeleted = true; x.ModifiedBy = schedule.CreatedBy; x.ModifiedDate = DateTime.UtcNow; });
                                            //_scheduleRepo.Update(alreadyExist);
                                            string name = usersList.Where(x => x.UserId == user).Select(x => x.Name).FirstOrDefault();
                                            errorMsg += $"Schedule of {name} is already exist on " + StartDateTime.Value.Date.ToString("MM-dd-yyyy") + Environment.NewLine;
                                            usersList.RemoveAll(x => x.UserId == user);
                                        }
                                        else
                                        if (_userRelationRepo.Table.Any(x => x.UserIdFk == user && x.ServiceLineIdFk == schedule.ServiceLineIdFk) && _userRoleRepo.Table.Any(x => x.UserIdFk == user && x.RoleIdFk == role))
                                        {
                                            var userSchedule = new UsersSchedule()
                                            {
                                                ScheduleDate = StartDateTime.Value.ToUniversalTime(),
                                                ScheduleDateStart = StartDateTime.Value.ToUniversalTime(),
                                                ScheduleDateEnd = EndDateTime.Value.ToUniversalTime(),
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
                else if (schedule.DateRangeId == 4) //For Monthly
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
                                    var alreadyExist = _scheduleRepo.Table.Where(x => x.UserIdFk == user && x.RoleIdFk == role && x.ServiceLineIdFk == schedule.ServiceLineIdFk && ((x.ScheduleDateStart <= StartDateTime && x.ScheduleDateEnd >= StartDateTime) || (x.ScheduleDateStart <= EndDateTime && x.ScheduleDateEnd >= EndDateTime)) && !x.IsDeleted).ToList();
                                    if (alreadyExist.Count > 0)
                                    {
                                        //alreadyExist.ForEach(x => { x.IsDeleted = true; x.ModifiedBy = schedule.CreatedBy; x.ModifiedDate = DateTime.UtcNow; });
                                        //_scheduleRepo.Update(alreadyExist);
                                        string name = usersList.Where(x => x.UserId == user).Select(x => x.Name).FirstOrDefault();
                                        errorMsg += $"Schedule of {name} is already exist on " + StartDateTime.Value.Date.ToString("MM-dd-yyyy") + Environment.NewLine;
                                        usersList.RemoveAll(x => x.UserId == user);
                                    }
                                    else
                                    if (_userRelationRepo.Table.Any(x => x.UserIdFk == user && x.ServiceLineIdFk == schedule.ServiceLineIdFk) && _userRoleRepo.Table.Any(x => x.UserIdFk == user && x.RoleIdFk == role))
                                    {
                                        var userSchedule = new UsersSchedule()
                                        {
                                            ScheduleDate = StartDateTime.Value.ToUniversalTime(),
                                            ScheduleDateStart = StartDateTime.Value.ToUniversalTime(),
                                            ScheduleDateEnd = EndDateTime.Value.ToUniversalTime(),
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
                            if (startDate.Date <= endDate.Date)
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
                                                ScheduleDate = StartDateTime.Value.ToUniversalTime(),
                                                ScheduleDateStart = StartDateTime.Value.ToUniversalTime(),
                                                ScheduleDateEnd = EndDateTime.Value.ToUniversalTime(),
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
                else if (schedule.DateRangeId == 5) //For Yearly
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
                                    var alreadyExist = _scheduleRepo.Table.Where(x => x.UserIdFk == user && x.RoleIdFk == role && x.ServiceLineIdFk == schedule.ServiceLineIdFk && ((x.ScheduleDateStart <= StartDateTime && x.ScheduleDateEnd >= StartDateTime) || (x.ScheduleDateStart <= EndDateTime && x.ScheduleDateEnd >= EndDateTime)) && !x.IsDeleted).ToList();
                                    if (alreadyExist.Count > 0)
                                    {
                                        //alreadyExist.ForEach(x => { x.IsDeleted = true; x.ModifiedBy = schedule.CreatedBy; x.ModifiedDate = DateTime.UtcNow; });
                                        //_scheduleRepo.Update(alreadyExist);
                                        string name = usersList.Where(x => x.UserId == user).Select(x => x.Name).FirstOrDefault();
                                        errorMsg += $"Schedule of {name} is already exist on " + StartDateTime.Value.Date.ToString("MM-dd-yyyy") + Environment.NewLine;
                                        usersList.RemoveAll(x => x.UserId == user);
                                    }
                                    else
                                    if (_userRelationRepo.Table.Any(x => x.UserIdFk == user && x.ServiceLineIdFk == schedule.ServiceLineIdFk) && _userRoleRepo.Table.Any(x => x.UserIdFk == user && x.RoleIdFk == role))
                                    {
                                        var userSchedule = new UsersSchedule()
                                        {
                                            ScheduleDate = StartDateTime.Value.ToUniversalTime(),
                                            ScheduleDateStart = StartDateTime.Value.ToUniversalTime(),
                                            ScheduleDateEnd = EndDateTime.Value.ToUniversalTime(),
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
                    // For Never Repeate
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
                            var alreadyExist = _scheduleRepo.Table.Where(x => x.UserIdFk == user && x.RoleIdFk == role && x.ServiceLineIdFk == schedule.ServiceLineIdFk && ((x.ScheduleDateStart <= StartDateTime && x.ScheduleDateEnd >= StartDateTime) || (x.ScheduleDateStart <= EndDateTime && x.ScheduleDateEnd >= EndDateTime)) && !x.IsDeleted).ToList();
                            if (alreadyExist.Count > 0)
                            {
                                //alreadyExist.ForEach(x => { x.IsDeleted = true; x.ModifiedBy = schedule.CreatedBy; x.ModifiedDate = DateTime.UtcNow; });
                                //_scheduleRepo.Update(alreadyExist);
                                string name = usersList.Where(x => x.UserId == user).Select(x => x.Name).FirstOrDefault();
                                errorMsg += $"Schedule of {name} is already exist on " + StartDateTime.Value.Date.ToString("MM-dd-yyyy") + Environment.NewLine;
                                usersList.RemoveAll(x => x.UserId == user);
                            }
                            else
                            if (_userRelationRepo.Table.Any(x => x.UserIdFk == user && x.ServiceLineIdFk == schedule.ServiceLineIdFk) && _userRoleRepo.Table.Any(x => x.UserIdFk == user && x.RoleIdFk == role))
                            {
                                var userSchedule = new UsersSchedule()
                                {
                                    ScheduleDate = StartDateTime.Value.ToUniversalTime(),
                                    ScheduleDateStart = StartDateTime.Value.ToUniversalTime(),
                                    ScheduleDateEnd = EndDateTime.Value.ToUniversalTime(),
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

                    var serviceName = this._serviceRepo.Table.Where(s => s.ServiceLineId == schedule.ServiceLineIdFk).Select(s => s.ServiceName).FirstOrDefault();
                    var users = this._userRepo.Table.Where(u => usersSchedules.Select(x => x.UserIdFk).Contains(u.UserId)).Select(u => $"{u.FirstName} {u.LastName}").ToArray().Aggregate((a, b) => a + ", " + b);

                    var logDesc = $"schedule of {users} for {serviceName}";
                    this._dbContext.Log(new { }, ActivityLogTableEnums.UsersSchedule.ToString(), 0, ActivityLogActionEnums.Create.ToInt(), null, logDesc);

                    if (!string.IsNullOrEmpty(errorMsg) && !string.IsNullOrWhiteSpace(errorMsg))
                        return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = errorMsg };
                    else
                        return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Schedule Created" };
                }
                else
                {
                    if (!string.IsNullOrEmpty(errorMsg) && !string.IsNullOrWhiteSpace(errorMsg))
                        return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = errorMsg };
                    else
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
                saveScheduleLog(schedule, ActivityLogActionEnums.Delete.ToInt());
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted." };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "No Schedule Found" };
            }
        }

        public void saveScheduleLog(UsersSchedule schedule, int action)
        {
            var selectedUserFullName = this._userRepo.Table.Where(u => u.UserId == schedule.UserIdFk).Select(u => $"{u.FirstName} {u.LastName}").FirstOrDefault();
            var serviceName = this._serviceRepo.Table.Where(s => s.ServiceLineId == schedule.ServiceLineIdFk).Select(s => s.ServiceName).FirstOrDefault();
            var logDesc = $"schedule of {selectedUserFullName} for {serviceName} from {schedule.ScheduleDateStart.ToString()} to {schedule.ScheduleDateStart.ToString()}";
            this._dbContext.Log(schedule, ActivityLogTableEnums.UsersSchedule.ToString(), schedule.UsersScheduleId, action, null, logDesc);
        }
    }
}
