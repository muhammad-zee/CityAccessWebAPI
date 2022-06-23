using System;
using System.IO;
using System.Net;

namespace TwilioAutomationService
{
    public static class AutomationService
    {
        public static void TriggerQueues()
        {
            string api = Configuration.Base_Url + "Queue/saveQueue";
            HttpWebRequest authRequest = (HttpWebRequest)WebRequest.Create(api);
            authRequest.ContentType = "application/json";
            authRequest.Method = "POST";
            //Set content length to 0
            authRequest.ContentLength = 0;
            try
            {
                WebResponse webResponse = authRequest.GetResponse();
                using (Stream dataStreamResponse = webResponse.GetResponseStream())
                {
                    using (StreamReader tReader = new StreamReader(dataStreamResponse))
                    {
                        // String response = await tReader.ReadToEndAsync();
                        //var Token = JsonConvert.DeserializeObject<string>(response);

                    }
                }
            }
            catch (Exception e)
            {

            }
        }
    }
}
