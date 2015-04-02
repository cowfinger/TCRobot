using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage.WorldWar
{
    class DoCreateGroup : TCPage
    {
        public static DoCreateGroup Open(RequestAgent agent, int teamId, string groupName)
        {
            var url = agent.BuildUrl(
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Do,
                TCFunc.create_group);

            var body = string.Format("team_id={0}&group_name={1}", teamId, groupName);
            return new DoCreateGroup(agent.WebClient.OpenUrl(url, body));
        }

        protected DoCreateGroup(string page)
            : base(page)
        {
        }
    }
}
