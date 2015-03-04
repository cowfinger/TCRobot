using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace TC
{
    class HttpClient
    {
        private const string UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; Trident/5.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.2; .NET4.0C; .NET4.0E)";
        private const string ContentType = "application/x-www-form-urlencoded";

        public CookieLite Cookie
        {
            get;
            private set;
        }

        public HttpClient(string cookie)
        {
            this.Cookie = new CookieLite(cookie);
        }

        public string OpenUrl(string url, string body = null)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = UserAgent;
                request.Headers.Add("Cookie", this.Cookie.CookieString);

                if (!string.IsNullOrEmpty(body))
                {
                    var codedBytes = Encoding.ASCII.GetBytes(body);
                    request.Method = "POST";
                    request.ContentType = ContentType;
                    request.ContentLength = codedBytes.Length;
                    request.GetRequestStream().Write(codedBytes, 0, codedBytes.Length);
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    this.Cookie.SetCookie(response.Headers["Set-Cookie"]);

                    string content = string.Empty;
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        content = reader.ReadToEnd();
                    }
                    return content;
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
