namespace TC
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

    using TC.TCUtility;

    public class HttpClient
    {
        private const string UserAgent =
            "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; Trident/5.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.2; .NET4.0C; .NET4.0E)";

        private const string ContentType = "application/x-www-form-urlencoded";

        public HttpClient(CookieContainer cookies = null)
        {
            this.Cookies = cookies ?? new CookieContainer();
        }

        public CookieContainer Cookies { get; set; }

        public string Location { get; private set; }

        public string Referer { get; set; }

        public string OpenUrl(string url, string body = null)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = UserAgent;
                request.Accept = "text/html, application/xhtml+xml, */*";
                // request.Headers.Add("Accept-Encoding", "gzip, deflate");
                request.Headers.Add("Accept-Language", "en-US,en;q=0.8,zh-Hans-CN;q=0.5,zh-Hans;q=0.3");
                request.Headers.Add("Cache-Control", "no-cache");

                request.CookieContainer = this.Cookies;

                if (!string.IsNullOrEmpty(this.Referer))
                {
                    request.Referer = this.Referer;
                    this.Referer = "";
                }

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
                    // this.Cookie.SetCookie(response.Headers["Set-Cookie"]);
                    this.Location = response.Headers["location"];

                    string content;
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        content = reader.ReadToEnd();
                    }
                    return content;
                }
            }
            catch (Exception e)
            {
                Logger.Verbose("HTTP Client Error:{0}", e.Message);
                return "";
            }
        }
    }
}