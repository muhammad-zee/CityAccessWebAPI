using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Data.Models;
using Web.Services.CommonVM;
using Web.Services.Interfaces;

namespace Web.Services.Concrete
{
    public class RequestsService : IRequestsService
    {

        //public ActionResult Index([Bind(Include = "ID, Notes, StateID ")] Req_forTransfer request, [Bind(Include = "Status, Partner, Req_PartnerList, Dwn,  Agent, ServTitle, Date, BookingDate, Date2, BookingDate2")] BookingFilters bookingFilters)
        //{
        //    Partner part;
        //    int partnerID;

        //    if (Session["admin"] == null)
        //    {
        //        // needed for the database fetch
        //        //partnerID = (int)Session["partnerID"];

        //        //  DB data needed for the e-mail
        //        part = db.Partners.Find(partnerID);
        //    }
        //    else
        //    {
        //        part = db.Partners.Where(a => a.tradeName == bookingFilters.Partner).FirstOrDefault();
        //        partnerID = part.ID;
        //    }

        //    IQueryable<Req_Partner> queryable = null;
        //    var requests = queryable;

        //    var requests1 =
        //          from requ in db.Requests
        //          join ag in db.Agreements on requ.agreementID equals ag.ID
        //          join us in db.Users on requ.bookerId equals us.ID
        //          join serv in db.Services on ag.serviceID equals serv.ID
        //          join serTyp in db.serviceTypes on serv.typeID equals serTyp.ID

        //          join ev in db.Events on requ.eventID equals ev.ID
        //          into evnt
        //          from ev in evnt.DefaultIfEmpty()

        //          join partn in db.Partners on ag.Partner.ID equals partn.ID
        //          into part1
        //          from partn in part1.DefaultIfEmpty()
        //          select (new Req_Partner
        //          {
        //              req_ForTransfer = new Req_forTransfer
        //              {
        //                  ID = requ.ID,
        //                  AgreementID = requ.agreementID,
        //                  BookerId = requ.bookerId,
        //                  ContactName = requ.contactName,
        //                  ContactEmail = requ.contactEmail,
        //                  ContactPhone = requ.contactPhone,
        //                  EventDate = requ.eventDate,
        //                  EventTime = requ.eventTime,
        //                  NrPersons = requ.nrPersons,
        //                  Price = requ.price,
        //                  Reference = requ.reference,
        //                  Notes = requ.notes,
        //                  PickupLocation = requ.pickupLocation,
        //                  DropoffLocation = requ.dropoffLocation,
        //                  FlightNr = requ.flightNr,
        //                  StateID = requ.stateID,
        //                  BookingDate = requ.bookDate,
        //                  OperatorNotes = requ.OperatorNotes,
        //                  eventID = requ.eventID,
        //                  EventNotes = ev.notes,
        //                  Leg = 1
        //              },
        //              //Partner = partn != null ? partn : null,
        //              Partner = partn,
        //              Agreement = ag,
        //              ServiceType = serTyp
        //          });

        //    //filtering part
        //    if (bookingFilters.Agent != null)
        //    {
        //        requests1 = requests1.Where(a => a.Partner.tradeName == bookingFilters.Agent);
        //    }
        //    if (bookingFilters.ServTitle != null)
        //    {
        //        requests1 = requests1.Where(a => a.Agreement.Service.name == bookingFilters.ServTitle);
        //    }
        //    if (bookingFilters.Date != null)
        //    {
        //        requests1 = requests1.Where(a => a.req_ForTransfer.EventDate >= bookingFilters.Date);
        //    }
        //    if (bookingFilters.Date != null && bookingFilters.Date2 != null)
        //    {
        //        requests1 = requests1.Where(a => a.req_ForTransfer.EventDate >= bookingFilters.Date && a.req_ForTransfer.EventDate <= bookingFilters.Date2);
        //    }
        //    if (bookingFilters.BookingDate != null)
        //    {
        //        requests1 = requests1.Where(a => a.req_ForTransfer.BookingDate >= bookingFilters.BookingDate);
        //    }
        //    if (bookingFilters.BookingDate != null && bookingFilters.BookingDate2 != null)
        //    {
        //        requests1 = requests1.Where(a => a.req_ForTransfer.BookingDate >= bookingFilters.BookingDate && a.req_ForTransfer.BookingDate <= bookingFilters.BookingDate2);
        //    }

        //    if (bookingFilters.Status == null)
        //    {
        //        requests1 = requests1.Where(b => b.req_ForTransfer.StateID == "Submitted" || b.req_ForTransfer.StateID == "Approved");
        //        requests1 = requests1.Where(a => a.req_ForTransfer.EventDate >= System.DateTime.Today);
        //    }
        //    else
        //    {
        //        if (bookingFilters.Status != "All")
        //        {
        //            requests1 = requests1.Where(b => b.req_ForTransfer.StateID == bookingFilters.Status);
        //        }
        //        else
        //        {
        //            requests1 = requests1.Where(b => b.req_ForTransfer.StateID != "Site Approval" && b.req_ForTransfer.StateID != "Site Canceled");
        //        }
        //    }
        //    requests1 = requests1.Where(a => a.Agreement.Service.operatorID == partnerID);
        //    requests1 = requests1.OrderBy(a => a.req_ForTransfer.EventDate + " " + a.req_ForTransfer.EventTime);

        //    var requests2 =
        //          from requ in db.Requests
        //          join ag in db.Agreements on requ.agreementID equals ag.ID
        //          join us in db.Users on requ.bookerId equals us.ID
        //          join serv in db.Services on ag.serviceID equals serv.ID
        //          join serTyp in db.serviceTypes on serv.typeID equals serTyp.ID

        //          join ev in db.Events on requ.eventID equals ev.ID
        //          into evnt
        //          from ev in evnt.DefaultIfEmpty()

        //          join partn in db.Partners on ag.Partner.ID equals partn.ID
        //          into part1
        //          from partn in part1.DefaultIfEmpty()
        //          select new Req_Partner
        //          {
        //              req_ForTransfer = new Req_forTransfer
        //              {
        //                  ID = requ.ID,
        //                  AgreementID = requ.agreementID,
        //                  BookerId = requ.bookerId,
        //                  ContactName = requ.contactName,
        //                  ContactEmail = requ.contactEmail,
        //                  ContactPhone = requ.contactPhone,
        //                  EventDate = requ.returnDate,
        //                  EventTime = requ.returnTime,
        //                  NrPersons = requ.nrPersons,
        //                  Price = new decimal(),
        //                  Reference = requ.reference,
        //                  Notes = requ.notes,
        //                  PickupLocation = requ.returnPickup,
        //                  DropoffLocation = requ.returnDropoff,
        //                  FlightNr = requ.returnFlight,
        //                  StateID = requ.stateID,
        //                  BookingDate = requ.bookDate,
        //                  OperatorNotes = requ.OperatorNotes,
        //                  eventID = requ.eventID,
        //                  EventNotes = ev.notes,
        //                  Leg = 2
        //              },
        //              //Partner = partn != null ? partn: null,
        //              Partner = partn,
        //              Agreement = ag,
        //              ServiceType = serTyp
        //          };

        //    //filtering part
        //    requests2 = requests2.Where(a => a.req_ForTransfer.EventDate != null);

        //    if (bookingFilters.Agent != null)
        //    {
        //        requests2 = requests2.Where(a => a.Partner.tradeName == bookingFilters.Agent);
        //    }
        //    if (bookingFilters.ServTitle != null)
        //    {
        //        requests2 = requests2.Where(a => a.Agreement.Service.name == bookingFilters.ServTitle);
        //    }
        //    if (bookingFilters.Date != null)
        //    {
        //        requests2 = requests2.Where(a => a.req_ForTransfer.EventDate >= bookingFilters.Date);
        //    }
        //    if (bookingFilters.Date != null && bookingFilters.Date2 != null)
        //    {
        //        requests2 = requests2.Where(a => a.req_ForTransfer.EventDate >= bookingFilters.Date && a.req_ForTransfer.EventDate <= bookingFilters.Date2);
        //    }
        //    if (bookingFilters.BookingDate != null)
        //    {
        //        requests2 = requests2.Where(a => a.req_ForTransfer.BookingDate >= bookingFilters.BookingDate);
        //    }
        //    if (bookingFilters.BookingDate != null && bookingFilters.BookingDate2 != null)
        //    {
        //        requests2 = requests2.Where(a => a.req_ForTransfer.BookingDate >= bookingFilters.BookingDate && a.req_ForTransfer.BookingDate <= bookingFilters.BookingDate2);
        //    }
        //    if (bookingFilters.Status == null)
        //    {
        //        requests2 = requests2.Where(b => b.req_ForTransfer.StateID == "Submitted" || b.req_ForTransfer.StateID == "Approved");
        //        requests2 = requests2.Where(a => a.req_ForTransfer.EventDate >= System.DateTime.Today);
        //    }
        //    else
        //    {
        //        if (bookingFilters.Status != "All")
        //        {
        //            requests2 = requests2.Where(b => b.req_ForTransfer.StateID == bookingFilters.Status);
        //        }
        //        else
        //        {
        //            requests2 = requests2.Where(b => b.req_ForTransfer.StateID != "Site Approval" && b.req_ForTransfer.StateID != "Site Canceled");
        //        }
        //    }
        //    requests2 = requests2.Where(a => a.Agreement.Service.operatorID == partnerID);
        //    requests2 = requests2.OrderBy(a => a.req_ForTransfer.EventDate + " " + a.req_ForTransfer.EventTime);

        //    requests = requests1.Union(requests2).OrderBy(x => x.req_ForTransfer.EventDate + " " + x.req_ForTransfer.EventTime);

        //    List<Req_Partner> reqs = requests.ToList();
        //    decimal TotalPrice = 0;
        //    int eventID = 0;
        //    int TotalPersons = 0;
        //    int limit = reqs.Count();

        //    foreach (var r in requests)
        //    {

        //        if (r.req_ForTransfer.eventID != null)
        //        {

        //            if (r.req_ForTransfer.eventID != eventID)
        //            {
        //                if (eventID != 0)
        //                {
        //                    for (int i = 0; i < limit; i++)
        //                    {
        //                        if (reqs.Skip(i).First().req_ForTransfer.eventID == eventID)
        //                        {
        //                            reqs.Skip(i).First().req_ForTransfer.TotalPrice = TotalPrice;
        //                            reqs.Skip(i).First().req_ForTransfer.TotalNrPersons = TotalPersons;
        //                        }
        //                    }
        //                }

        //                eventID = (int)r.req_ForTransfer.eventID;
        //                TotalPrice = r.req_ForTransfer.Price;
        //                TotalPersons = (int)r.req_ForTransfer.NrPersons;
        //            }
        //            else
        //            {
        //                TotalPrice = TotalPrice + r.req_ForTransfer.Price;
        //                TotalPersons = TotalPersons + (int)r.req_ForTransfer.NrPersons;
        //            }
        //        }
        //    }
        //    for (int i = 0; i < limit; i++)
        //    {
        //        if (reqs.Skip(i).First().req_ForTransfer.eventID == eventID)
        //        {
        //            reqs.Skip(i).First().req_ForTransfer.TotalPrice = TotalPrice;
        //            reqs.Skip(i).First().req_ForTransfer.TotalNrPersons = TotalPersons;
        //        }
        //    }


        //    ViewBag.agent = (from ag in db.Agreements
        //                     join serv in db.Services on ag.serviceID equals serv.ID
        //                     join partn in db.Partners on ag.partnerID equals partn.ID
        //                     where partnerID == serv.operatorID
        //                     select partn).Distinct();

        //    ViewBag.Service = (from serv in db.Services
        //                       where partnerID == serv.operatorID
        //                       select serv).Distinct();




        //    List<SelectListItem> items = new List<SelectListItem>();
        //    items.Add(new SelectListItem() { Text = "Submitted", Value = "Submitted" });
        //    items.Add(new SelectListItem() { Text = "Approved", Value = "Approved" });
        //    items.Add(new SelectListItem() { Text = "Canceled", Value = "Canceled" });
        //    items.Add(new SelectListItem() { Text = "Invoiced", Value = "Invoiced" });
        //    items.Add(new SelectListItem() { Text = "All", Value = "All" });

        //    bookingFilters.StatusFilters = items;
        //    bookingFilters.Req_PartnerList = requests.ToList();
        //    ViewBag.stateID = db.States.Where(x => x.ID != "Site Approval" && x.ID != "Site Canceled"); //Good for the time being, but not scalable, think of a better solution
        //    ViewBag.partners = db.Partners;


        //    //Excel part
        //    if (bookingFilters.Dwn == 1)
        //    {
        //        StringWriter sw = new StringWriter();
        //        var requestsToUs = new System.Data.DataTable("requests");
        //        requestsToUs.Columns.Add("Request", typeof(string));
        //        requestsToUs.Columns.Add("Service Name", typeof(string));
        //        requestsToUs.Columns.Add("Partner", typeof(string));
        //        requestsToUs.Columns.Add("Customer", typeof(string));
        //        requestsToUs.Columns.Add("Date", typeof(string));
        //        requestsToUs.Columns.Add("Time", typeof(TimeSpan));
        //        requestsToUs.Columns.Add("Pax", typeof(int));
        //        requestsToUs.Columns.Add("Status", typeof(string));
        //        requestsToUs.Columns.Add("Pick up Location", typeof(string));
        //        requestsToUs.Columns.Add("Flight number", typeof(string));
        //        requestsToUs.Columns.Add("Drop off Location", typeof(string));
        //        requestsToUs.Columns.Add("Price", typeof(string));
        //        requestsToUs.Columns.Add("Com.", typeof(string));
        //        requestsToUs.Columns.Add("Client Name", typeof(string));
        //        requestsToUs.Columns.Add("Notes", typeof(string));
        //        requestsToUs.Columns.Add("Operator Notes", typeof(string));


        //        string partnerTradename;

        //        foreach (var item in bookingFilters.Req_PartnerList)
        //        {
        //            if (item.Partner == null)
        //            {
        //                partnerTradename = "General Request";
        //            }
        //            else
        //            {
        //                partnerTradename = item.Partner.tradeName;
        //            }

        //            requestsToUs.Rows.Add(
        //                item.req_ForTransfer.ID,
        //                item.Agreement.label,
        //                partnerTradename,
        //                item.req_ForTransfer.ContactName,
        //                item.req_ForTransfer.EventDate.Value.ToString("dd/MM/yyyy"),
        //                item.req_ForTransfer.EventTime,
        //                item.req_ForTransfer.NrPersons,
        //                item.req_ForTransfer.StateID,
        //                item.req_ForTransfer.PickupLocation,
        //                item.req_ForTransfer.FlightNr,
        //                item.req_ForTransfer.DropoffLocation,
        //                item.req_ForTransfer.Price.ToString().Replace(".", ","),
        //                item.Agreement.commissionValue.ToString().Replace(".", ","),
        //                item.req_ForTransfer.ContactName,
        //                item.req_ForTransfer.Notes,
        //                item.req_ForTransfer.OperatorNotes
        //                );

        //            System.Diagnostics.Debug.WriteLine(item.req_ForTransfer.Price.ToString().Replace(".", ","));
        //        }

        //        var grid = new GridView();
        //        grid.DataSource = requestsToUs;
        //        grid.DataBind();

        //        Response.ClearContent();
        //        Response.Buffer = true;
        //        Response.ClearContent();
        //        Response.AddHeader("content-disposition", "attachement;filename=ServicesRequested.xls");
        //        Response.ContentType = "application/ms-excel";

        //        HtmlTextWriter htw = new HtmlTextWriter(sw);

        //        grid.RenderControl(htw);

        //        Response.Write(sw.ToString());
        //        Response.Flush();
        //        Response.End();

        //    }



        //    BookingFilters bookingFilters1 = new BookingFilters { Req_PartnerList = reqs, StatusFilters = items, Status = bookingFilters.Status };

        //    if (request.Notes != null)
        //    {
        //        ModelState.Clear(); // temporary solution for state changes that change dropdown value 
        //    }
        //    return View(bookingFilters1);

        //}

    }
}
