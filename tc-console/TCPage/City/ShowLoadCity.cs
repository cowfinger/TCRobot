using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage.City
{
    class ShowLoadCity : TCPage
    {
        public static ShowLoadCity Open(RequestAgent agent, int cityId, int mt = 1)
        {
            var url = agent.BuildUrl(
                TCMod.city,
                TCSubMod.city,
                TCOperation.Show,
                TCFunc.load_city,
                new TCRequestArgument(TCElement.cid, cityId),
                new TCRequestArgument(TCElement.mt, mt));
            return new ShowLoadCity(agent.WebClient.OpenUrl(url));
        }

        protected ShowLoadCity(string page)
            : base(page)
        {
        }
    }
}
