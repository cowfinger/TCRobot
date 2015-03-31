using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TC.TCPage.Hero
{
    internal class ShowMyHeroes : TCPage
    {
        public static ShowMyHeroes Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(TCMod.hero, TCSubMod.hero, TCOperation.Show, TCFunc.my_heros);
            var rawPage = agent.WebClient.OpenUrl(url);
            return new ShowMyHeroes(rawPage);
        }

        public IEnumerable<HeroInfo> Heroes
        {
            get { return ParseHeroList(this.RawPage); }
        }

        public ShowMyHeroes(string page) : base(page)
        {
        }

        private static IEnumerable<HeroInfo> ParseHeroList(string content)
        {
            const string pattern = "<li id=\"li_hero_my(?<heroid>\\d+)\" hname=\"(?<heroname>.+)\" die=(?<isdead>\\d)>";
            const string statusPattern = @"\(\[\[jslang\('hero_status_(\d+)'\)\]\]\)";

            var matches = Regex.Matches(content, pattern);
            var statusMatches = Regex.Matches(content, statusPattern);
            var heroes = from Match match in matches
                   select
                       new HeroInfo
                           {
                               HeroId = int.Parse(match.Groups["heroid"].Value),
                               Name = match.Groups["heroname"].Value,
                               IsDead = match.Groups["isdead"].Value == "1"
                           };
            var heroStatus = from Match match in statusMatches select int.Parse(match.Groups[1].Value);
            return heroes.Zip(heroStatus, (hero, status) => { hero.Status = status; return hero; });
        }
    }
}