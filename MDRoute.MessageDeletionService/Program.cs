using System;
using System.IO;
using System.Net;

namespace MDRoute.MessageDeletionService
{
    class Program
    {
        public static string Base_Url = "https://5353-202-166-174-174.ngrok.io/";
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
        public static void TriggerApi()
        {

            string api = Base_Url + "Conversation/DeleteConversationMessagesAsPerHippaComplaint";
            HttpWebRequest authRequest = (HttpWebRequest)WebRequest.Create(api);
            authRequest.ContentType = "application/json";
            authRequest.Method = "POST";
            //Set content length to 0
            authRequest.ContentLength = 0;

            string data = "phone_number=19735559042";
            byte[] dataStream = Encoding.UTF8.GetBytes(data);
            authRequest.ContentLength = dataStream.Length;
            Stream newStream = authRequest.GetRequestStream();
            newStream.Write(dataStream, 0, dataStream.Length);
            newStream.Close();


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
