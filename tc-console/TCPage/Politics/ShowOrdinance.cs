using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage.Politics
{
    class ShowOrdinance : TCPage
    {
        public bool CanReOrg
        {
            get
            {
                return !this.RawPage.Contains(@"<span id=""civil_reorganize_span"" titleContent=""未过冻结时间"">");
            }
        }

        public static ShowOrdinance Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(
                TCMod.civil,
                TCSubMod.politics,
                TCOperation.Show,
                TCFunc.ordinance);
            return new ShowOrdinance(agent.WebClient.OpenUrl(url));
        }

        protected ShowOrdinance(string page) : base(page)
        {
        }
    }
}
