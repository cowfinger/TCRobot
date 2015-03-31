using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage.WorldWar
{
    class DoDisbandGroup : TCPage
    {
        public static DoDisbandGroup Open(RequestAgent agent, int groupId, int fromAddress = 1)
        {
            var url = agent.BuildUrl(
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Do,
                TCFunc.disband_group);
            var body = string.Format("group_id={0}&from_address={1}", groupId, fromAddress);
            return new DoDisbandGroup(agent.WebClient.OpenUrl(url, body));
        }

        protected DoDisbandGroup(string page) : base(page)
        {
        }
    }
}
