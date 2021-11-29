using System.Net;

namespace Web.Model
{
    public class BaseResponse
    {
        public HttpStatusCode Status { get; set; }
        public string Message { get; set; }
        public object Body { get; set; }
    }
}
