using System.Text.RegularExpressions;

namespace TC.TCPage.WorldWar
{
    class ShowReserveArmyInfoPage
    {
        public const string ReserveArmySumPattern = @"本城后备军总兵数：(\d+)\s*<span>本城后备将领总数：(\d+)</span>";

        public int ReserveArmyNum { get; set; }

        public int ReserveHeroNum { get; set; }

        public static ShowReserveArmyInfoPage Open(RequestAgent agent, int tabId)
        {
            var url = agent.BuildUrl(
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Show,
                TCFunc.reserve_army_info,
                new TCRequestArgument(TCElement.tab_id, tabId),
                new TCRequestArgument(TCElement.user_nickname, ""));
            var rawPage = agent.WebClient.OpenUrl(url);
            return new ShowReserveArmyInfoPage(rawPage);
        }

        public ShowReserveArmyInfoPage(string page)
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
