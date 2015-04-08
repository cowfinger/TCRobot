using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage.Politics
{
    class DoReorganize : TCPage
    {
        public bool Success
        {
            get { return this.RawPage.Contains("整治成功！"); }
        }

        public static DoReorganize Open(RequestAgent agent, int heroId)
        {
            var url = agent.BuildUrl(
                TCMod.civil,
                TCSubMod.politics,
                TCOperation.Do,
                TCFunc.reorganize);
            var body = string.Format("hero_id={0}", heroId);
            return new DoReorganize(agent.WebClient.OpenUrl(url, body));
        }

        protected DoReorganize(string page) : base(page)
        {
        }
    }
}
