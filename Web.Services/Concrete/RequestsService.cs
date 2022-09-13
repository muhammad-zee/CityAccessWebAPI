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
using Web.Services.CommonVM;
using Web.Services.Enums;
using Web.Services.Interfaces;

namespace Web.Services.Concrete
{
    public class RequestsService : IRequestsService
    {


        private readonly IGenericRepository<Partner> _partnersRepo;
        private readonly IGenericRepository<Agreement> _agreementsRepo;
        private readonly IGenericRepository<Request> _requestsRepo;
        private readonly IGenericRepository<RequestLog> _requestLogsRepo;
        private readonly IGenericRepository<User> _usersRepo;
        private readonly IGenericRepository<Service> _servicesRepo;
        private readonly IGenericRepository<ServiceType> _servicetypesRepo;
        private readonly IGenericRepository<Event> _eventsRepo;
        private readonly IEmailService _emailService;

        public RequestsService(IGenericRepository<Partner> partnersRepo,
            IGenericRepository<Agreement> agreementsRepo,
            IGenericRepository<Request> requestsRepo, 
            IGenericRepository<RequestLog> requestLogsRepo, 
            IGenericRepository<User> usersRepo,
            IGenericRepository<Service> servicesRepo,IGenericRepository<ServiceType> servicetypesRepo,
            IGenericRepository<Event> eventsRepo,
            IEmailService emailService)
            
        {
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

        public BaseResponse GetBookedServices(RequestsFilterVM filter)
        {

       
            int partnerID = ApplicationSettings.PartnerId;
            //Partner partner = this._partnersRepo.Table.Where(p => p.Id == partnerID && p.IsActive != false).FirstOrDefault();
            

            IQueryable<Req_Partner> queryable = null;
            var requests = queryable;
         

            var requests1 =
                  from requ in this._requestsRepo.Table
                  join ag in this._agreementsRepo.Table on requ.AgreementId equals ag.Id
                  join us in this._usersRepo.Table on requ.BookerId equals us.Id
                  join serv in this._servicesRepo.Table on ag.ServiceId equals serv.Id
                  join serTyp in this._servicetypesRepo.Table on serv.TypeId equals serTyp.Id
                  join ev in this._eventsRepo.Table on requ.EventId equals ev.Id
                  into evnt
                  from ev in evnt.DefaultIfEmpty()
                  join partn in this._partnersRepo.Table on ag.Partner.Id equals partn.Id
                  into part1
                  from partn in part1.DefaultIfEmpty()
                  select (new Req_Partner
                  {
                      req_ForTransfer = new Req_forTransfer
                      {
                          ID = requ.Id,
                          AgreementID = requ.AgreementId,
                          BookerId = requ.BookerId,
                          ContactName = requ.ContactName,
                          ContactEmail = requ.ContactEmail,
                          ContactPhone = requ.ContactPhone,
                          EventDate = requ.EventDate,
                          EventTime = requ.EventTime,
                          NrPersons = requ.NrPersons,
                          Price = requ.Price,
                          Reference = requ.Reference,
                          Notes = requ.Notes,
                          PickupLocation = requ.PickupLocation,
                          DropoffLocation = requ.DropoffLocation,
                          FlightNr = requ.FlightNr,
                          StateID = requ.StateId,
                          BookingDate = requ.BookDate,
                          OperatorNotes = requ.OperatorNotes,
                          eventID = requ.EventId,
                          EventNotes = ev.Notes,
                          Leg = 1
                      },
                      //Partner = partn != null ? partn : null,
                      Partner = partn,
                      Agreement = ag,
                      ServiceType = serTyp
                  });


            //filtering part
           // if (bookingFilters.Agent != null)
           // {
            //    requests1 = requests1.Where(a => a.Partner.tradeName == bookingFilters.Agent);
           // }
           // if (bookingFilters.ServTitle != null)
          //  {
          //      requests1 = requests1.Where(a => a.Agreement.Service.name == bookingFilters.ServTitle);
           // }
           // if (bookingFilters.Date != null)
           // {
            //    requests1 = requests1.Where(a => a.req_ForTransfer.EventDate >= bookingFilters.Date);
           // }
           // if (bookingFilters.Date != null && bookingFilters.Date2 != null)
           // {
           //     requests1 = requests1.Where(a => a.req_ForTransfer.EventDate >= bookingFilters.Date && a.req_ForTransfer.EventDate <= bookingFilters.Date2);
           // }
           // if (bookingFilters.BookingDate != null)
           // {
           //     requests1 = requests1.Where(a => a.req_ForTransfer.BookingDate >= bookingFilters.BookingDate);
           // }
           // if (bookingFilters.BookingDate != null && bookingFilters.BookingDate2 != null)
           // {
           //     requests1 = requests1.Where(a => a.req_ForTransfer.BookingDate >= bookingFilters.BookingDate && a.req_ForTransfer.BookingDate <= bookingFilters.BookingDate2);
           // }

           // if (bookingFilters.Status == null)
           // {
               // requests1 = requests1.Where(b => b.req_ForTransfer.StateID == "Submitted" || b.req_ForTransfer.StateID == "Approved");
              //  requests1 = requests1.Where(a => a.req_ForTransfer.EventDate >= System.DateTime.Today);
           // }
           // else
           // {
             //   if (bookingFilters.Status != "All")
             //   {
              //      requests1 = requests1.Where(b => b.req_ForTransfer.StateID == bookingFilters.Status);
              //  }
              //  else
               // {
               //     requests1 = requests1.Where(b => b.req_ForTransfer.StateID != "Site Approval" && b.req_ForTransfer.StateID != "Site Canceled");
              //  }
           // }
           requests1 = requests1.Where(a => a.Agreement.Service.OperatorId == partnerID);
            requests1 = requests1.OrderBy(a => a.req_ForTransfer.EventDate + " " + a.req_ForTransfer.EventTime);

            var requests2 =
                  from requ in this._requestsRepo.Table
                  join ag in this._agreementsRepo.Table on requ.AgreementId equals ag.Id
                  join us in this._usersRepo.Table on requ.BookerId equals us.Id
                  join serv in this._servicesRepo.Table on ag.ServiceId equals serv.Id
                  join serTyp in this._servicetypesRepo.Table on serv.TypeId equals serTyp.Id

                  join ev in this._eventsRepo.Table on requ.EventId equals ev.Id
                  into evnt
                  from ev in evnt.DefaultIfEmpty()

                  join partn in this._partnersRepo.Table on ag.Partner.Id equals partn.Id
                  into part1
                  from partn in part1.DefaultIfEmpty()
                  select new Req_Partner
                  {
                      req_ForTransfer = new Req_forTransfer
                      {
                          ID = requ.Id,
                          AgreementID = requ.AgreementId,
                          BookerId = requ.BookerId,
                          ContactName = requ.ContactName,
                          ContactEmail = requ.ContactEmail,
                          ContactPhone = requ.ContactPhone,
                          EventDate = requ.ReturnDate,
                          EventTime = requ.ReturnTime,
                          NrPersons = requ.NrPersons,
                          Price = new decimal(),
                          Reference = requ.Reference,
                          Notes = requ.Notes,
                          PickupLocation = requ.ReturnPickup,
                          DropoffLocation = requ.ReturnDropoff,
                          FlightNr = requ.ReturnFlight,
                          StateID = requ.StateId,
                          BookingDate = requ.BookDate,
                          OperatorNotes = requ.OperatorNotes,
                          eventID = requ.EventId,
                          EventNotes = ev.Notes,
                          Leg = 2
                      },
                      //Partner = partn != null ? partn: null,
                      Partner = partn,
                      Agreement = ag,
                      ServiceType = serTyp
                  };

            //filtering part
           // requests2 = requests2.Where(a => a.req_ForTransfer.EventDate != null);

           // if (bookingFilters.Agent != null)
           // {
           //     requests2 = requests2.Where(a => a.Partner.tradeName == bookingFilters.Agent);
           // }
           // if (bookingFilters.ServTitle != null)
           // {
           //     requests2 = requests2.Where(a => a.Agreement.Service.name == bookingFilters.ServTitle);
           // }
           // if (bookingFilters.Date != null)
           // {
           //     requests2 = requests2.Where(a => a.req_ForTransfer.EventDate >= bookingFilters.Date);
           // }
           // if (bookingFilters.Date != null && bookingFilters.Date2 != null)
           // {
           //     requests2 = requests2.Where(a => a.req_ForTransfer.EventDate >= bookingFilters.Date && a.req_ForTransfer.EventDate <= bookingFilters.Date2);
          //  }
          //  if (bookingFilters.BookingDate != null)
           // {
           //     requests2 = requests2.Where(a => a.req_ForTransfer.BookingDate >= bookingFilters.BookingDate);
           // }
           // if (bookingFilters.BookingDate != null && bookingFilters.BookingDate2 != null)
           // {
          //      requests2 = requests2.Where(a => a.req_ForTransfer.BookingDate >= bookingFilters.BookingDate && a.req_ForTransfer.BookingDate <= bookingFilters.BookingDate2);
          //  }
          //  if (bookingFilters.Status == null)
          //  {
           //     requests2 = requests2.Where(b => b.req_ForTransfer.StateID == "Submitted" || b.req_ForTransfer.StateID == "Approved");
           //     requests2 = requests2.Where(a => a.req_ForTransfer.EventDate >= System.DateTime.Today);
           // }
          //  else
          //  {
           //     if (bookingFilters.Status != "All")
           //     {
           //         requests2 = requests2.Where(b => b.req_ForTransfer.StateID == bookingFilters.Status);
           //     }
           //     else
           //     {
           //         requests2 = requests2.Where(b => b.req_ForTransfer.StateID != "Site Approval" && b.req_ForTransfer.StateID != "Site Canceled");
           //     }
           // }
            requests2 = requests2.Where(a => a.Agreement.Service.OperatorId == partnerID);
            requests2 = requests2.OrderBy(a => a.req_ForTransfer.EventDate + " " + a.req_ForTransfer.EventTime);

            requests = requests1.AsEnumerable().Union(requests2.AsEnumerable()).AsQueryable();//.OrderBy(x => x.req_ForTransfer.EventDate + " " + x.req_ForTransfer.EventTime);
            //requests = requests1.Union(requests2).OrderBy(x => x.req_ForTransfer.EventDate + " " + x.req_ForTransfer.EventTime);

            //List<Req_Partner> reqs = requests.ToList();
            //decimal TotalPrice = 0;
            //int eventID = 0;
            //int TotalPersons = 0;
            //int limit = reqs.Count();

            //for (int i = 0; i < limit; i++)
            //{
            //    if (reqs.Skip(i).First().req_ForTransfer.eventID == eventID)
            //    {
            //        reqs.Skip(i).First().req_ForTransfer.TotalPrice = TotalPrice;
            //        reqs.Skip(i).First().req_ForTransfer.TotalNrPersons = TotalPersons;
            //    }
            //}


            // ViewBag.agent = (from ag in this._agreementsRepo.Table
            //     join serv in this._servicetypesRepo.Table on ag.ServiceId equals serv.Id
            //     join partn in this._partnersRepo.Table on ag.PartnerId equals partn.Id
            //                 where partnerID == serv.operatorID
            //                 select partn).Distinct();

            //ViewBag.Service = (from serv in this._servicesRepo.Table
            //                   where partnerID == serv.operatorID
            //                   select serv).Distinct();




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

            // if (request.Notes != null)
            // {
            //     ModelState.Clear(); 
            // temporary solution for state changes that change dropdown value 
            // }
            //  return View(bookingFilters1);
            return new BaseResponse { Status = HttpStatusCode.OK, Message = "dataretrund", Body = requests };

        }

        public void exportServicesToExcelSheet()
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
                ReturnDate = requestParam.returnDate,
                ReturnTime = requestParam.returnTime,
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
        public void SendServiceBookedEmail(Request request, Partner part, Agreement ag, Service serv, Partner opr, bool? FromPartnerSite)
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

    }
}
