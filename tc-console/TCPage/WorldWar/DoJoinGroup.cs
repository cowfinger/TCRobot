using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage.WorldWar
{
    class DoJoinGroup : TCPage
    {
        public static DoJoinGroup Open(
            RequestAgent agent,
            int teamId,
            int groupId,
            int fromAddress = 1)
        {
            var url = agent.BuildUrl(
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Do,
                TCFunc.join_group);
            var body = string.Format("team_id={0}&group_id={1}&from_address={2}",
                teamId, groupId, fromAddress);
            return new DoJoinGroup(agent.WebClient.OpenUrl(url, body));
        }

        protected DoJoinGroup(string page) : base(page)
        {
        }
    }
}
