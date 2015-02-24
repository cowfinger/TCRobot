using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TC
{
    class CookieLite
    {
        private Dictionary<string, string> cookieMap = new Dictionary<string, string>();

        static public Dictionary<string, string> ParseCookieString(string cookieString)
        {
            var result = new Dictionary<string, string>();
            if (cookieString == null)
                return result;

            string[] strs = cookieString.Split(';');
            foreach (string i in strs)
            {
                string key = "";
                string val = "";

                int seppos = i.IndexOf('=');
                if (seppos == -1)
                {
                    key = i.Trim(' ');
                }
                else
                {
                    key = i.Substring(0, seppos).Trim(' ');
                    val = i.Substring(seppos + 1);
                }

                if (string.IsNullOrEmpty(key))
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

        static public string ComposeCookieString(Dictionary<string, string> cookie)
        {
            var cookieBuilder = new StringBuilder();
            foreach (var i in cookie)
            {
                if (string.Compare(i.Key, "path", true) == 0)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(i.Value))
                {
                    cookieBuilder.AppendFormat("{0}={1};", i.Key, i.Value);
                }
            }

            return cookieBuilder.ToString();
        }

        public string CookieString
        {
            get
            {
                lock (this.cookieMap)
                {
                    return ComposeCookieString(this.cookieMap);
                }
            }
        }

        public void SetCookie(string cookieString)
        {
            var setCookies = ParseCookieString(cookieString);

            foreach (var cookie in setCookies)
            {
                lock (this.cookieMap)
                {
                    if (this.cookieMap.ContainsKey(cookie.Key))
                    {
                        this.cookieMap[cookie.Key] = cookie.Value;
                    }
                    else
                    {
                        this.cookieMap.Add(cookie.Key, cookie.Value);
                    }
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
                SetCookie(streamReader.ReadToEnd());
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

        public CookieLite(string cookie = "")
        {
            if (!string.IsNullOrEmpty(cookie))
            {
                SetCookie(cookie);
            }
        }
    }
}
