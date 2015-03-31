using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage
{
    class DoGetData : TCPage
    {
        public const string RequestString = "mod=get_data&op=do";

        public static DoGetData Open(RequestAgent agent, int re, int task, int taskHint)
        {
            var url = agent.BuildUrl(RequestString);
            var body = string.Format(
                "module=%7B%22re%22%3A%5B{0}%5D%2C%22task%22%3A%5B{1}%2C{2}%5D%7D",
                re, task, taskHint);
            return new DoGetData(agent.WebClient.OpenUrl(url, body));
        }

        protected DoGetData(string page) : base(page)
        {
        }
    }
}
