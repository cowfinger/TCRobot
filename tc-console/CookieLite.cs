using System;
using System.Collections.Generic;
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
            string output = "";
            foreach (var i in cookie)
            {
                if (string.Compare(i.Key, "path", true) == 0)
                {
                    continue;
                }

                output += i;
                if (string.IsNullOrEmpty(i.Value))
                {
                    output += "=";
                    output += i.Value;
                }

                output += ";";
            }

            return output;
        }

        public string CookieString
        {
            get
            {
                return ComposeCookieString(this.cookieMap);
            }

            set
            {
                var setCookies = ParseCookieString(value);

                foreach (var cookie in setCookies)
                {
                    if (this.cookieMap[cookie.Key].Contains(cookie.Key))
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

        public CookieLite()
        {
        }
    }
}
