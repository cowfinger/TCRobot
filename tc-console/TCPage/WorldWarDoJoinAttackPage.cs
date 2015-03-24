using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage
{
    class WorldWarDoJoinAttackPage
    {
        public string RawPage { get; private set; }

        public static WorldWarDoJoinAttackPage Open(RequestAgent agent, int groupId, int toCityId, int attackType = 1)
        {
            var url = agent.BuildUrl(
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Do,
                TCFunc.join_attack);
            var body = string.Format("group_id={0}&to_city_id={1}&join_attack_type={2}", groupId, toCityId, attackType);
            var page = agent.WebClient.OpenUrl(url, body);
            return new WorldWarDoJoinAttackPage(page);
        }

        public WorldWarDoJoinAttackPage(string page)
        {
            this.RawPage = page;
        }
    }
}
