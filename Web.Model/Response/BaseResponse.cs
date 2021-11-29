using System.Net;

namespace Web.Model
{
    public class BaseResponse
    {
        public HttpStatusCode Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
