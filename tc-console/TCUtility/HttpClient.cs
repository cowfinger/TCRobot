using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TC
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Web;

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

        public static IEnumerable<Cookie> GetAllCookies(CookieContainer cc)
        {
            var table = (Hashtable)cc.GetType().InvokeMember(
                "m_domainTable",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.GetField |
                System.Reflection.BindingFlags.Instance,
                null, cc, new object[] { });

            foreach (var pathList in table.Values)
            {
                var lstCookieCol = (SortedList)pathList.GetType().InvokeMember(
                    "m_list",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.GetField |
                    System.Reflection.BindingFlags.Instance,
                    null, pathList, new object[] { });
                foreach (var c in from CookieCollection colCookies in lstCookieCol.Values
                                  from Cookie c in colCookies
                                  select c)
                {
                    yield return c;
                }
            }
        }

        public static void SaveCookies(CookieContainer cookieContainer, string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }

            var cookies = GetAllCookies(cookieContainer);
            using (var stream = new StreamWriter(file))
            {
                foreach (var cookie in cookies)
                {
                    var line = string.Format("{0};{1};{2};{3};{4};{5}",
                        cookie.Domain, cookie.Name, cookie.Path, cookie.Port,
                        cookie.Secure.ToString(), cookie.Value);
                    stream.WriteLine(line);
                }
            }
        }

        public static CookieContainer LoadCookies(string file)
        {
            var cookieContainer = new CookieContainer();
            using (var stream = new StreamReader(file))
            {
                while (!stream.EndOfStream)
                {
                    var line = stream.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    var cc = line.Split(';');
                    var ck = new Cookie
                    {
                        Discard = false,
                        Domain = cc[0],
                        Expired = true,
                        HttpOnly = true,
                        Name = cc[1],
                        Path = cc[2],
                        Port = cc[3],
                        Secure = bool.Parse(cc[4]),
                        Value = cc[5]
                    };
                    cookieContainer.Add(ck);
                }
            }

            return cookieContainer;
        }

        public string OpenUrl(string url, string body = null)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = UserAgent;
                request.Accept = "text/html, application/xhtml+xml, */*";
                request.Headers.Add("Accept-Language", "en-US,en;q=0.8,zh-Hans-CN;q=0.5,zh-Hans;q=0.3");
                request.Headers.Add("Cache-Control", "no-cache");

                request.CookieContainer = this.Cookies;

                if (!string.IsNullOrEmpty(this.Referer))
                {
                    request.Referer = this.Referer;
                    this.Referer = "";
                }

                if (body != null)
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