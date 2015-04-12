using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage.Science
{
    class DoRecruit : TCPage
    {
        public bool Success
        {
            get { return this.RawPage.Contains("成功招募中"); }
        }

        public static DoRecruit Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(
                TCMod.science,
                TCSubMod.science,
                TCOperation.Do,
                TCFunc.recruit);
            return new DoRecruit(agent.WebClient.OpenUrl(url, ""));
        }

        protected DoRecruit(string page) : base(page)
        {
        }
    }
}
