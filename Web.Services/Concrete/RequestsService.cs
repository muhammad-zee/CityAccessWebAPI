using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Web.Data.Models;
using Web.DLL.Generic_Repository;
using Web.Model;
using Web.Model.Common;
using Web.Services.Enums;
using Web.Services.Helper;
using Web.Services.Interfaces;

namespace Web.Services.Concrete
{
    public class RequestsService : IRequestsService
    {

        private readonly CityAccess_DbContext _dbContext;
        private readonly IGenericRepository<Partner> _partnersRepo;
        private readonly IGenericRepository<Agreement> _agreementsRepo;
        private readonly IGenericRepository<Request> _requestsRepo;
        private readonly IGenericRepository<RequestLog> _requestLogsRepo;
        private readonly IGenericRepository<User> _usersRepo;
        private readonly IGenericRepository<Service> _servicesRepo;
        private readonly IGenericRepository<ServiceType> _servicetypesRepo;
        private readonly IGenericRepository<Event> _eventsRepo;
        private readonly IEmailService _emailService;

        public RequestsService(CityAccess_DbContext dbContext,
            IGenericRepository<Partner> partnersRepo,
            IGenericRepository<Agreement> agreementsRepo,
            IGenericRepository<Request> requestsRepo,
            IGenericRepository<RequestLog> requestLogsRepo,
            IGenericRepository<User> usersRepo,
            IGenericRepository<Service> servicesRepo, IGenericRepository<ServiceType> servicetypesRepo,
            IGenericRepository<Event> eventsRepo,
            IEmailService emailService)

        {
            this._dbContext = dbContext;
            this._partnersRepo = partnersRepo;
            this._agreementsRepo = agreementsRepo;
            this._requestsRepo = requestsRepo;
            this._requestLogsRepo = requestLogsRepo;
            this._usersRepo = usersRepo;
            this._servicesRepo = servicesRepo;
            this._servicetypesRepo = servicetypesRepo;
            this._eventsRepo = eventsRepo;
            this._emailService = emailService;
        }

        public BaseResponse GetRequestsToUs(RequestsFilterVM filter)
        {
            int partnerID = ApplicationSettings.PartnerId;
            //Partner partner = this._partnersRepo.Table.Where(p => p.Id == partnerID && p.IsActive != false).FirstOrDefault();

            var requests = this._dbContext.LoadStoredProcedure("ca_getRequestsToUsByPartnerId")
                            .WithSqlParam("@pPartnerId", partnerID)
                            .WithSqlParam("@pOperatorId", filter.OperatorId)
                            .WithSqlParam("@pServiceId", filter.ServiceId)
                            .WithSqlParam("@pStatus", filter.Status)
                            .WithSqlParam("@pServiceStartDate", filter.ServiceStartDate)
                            .WithSqlParam("@pServiceEndDate", filter.ServiceEndDate)
                            .WithSqlParam("@pBookingStartDate", filter.BookingStartDate)
                            .WithSqlParam("@pBookingEndDate", filter.BookingEndDate)
                            .ExecuteStoredProc<RequestVM>();

            // List<SelectListItem> items = new List<SelectListItem>();
            // items.Add(new SelectListItem() { Text = "Submitted", Value = "Submitted" });
            // items.Add(new SelectListItem() { Text = "Approved", Value = "Approved" });
            // items.Add(new SelectListItem() { Text = "Canceled", Value = "Canceled" });
            // items.Add(new SelectListItem() { Text = "Invoiced", Value = "Invoiced" });
            // items.Add(new SelectListItem() { Text = "All", Value = "All" });

            //  bookingFilters.StatusFilters = items;
            //  bookingFilters.Req_PartnerList = requests.ToList();
            // ViewBag.stateID = this.States.Where(x => x.ID != "Site Approval" && x.ID != "Site Canceled"); 
            //Good for the time being, but not scalable, think of a better solution
            // ViewBag.partners = db.Partners;




            // BookingFilters bookingFilters1 = new BookingFilters { Req_PartnerList = reqs, StatusFilters = items, Status = bookingFilters.Status };


            return new BaseResponse { Status = HttpStatusCode.OK, Message = "Data returned", Body = requests };

        }
        public BaseResponse GetRequestsToUsForCalendar(RequestsFilterVM filter)
        {
            int partnerID = ApplicationSettings.PartnerId;
            //Partner partner = this._partnersRepo.Table.Where(p => p.Id == partnerID && p.IsActive != false).FirstOrDefault();
            var requests = this._dbContext.LoadStoredProcedure("ca_getRequestsToUsByPartnerId")
                            .WithSqlParam("@pPartnerId", partnerID)
                            .WithSqlParam("@pOperatorId", filter.OperatorId)
                            .WithSqlParam("@pServiceId", filter.ServiceId)
                            .WithSqlParam("@pStatus", filter.Status)
                            .WithSqlParam("@pServiceStartDate", filter.ServiceStartDate)
                            .WithSqlParam("@pServiceEndDate", filter.ServiceEndDate)
                            .WithSqlParam("@pBookingStartDate", filter.BookingStartDate)
                            .WithSqlParam("@pBookingEndDate", filter.BookingEndDate)
                            .ExecuteStoredProc<RequestVM>();
            var requestsCalendarView = BuildTree(requests);


            return new BaseResponse { Status = HttpStatusCode.OK, Message = "dataretrund", Body = requestsCalendarView };

        }
        private IList<RequestsCalendarViewVM> BuildTree(IEnumerable<RequestVM> reqList)
        {
            List<RequestsCalendarViewVM> calendarView = new();
            List<RequestsTimeSlots> timeSlots = null;
            var dateList = reqList.Select(r => r.EventDate).Distinct();
            foreach (var date in dateList)
            {
                timeSlots = new();
                var timeList = reqList.Where(r => r.EventDate == date).Select(r => r.EventTime).Distinct();
                foreach (var time in timeList)
                {
                    var requests = reqList.Where(r => r.EventTime == time).ToList();
                    var slot = new RequestsTimeSlots
                    {
                        Time = time,
                        Requests = requests
                    };
                    timeSlots.Add(slot);
                }
                calendarView.Add(new RequestsCalendarViewVM
                {
                    Date = date,
                    Slotes = timeSlots
                });
            }
            return calendarView;
        }
        public BaseResponse GetRequestDetail(int requestId)
        {
            var request = this._requestsRepo.Table.FirstOrDefault(r => r.Id == requestId);
            var agreement = this._agreementsRepo.Table.FirstOrDefault(a => a.Id == request.AgreementId);
            var partner = this._partnersRepo.Table.FirstOrDefault(p => p.Id == agreement.PartnerId && p.IsActive == true);
            var requestVM = new RequestVM
            {
                ID = request.Id,
                AgreementID = request.AgreementId,
                AgreementName = agreement.Label,
                PartnerName = partner.TradeName,
                Price = request.Price,
                EventDate = request.EventDate,
                EventTime = request.EventTime,
                Notes = request.Notes,
                ContactName = request.ContactName,
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone,
                NrPersons = request.NrPersons,
                PickupLocation = request.PickupLocation,
                DropoffLocation = request.DropoffLocation,
                ReturnPickup = request.ReturnPickup,
                ReturnDropoff = request.ReturnDropoff,
                FlightNr = request.FlightNr,
                ReturnDate = request.ReturnDate,
                ReturnTime = request.ReturnTime,
                ReturnFlight = request.ReturnFlight,
                StateID = request.StateId,
                Reference = request.Reference,
                BookDate = request.BookDate,
                BookTime = request.BookTime,
                ExtraDate1 = request.ExtraDate1,
                ExtraDate2 = request.ExtraDate2,
                //ExtraDate3=request.ExtraDate3,
                ExtraTime1 = request.ExtraTime1,
                //ExtraTime2=request.ExtraTime2,
                //ExtraTime3=request.ExtraTime3,
                ExtraText1 = request.ExtraText1,
                //ExtraText2=request.ExtraText2,
                //ExtraText3=request.ExtraText3,
                ExtraMultiText1 = request.ExtraMultiText1,
                //ExtraMultiText2=request.ExtraMultiText2,
                //ExtraMultiText3=request.ExtraMultiText3,
                //OperatorNotes=request.OperatorNotes,
                //EventId=request.EventId,
                //ResponsibleId=request.ResponsibleId,
                ClientNotes = request.ClientNotes,
                ServiceImage = "/Images/logo.png"

            };
            return new BaseResponse
            {
                Status = HttpStatusCode.OK,
                Message = "Request detail returned",
                Body = requestVM
            };
        }
        private void exportServicesToExcelSheet()
        {

            //StringWriter sw = new StringWriter();
            //var requestsToUs = new System.Data.DataTable("requests");
            //requestsToUs.Columns.Add("Request", typeof(string));
            //requestsToUs.Columns.Add("Service Name", typeof(string));
            //requestsToUs.Columns.Add("Partner", typeof(string));
            //requestsToUs.Columns.Add("Customer", typeof(string));
            //requestsToUs.Columns.Add("Date", typeof(string));
            //requestsToUs.Columns.Add("Time", typeof(TimeSpan));
            //requestsToUs.Columns.Add("Pax", typeof(int));
            //requestsToUs.Columns.Add("Status", typeof(string));
            //requestsToUs.Columns.Add("Pick up Location", typeof(string));
            //requestsToUs.Columns.Add("Flight number", typeof(string));
            //requestsToUs.Columns.Add("Drop off Location", typeof(string));
            //requestsToUs.Columns.Add("Price", typeof(string));
            //requestsToUs.Columns.Add("Com.", typeof(string));
            //requestsToUs.Columns.Add("Client Name", typeof(string));
            //requestsToUs.Columns.Add("Notes", typeof(string));
            //requestsToUs.Columns.Add("Operator Notes", typeof(string));


            //string partnerTradename;

            //foreach (var item in bookingFilters.Req_PartnerList)
            //{
            //    if (item.Partner == null)
            //    {
            //        partnerTradename = "General Request";
            //    }
            //    else
            //    {
            //        partnerTradename = item.Partner.tradeName;
            //    }

            //    requestsToUs.Rows.Add(
            //        item.req_ForTransfer.ID,
            //        item.Agreement.label,
            //        partnerTradename,
            //        item.req_ForTransfer.ContactName,
            //        item.req_ForTransfer.EventDate.Value.ToString("dd/MM/yyyy"),
            //        item.req_ForTransfer.EventTime,
            //        item.req_ForTransfer.NrPersons,
            //        item.req_ForTransfer.StateID,
            //        item.req_ForTransfer.PickupLocation,
            //        item.req_ForTransfer.FlightNr,
            //        item.req_ForTransfer.DropoffLocation,
            //        item.req_ForTransfer.Price.ToString().Replace(".", ","),
            //        item.Agreement.commissionValue.ToString().Replace(".", ","),
            //        item.req_ForTransfer.ContactName,
            //        item.req_ForTransfer.Notes,
            //        item.req_ForTransfer.OperatorNotes
            //        );

            //    System.Diagnostics.Debug.WriteLine(item.req_ForTransfer.Price.ToString().Replace(".", ","));
            //}

            //var grid = new GridView();
            //grid.DataSource = requestsToUs;
            //grid.DataBind();

            //Response.ClearContent();
            //Response.Buffer = true;
            //Response.ClearContent();
            //Response.AddHeader("content-disposition", "attachement;filename=ServicesRequested.xls");
            //Response.ContentType = "application/ms-excel";

            //HtmlTextWriter htw = new HtmlTextWriter(sw);

            //grid.RenderControl(htw);

            //Response.Write(sw.ToString());
            //Response.Flush();
            //Response.End();


        }
        public BaseResponse SaveBookingRequest(RequestVM requestParam)
        {
            bool? FromPartnerSite = requestParam.FromPartnerSite == null ? false : requestParam.FromPartnerSite;
            BaseResponse response = new BaseResponse();
            Request request = new Request
            {
                ContactName = requestParam.ContactName,
                ContactEmail = requestParam.ContactEmail,
                ContactPhone = requestParam.ContactPhone,
                EventDate = requestParam.EventDate,
                EventTime = requestParam.EventTime,
                NrPersons = requestParam.NrPersons,
                Price = requestParam.Price,
                Reference = requestParam.Reference,
                Notes = requestParam.Notes,
                //IsTransfer=requestParam.IsTransfer,
                PickupLocation = requestParam.PickupLocation,
                DropoffLocation = requestParam.DropoffLocation,
                FlightNr = requestParam.FlightNr,
                //HasReturn=requestParam.hasreturn,
                ReturnDate = requestParam.ReturnDate,
                ReturnTime = requestParam.ReturnTime,
                ReturnFlight = requestParam.ReturnFlight,
                ReturnPickup = requestParam.ReturnPickup,
                ReturnDropoff = requestParam.ReturnDropoff,
                ExtraDate1 = requestParam.ExtraDate1,
                ExtraDate2 = requestParam.ExtraDate2,
                ExtraTime1 = requestParam.ExtraTime1,
                ExtraText1 = requestParam.ExtraText1,
                ExtraMultiText1 = requestParam.ExtraMultiText1,
                ClientNotes = requestParam.ClientNotes,
                BookDate = requestParam.BookDate,
                BookTime = requestParam.BookTime
            };
            request.AgreementId = requestParam.AgreementID;
            request.BookDate = System.DateTime.Now;

            DateTime midnigth = new DateTime(request.BookDate.Value.Year, request.BookDate.Value.Month, request.BookDate.Value.Day, 0, 0, 0);

            request.BookTime = request.BookDate - midnigth;

            var ag = this._agreementsRepo.Table.Where(z => z.Id == request.AgreementId).FirstOrDefault();
            var serv = this._servicesRepo.Table.Where(p => p.Id == ag.ServiceId).FirstOrDefault();
            var opr = this._partnersRepo.Table.Where(a => a.Id == serv.OperatorId).FirstOrDefault();
            var part = new Partner();
            if (FromPartnerSite == true)
            {
                request.BookerId = 1;
                request.StateId = Constants.StateTransitions.SiteApproval;
            }
            else
            {
                request.BookerId = ApplicationSettings.UserId;
                part = this._partnersRepo.Table.FirstOrDefault(p => p.Id == ApplicationSettings.PartnerId);
                if (ag.NeedsApproval == true)
                {
                    request.StateId = Constants.StateTransitions.Submitted;
                }
                else
                {
                    request.StateId = Constants.StateTransitions.Approved;
                }
            }
            this._requestsRepo.Insert(request);

            Request req = request;
            int requestID = req.Id;

            RequestLog reqLog = new RequestLog();
            reqLog.Date = DateTime.Now;
            reqLog.Time = DateTime.Now.ToString("HH:mm");
            reqLog.RequestId = requestID;
            reqLog.UserId = ApplicationSettings.UserId;
            reqLog.Notes = FromPartnerSite == true ? "Booking created by site request!" : "Booking created!";
            this._requestLogsRepo.Insert(reqLog);

            //Set up and forwarding e - mail to operator
            this.SendServiceBookedEmail(req, part, ag, serv, opr, FromPartnerSite);
            response.Status = HttpStatusCode.OK;
            response.Message = "Service booked successfully";
            return response;
        }
        private void SendServiceBookedEmail(Request request, Partner part, Agreement ag, Service serv, Partner opr, bool? FromPartnerSite)
        {

            string og = " " + request.EventDate.ToString("dd-MM-yyyy");
            string date = og.Replace("12:00:00 AM", " ");
            var link = "";//Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "/");
            string url = "ServicesRequested/Details/" + request.Id;
            url = link + url;
            string subject = string.Empty;
            string content = string.Empty;
            if (FromPartnerSite == null || FromPartnerSite == false)
            {
                if (ag.NeedsApproval == true)
                {
                    subject = "New request#" + request.Id + " " + ag.Label + "-" + part.TradeName + "-" + date + "-" + request.NrPersons + "-needs approval";
                }
                else
                {
                    subject = "New request#" + request.Id + " " + ag.Label + "-" + part.TradeName + "-" + date + "-" + request.NrPersons + "-approved";
                }

                content = "New request from " + part.TradeName + ":<br> ";
                if (request.ReturnDate == null)
                {

                    content = content + "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>" +
                        "<tr><th>Field</th><th>Value</th></tr></thead><tbody><tr><td>Service</td><td>" + ag.Label + "</td></tr>"
                        + "<tr><td> Operator </td><td> " + opr.TradeName + " </td></tr>"
                        + "<tr><td> Agent </td><td> " + ag.Partner.TradeName + " </td></tr>"
                        + "<tr><td> Date </td><td> " + og + " </td></tr>" +
                        "<tr><td>Time</td><td>" + request.EventTime + "</td></tr>" +
                        "<tr><td>Client name</td><td>" + request.ContactName + "</td></tr>" +
                        "<tr><td>Client e-mail</td><td>" + request.ContactEmail + "</td></tr>" +
                        "<tr><td>Client phone</td><td>" + request.ContactPhone + "</td></tr>" +
                        "<tr><td>Nº of persons</td><td>" + request.NrPersons + "</td></tr>" +
                        "<tr><td>Price</td><td>" + request.Price + "</td></tr>";
                    if (request.PickupLocation != null)
                    {
                        content = content +
                        "<tr><td>Pick up location</td><td>" + request.PickupLocation + "</td></tr>";
                    }
                    if (request.DropoffLocation != null)
                    {
                        content = content +
                        "<tr><td>Dropoff location</td><td>" + request.DropoffLocation + "</td></tr>";
                    }
                    if (request.FlightNr != null)
                    {
                        content = content +
                        "<tr><td>Flight number</td><td>" + request.FlightNr + "</td></tr>";
                    }
                    content = content +
                        "<tr><td>Notes</td><td>" + request.Notes + "</td></tr>" +
                        "<tr><td>Operator notes</td><td>" + request.OperatorNotes + "</td></tr></tbody></table>";
                }
                else
                {
                    og = " " + request.ReturnDate;
                    string returnDate = " " + request.ReturnDate?.ToString("dd-MM-yyyy");

                    content = content + "<br/><br><table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>"
                         + "<tr><th>Field</th><th>Value</th></tr></thead><tbody><tr><td>Service</td><td>" + serv.Name + "</td></tr>"
                         + "<tr><td> Operator </td><td> " + opr.TradeName + " </td></tr>"
                         + "<tr><td> Agent </td><td> " + ag.Partner.TradeName + " </td></tr>"
                         + "<tr><td> Date </td><td> " + og + " </td></tr>" +
                         "<tr><td>Time</td><td>" + request.EventTime + "</td></tr>" +
                         "<tr><td>Client name</td><td>" + request.ContactName + "</td></tr>" +
                         "<tr><td>Client e-mail</td><td>" + request.ContactEmail + "</td></tr>" +
                         "<tr><td>Client phone</td><td>" + request.ContactPhone + "</td></tr>" +
                         "<tr><td>Nº of persons</td><td>" + request.NrPersons + "</td></tr>" +
                         "<tr><td>Price</td><td>" + request.Price + "</td></tr>" +
                         "<tr><td>Pick up location</td><td>" + request.PickupLocation + "</td></tr>" +
                         "<tr><td>Dropoff location</td><td>" + request.DropoffLocation + "</td></tr>" +
                         "<tr><td>Flight number</td><td>" + request.FlightNr + "</td></tr>" +
                         "<tr><td>Return Date</td><td>" + returnDate + "</td></tr>" +
                         "<tr><td>Return Time</td><td>" + request.ReturnTime + "</td></tr>" +
                         "<tr><td>Return flight number</td><td>" + request.ReturnFlight + "</td></tr>" +
                         "<tr><td>Return pickup</td><td>" + request.ReturnPickup + "</td></tr>" +
                         "<tr><td>Return dropoff</td><td>" + request.ReturnDropoff + "</td></tr>" +
                         "<tr><td>Notes</td><td>" + request.Notes + "</td></tr>" +
                         "<tr><td>Operator notes</td><td>" + request.OperatorNotes + "</td></tr></tbody></table>";
                }
                this._emailService.Email_to_send(opr.Email, url, content, subject);
            }
            else
            {
                if (ag.NeedsApproval == true)
                {
                    subject = "New request#" + request.Id + " " + ag.Label + "-from client " + request.ContactName + "-" + date + "-" + request.NrPersons + "-needs approval";
                }
                else
                {
                    subject = "New request#" + request.Id + " " + ag.Label + "-from client" + request.ContactName + "-" + date + "-" + request.NrPersons + "-approved";
                }

                content = "New request from " + request.ContactName + ":<br> ";

                if (request.ReturnDate == null)
                {

                    content = content + "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>" +
                        "<tr><th>Field</th><th>Value</th></tr></thead><tbody><tr><td>Service</td><td>" + ag.Label + "</td></tr>"
                        + "<tr><td> Operator </td><td> " + opr.TradeName + " </td></tr>"
                        + "<tr><td> Agent </td><td> " + ag.Partner.TradeName + " </td></tr>"
                        + "<tr><td> Date </td><td> " + og + " </td></tr>" +
                        "<tr><td>Time</td><td>" + request.EventTime + "</td></tr>" +
                        "<tr><td>Client name</td><td>" + request.ContactName + "</td></tr>" +
                        "<tr><td>Client e-mail</td><td>" + request.ContactEmail + "</td></tr>" +
                        "<tr><td>Client phone</td><td>" + request.ContactPhone + "</td></tr>" +
                        "<tr><td>Nº of persons</td><td>" + request.NrPersons + "</td></tr>" +
                        "<tr><td>Price</td><td>" + request.Price + "</td></tr>";
                    if (request.PickupLocation != null)
                    {
                        content = content +
                        "<tr><td>Pick up location</td><td>" + request.PickupLocation + "</td></tr>";
                    }
                    if (request.DropoffLocation != null)
                    {
                        content = content +
                        "<tr><td>Dropoff location</td><td>" + request.DropoffLocation + "</td></tr>";
                    }
                    if (request.FlightNr != null)
                    {
                        content = content +
                        "<tr><td>Flight number</td><td>" + request.FlightNr + "</td></tr>";
                    }
                    content = content +
                        "<tr><td>Notes</td><td>" + request.Notes + "</td></tr>" +
                        "<tr><td>Operator notes</td><td>" + request.OperatorNotes + "</td></tr></tbody></table>";
                }
                else
                {
                    og = " " + request.ReturnDate;
                    string returnDate = " " + request.ReturnDate?.ToString("dd-MM-yyyy");

                    content = content + "<br/><br><table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>"
                         + "<tr><th>Field</th><th>Value</th></tr></thead><tbody><tr><td>Service</td><td>" + serv.Name + "</td></tr>"
                         + "<tr><td> Operator </td><td> " + opr.TradeName + " </td></tr>"
                         + "<tr><td> Agent </td><td> " + ag.Partner.TradeName + " </td></tr>"
                         + "<tr><td> Date </td><td> " + og + " </td></tr>" +
                         "<tr><td>Time</td><td>" + request.EventTime + "</td></tr>" +
                         "<tr><td>Client name</td><td>" + request.ContactName + "</td></tr>" +
                         "<tr><td>Client e-mail</td><td>" + request.ContactEmail + "</td></tr>" +
                         "<tr><td>Client phone</td><td>" + request.ContactPhone + "</td></tr>" +
                         "<tr><td>Nº of persons</td><td>" + request.NrPersons + "</td></tr>" +
                         "<tr><td>Price</td><td>" + request.Price + "</td></tr>" +
                         "<tr><td>Pick up location</td><td>" + request.PickupLocation + "</td></tr>" +
                         "<tr><td>Dropoff location</td><td>" + request.DropoffLocation + "</td></tr>" +
                         "<tr><td>Flight number</td><td>" + request.FlightNr + "</td></tr>" +
                         "<tr><td>Return Date</td><td>" + returnDate + "</td></tr>" +
                         "<tr><td>Return Time</td><td>" + request.ReturnTime + "</td></tr>" +
                         "<tr><td>Return flight number</td><td>" + request.ReturnFlight + "</td></tr>" +
                         "<tr><td>Return pickup</td><td>" + request.ReturnPickup + "</td></tr>" +
                         "<tr><td>Return dropoff</td><td>" + request.ReturnDropoff + "</td></tr>" +
                         "<tr><td>Notes</td><td>" + request.Notes + "</td></tr>" +
                         "<tr><td>Operator notes</td><td>" + request.OperatorNotes + "</td></tr></tbody></table>";
                }
                this._emailService.Email_to_send(part.Email, url, content, subject);
            }


        }


        public BaseResponse UpdateRequestStatus(int requestId, string stateId)
        {

            Request req = this._requestsRepo.Table.FirstOrDefault(r => r.Id == requestId);
            var ag = this._agreementsRepo.Table.Where(z => z.Id == req.AgreementId).FirstOrDefault();
            var serv = this._servicesRepo.Table.Where(p => p.Id == ag.ServiceId).FirstOrDefault();
            var opr = this._partnersRepo.Table.Where(a => a.Id == serv.OperatorId).FirstOrDefault();
            var part = this._partnersRepo.Table.Where(p => p.Id == ApplicationSettings.PartnerId).FirstOrDefault();
            var user = this._usersRepo.Table.Where(u => u.Id == ApplicationSettings.UserId).FirstOrDefault();
            string Changes = "-Status  from " + req.StateId + " to " + stateId + "\n";
            req.StateId = stateId;

            this._requestsRepo.Update(req);
            //DB data needed for the e-mail
            var agentUser = this._usersRepo.Table.FirstOrDefault(x => x.Id == req.BookerId);

            RequestLog reqLog = new RequestLog();
            reqLog.Date = System.DateTime.Now;
            reqLog.Time = System.DateTime.Now.ToString("HH:mm");
            reqLog.RequestId = req.Id;
            reqLog.UserId = ApplicationSettings.UserId;
            reqLog.Notes = Changes;
            this._requestLogsRepo.Insert(reqLog);


            //general info about the request
            string reqDetails = String.Empty;

            string og = " " + req.EventDate.ToString("dd-MM-yyyy");
            string date = og.Replace("12:00:00 AM", " ");

            Changes = "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>"
                + "<tr><th>Field</th><th>Old value</th><th>New value</th></tr></thead><tbody><tr><td>Status</td><td>" + req.Notes + "</td><td>" + req.StateId + "</td></tr></tbody></table>";

            reqDetails = "<br/><br><p></p>Request general details:<br/><br> ";

            if (req.ReturnDate == null)
            {

                reqDetails = reqDetails + "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>"
                    + "<tr><th>Field</th><th>Value</th></tr></thead><tbody><tr><td>Service</td><td>" + serv.Name + "</td></tr>"
                    + "<tr><td> Operator </td><td> " + part.TradeName + " </td></tr>"
                    + "<tr><td> Agent </td><td> " + agentUser.Partner.TradeName + " </td></tr>"
                    + "<tr><td> Date </td><td> " + og + " </td></tr>" +
                    "<tr><td>Time</td><td>" + req.EventTime + "</td></tr>" +
                    "<tr><td>Client name</td><td>" + req.ContactName + "</td></tr>" +
                    "<tr><td>Client e-mail</td><td>" + req.ContactEmail + "</td></tr>" +
                    "<tr><td>Client phone</td><td>" + req.ContactPhone + "</td></tr>" +
                    "<tr><td>Nº of persons</td><td>" + req.NrPersons + "</td></tr>" +
                    "<tr><td>Price</td><td>" + req.Price + "</td></tr>";
                if (req.PickupLocation != null)
                {
                    reqDetails = reqDetails +
                    "<tr><td>Pick up location</td><td>" + req.PickupLocation + "</td></tr>";
                }
                if (req.DropoffLocation != null)
                {
                    reqDetails = reqDetails +
                    "<tr><td>Dropoff location</td><td>" + req.DropoffLocation + "</td></tr>";
                }
                if (req.FlightNr != null)
                {
                    reqDetails = reqDetails +
                    "<tr><td>Flight number</td><td>" + req.FlightNr + "</td></tr>";
                }

                reqDetails = reqDetails +
                    "<tr><td>Client Notes</td><td>" + req.ClientNotes + "</td></tr>" +
                    "<tr><td>Notes</td><td>" + req.Notes + "</td></tr>" +
                    "<tr><td>Operator notes</td><td>" + req.OperatorNotes + "</td></tr></tbody></table>";
            }
            else
            {
                //og = " " + req.returnDate;
                string returnDate = " " + req.ReturnDate?.ToString("dd-MM-yyyy");

                reqDetails = reqDetails + "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>"
                     + "<tr><th>Field</th><th>Value</th></tr></thead><tbody><tr><td>Service</td><td>" + serv.Name + "</td></tr>"
                     + "<tr><td> Operator </td><td> " + part.TradeName + " </td></tr>"
                     + "<tr><td> Agent </td><td> " + agentUser.Partner.TradeName + " </td></tr>"
                     + "<tr><td> Date </td><td> " + og + " </td></tr>" +
                     "<tr><td>Time</td><td>" + req.EventTime + "</td></tr>" +
                     "<tr><td>Client name</td><td>" + req.ContactName + "</td></tr>" +
                     "<tr><td>Client e-mail</td><td>" + req.ContactEmail + "</td></tr>" +
                     "<tr><td>Client phone</td><td>" + req.ContactPhone + "</td></tr>" +
                     "<tr><td>Nº of persons</td><td>" + req.NrPersons + "</td></tr>" +
                     "<tr><td>Price</td><td>" + req.Price + "</td></tr>" +
                     "<tr><td>Pick up location</td><td>" + req.PickupLocation + "</td></tr>" +
                     "<tr><td>Dropoff location</td><td>" + req.DropoffLocation + "</td></tr>" +
                     "<tr><td>Flight number</td><td>" + req.FlightNr + "</td></tr>" +
                     "<tr><td>Return Date</td><td>" + returnDate + "</td></tr>" +
                     "<tr><td>Return Time</td><td>" + req.ReturnTime + "</td></tr>" +
                     "<tr><td>Return flight number</td><td>" + req.ReturnFlight + "</td></tr>" +
                     "<tr><td>Return pickup</td><td>" + req.ReturnPickup + "</td></tr>" +
                     "<tr><td>Return dropoff</td><td>" + req.ReturnDropoff + "</td></tr>" +
                     "<tr><td>Client Notes</td><td>" + req.ClientNotes + "</td></tr>" +
                     "<tr><td>Notes</td><td>" + req.Notes + "</td></tr>" +
                     "<tr><td>Operator notes</td><td>" + req.OperatorNotes + "</td></tr></tbody></table>";
            }

            //Set up and forwarding e-mails
            string time = System.DateTime.Now.ToString("HH:mm");
            var link = "";// Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "/");

            string url = "ServicesBooked/Edit/" + req.Id;
            url = link + url;

            string subject = "Booking " + req.Agreement.Label + "#" + req.Id + "-" + part.TradeName + " was changed.";


            Changes = "<br/><br>" + Changes;
            string content = this._emailService.AgentContent(user.FullName, time) + Changes + reqDetails;

            this._emailService.Email_to_send(agentUser.Partner.Email, url, content, subject);

            url = "ServicesRequested/Details/" + req.Id;
            url = link + url;

            subject = "Request " + req.Agreement.Label + "#" + req.Id + "-" + agentUser.Partner.TradeName + " was changed.";

            content = this._emailService.OperatorEditorContent(part.TradeName, time) + Changes + reqDetails;

            this._emailService.Email_to_send(part.Email, url, content, subject);

            return new BaseResponse
            {
                Status = HttpStatusCode.OK,
                Message = "Request status updated"
            };
        }
        public BaseResponse UpdateRequestAssignee(int requestId, string stateID, string operatorNotes,string ResponsibleID)
        {


            int? responsibleID = null;

            if (ResponsibleID != "")
            {
                responsibleID = Int32.Parse(ResponsibleID);
            }



            Request req = this._requestsRepo.Table.FirstOrDefault(r => r.Id == requestId);
            var ag = this._agreementsRepo.Table.Where(z => z.Id == req.AgreementId).FirstOrDefault();
            var serv = this._servicesRepo.Table.Where(p => p.Id == ag.ServiceId).FirstOrDefault();
            var opr = this._partnersRepo.Table.Where(a => a.Id == serv.OperatorId).FirstOrDefault();
            var part = this._partnersRepo.Table.Where(p => p.Id == ApplicationSettings.PartnerId).FirstOrDefault();
            var user = this._usersRepo.Table.Where(u => u.Id == ApplicationSettings.UserId).FirstOrDefault();
            var newResponsible = this._usersRepo.Table.Where(u => u.Id == responsibleID).FirstOrDefault();

            string Changes = string.Empty;

            string Changes1 = "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>" +
                "<tr><th>Field</th><th>Old value</th><th>New value</th></tr></thead><tbody>";
            if (req.StateId != stateID && stateID != "")
            {
                Changes = "-Status  from " + req.StateId + " to " + stateID + "\n";
                Changes1 = Changes1 + "<tr><td>Status</td><td>" + req.StateId + "</td><td>" + stateID + "</td></tr>";
                req.StateId = stateID;
            }
            if (req.OperatorNotes == null && operatorNotes != "")
            {
                Changes = Changes + "-Operator notes from no value to " + operatorNotes + "\n";
                Changes1 = Changes1 + "<tr><td>Operator notes</td><td>No value</td><td>" + operatorNotes + "</td></tr>";
                req.OperatorNotes = operatorNotes;
            }
            else
            {
                if (req.OperatorNotes != operatorNotes && operatorNotes != "")
                {
                    Changes = Changes + "-Operator notes from " + req.OperatorNotes + " to " + operatorNotes + "\n";
                    Changes1 = Changes1 + "<tr><td>Operator notes</td><td>" + req.OperatorNotes + "</td><td>" + operatorNotes + "</td></tr>";
                    req.OperatorNotes = operatorNotes;
                }
            }
            if (req.ResponsibleId != responsibleID && (newResponsible != null || responsibleID == 0))
            {
                string oldResponsible = " no value ";

                string NewResponsible = "no value";

                if (req != null)
                {
                    var User1 = this._usersRepo.Table.FirstOrDefault(x => x.Id == req.BookerId);
                    oldResponsible = User1.FullName;
                }

                if (newResponsible != null)
                {
                    NewResponsible = newResponsible.FullName;
                }

                Changes = Changes + "- Assigned from " + oldResponsible + " to " + NewResponsible + "\n";
                Changes1 = Changes1 + "<tr><td>Assigned</td><td>" + oldResponsible + "</td><td>" + NewResponsible + "</td></tr>";

                if (responsibleID == 0)
                {
                    responsibleID = null;
                }

                req.ResponsibleId = responsibleID;
            }

            Changes1 = Changes1 + "</tbody></table>";

            //DB data needed for the e-mail
            var agentUser = this._usersRepo.Table.Where(x => x.Id == req.BookerId).FirstOrDefault();
            agentUser.Partner = this._partnersRepo.Table.FirstOrDefault(x => x.Id == agentUser.PartnerId && x.IsActive == true);

                if (Changes != string.Empty)
                {
                    _requestsRepo.Update(req);

                    RequestLog reqLog = new RequestLog();
                    reqLog.Date = System.DateTime.Now;
                    reqLog.Time = System.DateTime.Now.ToString("HH:mm");
                    reqLog.RequestId = req.Id;
                    reqLog.UserId = ApplicationSettings.UserId;
                    reqLog.Notes = Changes;
                    this._requestLogsRepo.Insert(reqLog);

                    //Set up and forwarding e-mails

                    //var link = Request.url.AbsoluteUri.Replace(Request.url.PathAndQuery, "/");

                    string url = "ServicesBooked/Edit/" + req.Id;
                    //url = link + url;

                    string subject = "Booking " + req.Agreement.Label + "#" + req.Id + "-" + part.TradeName + " was changed.";

                    Changes = "<br/><br>" + Changes1;


                    //general info about the request
                    string reqDetails = String.Empty;

                    string og = " " + req.EventDate.ToString("dd-MM-yyyy");
                    string date = og.Replace("12:00:00 AM", " ");

                    reqDetails = "<br/><br><p></p>Request general details:<br/><br> ";

                    if (req.ReturnDate == null)
                    {

                        reqDetails = reqDetails + "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>"
                            + "<tr><th>Field</th><th>Value</th></tr></thead>" +
                            "<tbody style='line-height: 1.5;font-size: 12px;'>" +
                            "<tr><td>Service</td><td>" + serv.Name + "</td></tr>"
                            + "<tr><td> Operator </td><td> " + part.TradeName + " </td></tr>"
                            + "<tr><td> Agent </td><td> " + agentUser.FullName + " </td></tr>"
                            + "<tr><td> Date </td><td> " + og + " </td></tr>" +
                            "<tr><td>Time</td><td>" + req.EventTime + "</td></tr>" +
                            "<tr><td>Client name</td><td>" + req.ContactName + "</td></tr>" +
                            "<tr><td>Client e-mail</td><td>" + req.ContactEmail + "</td></tr>" +
                            "<tr><td>Client phone</td><td>" + req.ContactPhone + "</td></tr>" +
                            "<tr><td>Nº of persons</td><td>" + req.NrPersons + "</td></tr>" +
                            "<tr><td>Price</td><td>" + req.Price + "</td></tr>";
                        if (req.PickupLocation != null)
                        {
                            reqDetails = reqDetails +
                            "<tr><td>Pick up location</td><td>" + req.PickupLocation + "</td></tr>";
                        }
                        if (req.DropoffLocation != null)
                        {
                            reqDetails = reqDetails +
                            "<tr><td>Dropoff location</td><td>" + req.DropoffLocation + "</td></tr>";
                        }
                        if (req.FlightNr != null)
                        {
                            reqDetails = reqDetails +
                            "<tr><td>Flight number</td><td>" + req.FlightNr + "</td></tr>";
                        }
                        reqDetails = reqDetails +
                            "<tr><td>Client notes</td><td>" + req.ClientNotes + "</td></tr>" +
                            "<tr><td>Notes</td><td>" + req.Notes + "</td></tr>" +
                            "<tr><td>Operator notes</td><td>" + req.OperatorNotes + "</td></tr></tbody></table>";
                    }
                    else
                    {
                        //og = " " + req.returnDate;
                        string returnDate = " " + req.ReturnDate?.ToString("dd-MM-yyyy");

                        reqDetails = reqDetails + "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>" +
                            "<tr><th>Field</th><th>Value</th></tr></thead>" +
                            "<tbody  style='line-height: 2;font-size: 12px;'>" +
                            "<tr><td>Service</td><td>" + serv.Name + "</td></tr>"
                            + "<tr><td> Operator </td><td> " + part.TradeName + " </td></tr>"
                             + "<tr><td> Agent </td><td> " + agentUser.Partner.TradeName + " </td></tr>"
                             + "<tr><td> Date </td><td> " + og + " </td></tr>" +
                             "<tr><td>Time</td><td>" + req.EventTime + "</td></tr>" +
                             "<tr><td>Client name</td><td>" + req.ContactName + "</td></tr>" +
                             "<tr><td>Client e-mail</td><td>" + req.ContactEmail + "</td></tr>" +
                             "<tr><td>Client phone</td><td>" + req.ContactPhone + "</td></tr>" +
                             "<tr><td>Nº of persons</td><td>" + req.NrPersons + "</td></tr>" +
                             "<tr><td>Price</td><td>" + req.Price + "</td></tr>" +
                             "<tr><td>Pick up location</td><td>" + req.PickupLocation + "</td></tr>" +
                             "<tr><td>Dropoff location</td><td>" + req.DropoffLocation + "</td></tr>" +
                             "<tr><td>Flight number</td><td>" + req.FlightNr + "</td></tr>" +
                             "<tr><td>Return Date</td><td>" + returnDate + "</td></tr>" +
                             "<tr><td>Return Time</td><td>" + req.ReturnTime + "</td></tr>" +
                             "<tr><td>Return flight number</td><td>" + req.ReturnFlight + "</td></tr>" +
                             "<tr><td>Return pickup</td><td>" + req.ReturnPickup + "</td></tr>" +
                             "<tr><td>Return dropoff</td><td>" + req.ReturnDropoff + "</td></tr>" +
                             "<tr><td>Client notes</td><td>" + req.ClientNotes + "</td></tr>" +
                             "<tr><td>Notes</td><td>" + req.Notes + "</td></tr>" +
                             "<tr><td>Operator notes</td><td>" + req.OperatorNotes + "</td></tr></tbody></table>";
                    }


                    string time = System.DateTime.Now.ToString("HH:mm");
                    string content = this._emailService.AgentContent(user.FullName, time) + Changes + reqDetails;

                    this._emailService.Email_to_send(agentUser.Partner.Email, url, content, subject);

                    url = "ServicesRequested/Details/" + req.Id;
                    //url = link + url;

                    subject = "Request " + req.Agreement.Label + "#" + req.Id + "-" + agentUser.Partner.TradeName + " was changed.";


                    content = this._emailService.OperatorEditorContent(part.TradeName, time) + Changes + reqDetails;
                    this._emailService.Email_to_send(part.Email, url, content, subject);

                //content = email.OperatorEditorContent() + Changes;
                //email.Email_to_send(user.email, url, content, subject);
               

                }

                return new BaseResponse()
                {
                Status = HttpStatusCode.OK,
                Message = "Request status Assign"
                };



        }

    }
}
