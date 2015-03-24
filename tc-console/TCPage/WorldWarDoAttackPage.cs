using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage
{
    class WorldWarDoAttackPage
    {
        public string RawPage { get; private set; }

        public static WorldWarDoAttackPage Open(RequestAgent agent, int teamId, int toCityId)
        {
            var url = agent.BuildUrl(
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Do,
                TCFunc.attack);
            var body = string.Format("team_id={0}&to_city_id={1}", teamId, toCityId);
            var page = agent.WebClient.OpenUrl(url, body);
            return new WorldWarDoAttackPage(page);
        }

        public WorldWarDoAttackPage(string page)
        {
            this.RawPage = page;
        }
    }
}
