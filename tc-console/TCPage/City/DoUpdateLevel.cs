using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TC.TCTasks;

namespace TC.TCPage.City
{
    class DoUpdateLevel : TCPage
    {
        public bool Success
        {
            get { return this.RawPage.Contains("城池升级命令已下达"); }
        }

        public static DoUpdateLevel Open(RequestAgent agent, int nowLevel, int heroId, int cityId)
        {
            var url = agent.BuildUrl(
                TCMod.city,
                TCSubMod.city,
                TCOperation.Do,
                TCFunc.update_level,
                new TCRequestArgument(TCElement.level, nowLevel));
            var body = string.Format("hid={0}&city_id={1}", heroId, cityId);
            return new DoUpdateLevel(agent.WebClient.OpenUrl(url, body));
        }

        protected DoUpdateLevel(string page) : base(page)
        {
        }
    }
}
