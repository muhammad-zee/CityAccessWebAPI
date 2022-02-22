using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web.Services.Interfaces
{
    public interface IHttpClient
    {
        Task<IDictionary<string, object>> GetAsync(string apiUrl);
    }
}
