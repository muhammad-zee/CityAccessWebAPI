using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Web.Model;
using Web.Services.Interfaces;

namespace Web.Services.Helper
{
    public class HttpClientHelper: IHttpClient
    {
        HttpClient _httpClient;
        public HttpClientHelper() 
        {
            this._httpClient = new HttpClient();
        }

        public async Task<IDictionary<string ,object>> GetAsync(string apiUrl) 
        {
            var result = new Dictionary<string, object>();
            var googleApiResult = await this._httpClient.GetAsync(apiUrl).ConfigureAwait(false);
            if (googleApiResult.IsSuccessStatusCode)
            {
                await googleApiResult.Content.ReadAsStringAsync().ContinueWith((Task<string> x) =>
                {
                    result = JsonConvert.DeserializeObject<Dictionary<string, object>>(x.Result);
                });
            }
            else {
                var content = await googleApiResult.Content.ReadAsStringAsync();
                googleApiResult.Content?.Dispose();
                throw new HttpRequestException($"{googleApiResult.StatusCode}:{content}");
            }

            return result;
        }
    }
}
