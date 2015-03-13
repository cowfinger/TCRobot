using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace TC.TCPage
{
    class TCPage
    {
        public string RawPage
        {
            get;
            private set;
        }

        public static T OpenUrl<T>(AccountInfo account, string url) where T : TCPage, new()
        {
            var webClient = new HttpClient(account.CookieStr);
            var rawPage = webClient.OpenUrl(url);
            var page = new T();
            account.CookieStr = webClient.Cookie.CookieString;
            return page;
        }

        public TCPage(string rawPage)
        {
            this.RawPage = rawPage;
        }
    }
}
