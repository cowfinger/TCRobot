using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TC.TCPage
{
    class WorldWarShowMoveArmyPage
    {
        public int BrickNum { get; private set; }

        public IEnumerable<int> HeroIdList { get; private set; }

        public IEnumerable<Soldier> Army { get; private set; }

        public static WorldWarShowMoveArmyPage Open(RequestAgent agent, int fromCityId)
        {
            var url = agent.BuildUrl(
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Show,
                TCFunc.move_army,
                new TCRequestArgument(TCElement.from_city_id, fromCityId));
            var rawPage = agent.WebClient.OpenUrl(url);
            return new WorldWarShowMoveArmyPage(rawPage);
        }

        public WorldWarShowMoveArmyPage(string page)
        {
            this.BrickNum = ParseBrickNumberFromMovePage(page);
            this.HeroIdList = ParseHeroIdListFromMovePage(page);
            this.Army = ParseSoldierListFromMovePage(page);
        }

        private static IEnumerable<int> ParseHeroIdListFromMovePage(string page)
        {
            const string pattern = "hero_id=\"(\\d+)\" hero_status=\"(\\d+)\"";
            var matches = Regex.Matches(page, pattern);
            return from Match match in matches select int.Parse(match.Groups[1].Value);
        }

        private static int ParseBrickNumberFromMovePage(string page)
        {
            const string pattern = "<span id=\"brick_num_max\">\\d+</span>/(\\d+)</span>\\)</span>";
            var match = Regex.Match(page, pattern);
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }

        private static IEnumerable<Soldier> ParseSoldierListFromMovePage(string page)
        {
            const string idPattern = "max_num=(\\d+) name=\"s_(\\d+)\"";
            return from Match match in Regex.Matches(page, idPattern)
                   select
                       new Soldier
                           {
                               Name = FormMain.KeyWordMap[string.Format("soldier_{0}", match.Groups[2].Value)],
                               SoldierType = int.Parse(match.Groups[2].Value),
                               SoldierNumber = int.Parse(match.Groups[1].Value)
                           };
        }
    }
}
