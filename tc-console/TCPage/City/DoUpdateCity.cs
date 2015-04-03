using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TC.TCTasks;

namespace TC.TCPage.City
{
    class DoUpdateCity : TCPage
    {
        public static DoUpdateCity Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(
                TCMod.city,
                TCSubMod.city,
                TCOperation.Do,
                TCFunc.update_city);
            return new DoUpdateCity(agent.WebClient.OpenUrl(url));
        }

        protected DoUpdateCity(string page) : base(page)
        {
        }
    }
}
