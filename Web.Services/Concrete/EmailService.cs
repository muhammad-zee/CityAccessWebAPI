using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Web.Services.Extensions;
using Web.Services.Interfaces;

namespace Web.Services.Concrete
{
    public class EmailService : IEmailService
    {
        IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            this._config = config;
        }

        public string RegistrationSubject()
        {
            string subject = "Registration successful.";
            return subject;
        }

        public string EditorSubject()
        {
            string subject = "Booking change successful.";
            return subject;
        }
        public string OperatorEditorSubject()
        {
            string subject = "Request change successful.";
            return subject;
        }

        public string AgentSubject()
        {
            string subject = "One of your bookings was changed.";
            return subject;
        }

        public string OperatorSubejct()
        {
            string subject = "One of your requests was changed.";
            return subject;
        }

        public string RegistrationContent()
        {
            string content = "<br/><br> Your CityAccess account was successfuly created." +
                "Follow the link below to complete your registration. ";
            return content;
        }

        public string EditorContent()
        {
            string content = "<br/><br> You changed the following field(s) in a booking:";
            return content;
        }

        public string OperatorEditorContent(string agent, string time)
        {
            string content = "<br/><br> Request from " + agent + " was changed at " + time + " :";
            return content;
        }

        public string AgentContent(string user, string time)
        {
            string content = "<br/><br> The following changes were made by " + user + " at " + time + ":";
            return content;
        }

        public string OperatorContent(string agent, string time)
        {
            string content = "<br/><br>" + agent + " made the following changes at " + time + ":";
            return content;
        }
        public void Email_to_send(string email, string url, string content, string subject)
        {
            var sender = new MailAddress("CityAccess@cityaccess.pt", "CityAccess");
            var receiver = new MailAddress(email);
            //var senderPassword = "CA2018123!"; //por passe de dummy mail

            if (url != null)
            {
                content = content + "<br/><br><a href='" + url + "'>" + url + "</a>";

            }


            //String path = Server.MapPath("~/Content/h_logo_colors_G.png");

            // adicionar caminho relativo
            //Attachment att = new Attachment(Server.MapPath("~/Content/h_logo_colors_G.png"));
            //LinkedResource linkedImage = new LinkedResource(Server.MapPath("~/Content/h_logo_colors_G.png"), MediaTypeNames.Image.Jpeg)

            // commented on 8/10/2019
            //string image = "https://cityaccess.pt:8443/smb/file-manager/show?currentDir=%2FCityAccess%2FContent&file=h_logo_colors_G.png";
            //string image = "https://www.google.pt/search?biw=1920&bih=938&tbm=isch&sa=1&ei=us27W-6pM82Ua_elgeAM&q=lisboa+central+hostel&oq=lisboa+cen&gs_l=img.3.2.0l3j0i30k1l2j0i5i30k1l3j0i8i30k1l2.18981.20692.0.22802.10.10.0.0.0.0.90.813.10.10.0....0...1c.1.64.img..0.10.812...0i67k1.0.5bWO1vdQ1rg#imgrc=H3eyzvl33hOL3M:";
            //var webClient = new WebClient();



            //look deeper , as it might raise security issues for the app
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

            // commented on 8/10/2019
            //byte[] imageBytes = webClient.DownloadData(image);
            //MemoryStream ms = new MemoryStream(imageBytes);

            //could not use server.mappath because the httpContext is not available inside an child method
            LinkedResource linkedImage = new LinkedResource("wwwroot/logos/h_logo_colors_G.png")
            {
                ContentId = Guid.NewGuid().ToString(),

                // commented on 8/10/2019
                //linkedImage.ContentId = "CA_logo";
                ContentType = new System.Net.Mime.ContentType(MediaTypeNames.Image.Jpeg)
            };

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(content + "<br/><br>Greetings. <br><br><img width=200 height=70 src=cid:" + linkedImage.ContentId + " />", null, "text/html");

            // AlternateView imgView = AlternateView.CreateAlternateViewFromString("<img src="+linkedImage+">",null, MediaTypeNames.Text.Html);
            htmlView.LinkedResources.Add(linkedImage);

            var smtp = new SmtpClient
            {
                Host = this._config["MailSettings:Host"].ToString(),
                Port = this._config["MailSettings:Port"].ToInt(),
                EnableSsl = this._config["MailSettings:EnableSsl"].ToBool(),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(this._config["MailSettings:Username"].ToString(), this._config["MailSettings:Password"].ToString())
            };

            var message = new MailMessage(sender, receiver);

            message.Subject = subject;
            //message.Body = content;   
            message.AlternateViews.Add(htmlView);
            message.IsBodyHtml = true;

            smtp.Send(message);
        }
    }
}
