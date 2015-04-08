using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage.Train
{
    class DoDevelop : TCPage
    {
        public bool Success
        {
            get { return this.RawPage.Contains("研发成功"); }
        }

        public static DoDevelop Open(RequestAgent agent, int id)
        {
            var url = agent.BuildUrl(
                TCMod.military,
                TCSubMod.train,
                TCOperation.Do,
                TCFunc.develop);
            var body = string.Format("id={0}&sub_develop=true", id);
            return new DoDevelop(agent.WebClient.OpenUrl(url, body));
        }

        protected DoDevelop(string page) : base(page)
        {
        }
    }
}
