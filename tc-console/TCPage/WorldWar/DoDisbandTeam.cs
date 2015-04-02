using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage.WorldWar
{
    class DoDisbandTeam : TCPage
    {
        public static DoDisbandTeam Open(
            RequestAgent agent,
            int teamId,
            int fromAddress = 1,
            int detailFlag = 0)
        {
            var url = agent.BuildUrl(
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Do,
                TCFunc.disband_team);
            var body = string.Format("team_id={0}&from_address={1}&detail_flag={2}", teamId);
            return new DoDisbandTeam(agent.WebClient.OpenUrl(url, body));
        }

        protected DoDisbandTeam(string page) : base(page)
        {
        }
    }
}
