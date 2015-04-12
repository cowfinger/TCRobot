using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage.Science
{
    class DoUp : TCPage
    {
        public bool Success
        {
            get { return this.RawPage.Contains("研发成功！"); }
        }

        public static DoUp Open(RequestAgent agent, int scienceId, int heroId)
        {
            var url = agent.BuildUrl(
                TCMod.science,
                TCSubMod.science,
                TCOperation.Do,
                TCFunc.up);
            var body = string.Format("science_id={0}&hero_id={1}", scienceId, heroId);
            var page = agent.WebClient.OpenUrl(url, body);
            return new DoUp(page);
        }

        protected DoUp(string page) : base(page)
        {
        }
    }
}
