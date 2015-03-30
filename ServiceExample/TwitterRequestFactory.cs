using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TwitterWCFService
{   
    /// <summary>
    /// This class creates valid twitter public stream request with proper Authorization header.
    /// </summary>
    public class TwitterRequestFactory
    {
        private static readonly string requestMethodValue = "POST";
        private static readonly string acceptType = "application/json";
        private static readonly string contentType = "application/x-www-form-urlencoded";
        private static readonly string requestParams = "delimited=length&track=";
        private static readonly string url = "https://stream.twitter.com/1.1/statuses/filter.json";

        public TwitterRequestFactory() 
        {
        }

        /// <summary>
        /// Method creates HttpWebRequest object that would facilitate Twitter requirments. Func delegate is used to facilitate Unit Tests.
        /// </summary>
        /// <param name="urlEnding"></param>
        /// <param name="trackValue"></param>
        /// <param name="authFuncDelegate"></param>
        /// <returns></returns>
        
        public static HttpWebRequest InitRequest(string trackValue, Func<string, string, string, string> authFuncDelegate = null)
        {
            string authValue = GetAuthValue(url, trackValue, authFuncDelegate);
                //Authorization.InitAuthValue(requestMethodValue, url, trackValue);
           
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = requestMethodValue;
            request.Headers.Add("Authorization", authValue);
            request.Accept = acceptType;
            request.ContentType = contentType;

            request = AddParameters(request, trackValue);           
            return request;
        }

        /// <summary>
        /// Main justification for these method is to faciliate Unit Tests and allow denpendency injection in place for Authorization.InitAuthValue method.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="trackValue"></param>
        /// <param name="funcDelegate"></param>
        /// <returns></returns>
        public static string GetAuthValue(string url, string trackValue, Func<string, string, string, string> funcDelegate) 
        {
            Func<string, string, string, string> authFunc = funcDelegate;

            if (authFunc == null)
                authFunc = Authorization.InitAuthValue;

            string authValue = authFunc(requestMethodValue, url, trackValue);
            return authValue;
        }

        /// <summary>
        /// The method adds parameters to request in order to facilitate proper data stream format from Twitter Service and proper data filtering (trackValue param).
        /// Parameter "delimited" with value "length" (present in requestParams variable) determines the format of the data send back by Twitter in response stream. 
        /// It means that each tweet will be send seperately preceded by its size expressed in number of bytes.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="trackValue"></param>
        /// <returns></returns>
        public static HttpWebRequest AddParameters(HttpWebRequest request, string trackValue)
        {
            byte[] data = ToBytes(requestParams + trackValue);            
            request.ContentLength = data.Length;

            try
            {
                using (Stream post = request.GetRequestStream())
                {
                    post.Write(data, 0, data.Length);
                    post.Flush();
                    post.Close();
                }
            }
            catch
            {   
                throw;
            }
            return request;
        }

        public static byte[] ToBytes(string paramString)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(paramString);
            return data;
        }


    }
}
