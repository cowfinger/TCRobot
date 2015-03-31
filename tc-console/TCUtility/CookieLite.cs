namespace TC
{
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;

    public class CookieLite
    {
        public Dictionary<string, string> CookieMap { get; private set; }

        public CookieLite(string cookie = "")
        {
            this.CookieMap = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(cookie))
            {
                this.SetCookie(cookie);
            }
        }

        public string CookieString
        {
            get
            {
                lock (this.CookieMap)
                {
                    return ComposeCookieString(this.CookieMap);
                }
            }
        }

        public static Dictionary<string, string> ParseCookieString(string cookieString)
        {
            var result = new Dictionary<string, string>();
            if (cookieString == null)
            {
                return result;
            }

            var strs = cookieString.Split(',');
            foreach (var i in strs)
            {
                var key = "";
                var val = "";

                var pairs = i.Split(new[] { '=', ';' });
                if (pairs.Length < 2)
                {
                    continue;
                }

                key = pairs[0].Trim(' ');
                val = pairs[1].Trim(' ');

                if (string.IsNullOrEmpty(key) || key == "expires" || key == "path" || key.Contains(" "))
                {
                    continue;
                }

                if (result.ContainsKey(key))
                {
                    result[key] = val;
                }
                else
                {
                    result.Add(key, val);
                }
            }

            return result;
        }

        public static string ComposeCookieString(Dictionary<string, string> cookie)
        {
            var cookieBuilder = new StringBuilder();
            foreach (var i in cookie)
            {
                if (!string.IsNullOrEmpty(i.Value))
                {
                    cookieBuilder.AppendFormat("{0}={1};", i.Key, i.Value);
                }
            }

            return cookieBuilder.ToString();
        }

        public void SetCookie(CookieCollection cookies)
        {
            foreach (Cookie cookie in cookies)
            {
                lock (this.CookieMap)
                {
                    this.SetCookie(cookie.Name, cookie.Value);
                }
            }
        }

        public void SetCookie(string cookieString)
        {
            var setCookies = ParseCookieString(cookieString);

            foreach (var cookie in setCookies)
            {
                lock (this.CookieMap)
                {
                    this.SetCookie(cookie.Key, cookie.Value);
                }
            }
        }

        private void SetCookie(string key, string value)
        {
            if (this.CookieMap.ContainsKey(key))
            {
                if (value.StartsWith("delete"))
                {
                    this.CookieMap.Remove(key);
                }
                else
                {
                    this.CookieMap[key] = value;
                }
            }
            else
            {
                if (!value.StartsWith("delete"))
                {
                    this.CookieMap.Add(key, value);
                }
            }
        }

        public bool Load(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return false;
            }

            using (var streamReader = new StreamReader(fileName))
            {
                this.SetCookie(streamReader.ReadToEnd());
            }

            return true;
        }

        public void Save(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            using (var streamWriter = new StreamWriter(fileName))
            {
                streamWriter.Write(this.CookieString);
                streamWriter.Flush();
            }
        }
    }
}