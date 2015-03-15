using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TC.TCPage
{
    class WorldWarShowReserveArmyInfoPage
    {
        public const string ReserveArmySumPattern = @"本城后备军总兵数：(\d+)\s*<span>本城后备将领总数：(\d+)</span>";

        public int ReserveArmyNum { get; set; }

        public int ReserveHeroNum { get; set; }

        public static WorldWarShowReserveArmyInfoPage Open(RequestAgent agent, int tabId)
        {
            var url = agent.BuildUrl(
                TCMod.world,
                TCSubMod.world_war,
                TCOperation.Show,
                TCFunc.reserve_army_info,
                new TCRequestArgument(TCElement.tab_id, tabId),
                new TCRequestArgument(TCElement.user_nickname, ""));
            var rawPage = agent.WebClient.OpenUrl(url);
            return new WorldWarShowReserveArmyInfoPage(rawPage);
        }

        public WorldWarShowReserveArmyInfoPage(string page)
        {
            var reserveArmySumMatch = Regex.Match(page, ReserveArmySumPattern, RegexOptions.Singleline);
            if (reserveArmySumMatch.Success)
            {
                this.ReserveArmyNum = int.Parse(reserveArmySumMatch.Groups[1].Value);
                this.ReserveHeroNum = int.Parse(reserveArmySumMatch.Groups[2].Value);
            }
        }
    }
}
