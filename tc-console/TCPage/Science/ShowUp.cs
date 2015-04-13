using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TC.TCPage.Science
{
    class ShowUp : TCPage
    {
        public int HeroId
        {
            get
            {
                const string heroBarPattern = @"id=""science_up_need_hero_attr"" value=""(.+?)"" />";
                const string heroAttrPattern = @"""(?<id>\d+)"":" +
                    @"\{""time"":""""," +
                    @"""lead"":""(?<l>\d+\.\d+)""," +
                    @"""force"":""(?<f>\d+\.\d+)"","+
                    @"""intellect"":""(?<i>\d+\.\d+)""," +
                    @"""political"":""(?<p>\d+\.\d+)""," +
                    @"""charm"":""(?<c>\d+\.\d+)""," +
                    @"""status"":""1""\}";

                var heroMatches = Regex.Matches(this.RawPage, heroAttrPattern, RegexOptions.Singleline);
                var heroes = (from Match match in heroMatches
                    let id = int.Parse(match.Groups["id"].Value)
                    let lead = double.Parse(match.Groups["l"].Value)
                    let force = double.Parse(match.Groups["f"].Value)
                    let intellect = double.Parse(match.Groups["i"].Value)
                    let policical = double.Parse(match.Groups["p"].Value)
                    let charm = double.Parse(match.Groups["c"].Value)
                    select new {id, lead, force, intellect, policical, charm}).ToList();
                if (!heroes.Any())
                {
                    return 0;
                }

                var heroBarMatch = Regex.Match(this.RawPage, heroBarPattern, RegexOptions.Singleline);
                if (!heroBarMatch.Success)
                {
                    return heroes.First().id;
                }

                var heroBarStr = heroBarMatch.Groups[1].Value;
                var heroBars = (from heroBar in heroBarStr.Split('|')
                    let barPair = heroBar.Split(':')
                    let barKey = barPair[0]
                    let barVal = int.Parse(barPair[1])
                    select new {barKey, barVal}).ToList();

                var validHeroes = heroes.Where(hero =>
                {
                    return !heroBars.Any(bar =>
                    {
                        switch (bar.barKey)
                        {
                            case "lead":
                                return hero.lead < bar.barVal;
                            case "force":
                                return hero.force < bar.barVal;
                            case "intellect":
                                return hero.intellect < bar.barVal;
                            case "political":
                                return hero.policical < bar.barVal;
                            case "charm":
                                return hero.charm < bar.barVal;
                        }
                        return false;
                    });
                }).Select(hero => hero.id).ToList();

                return validHeroes.Any() ? validHeroes.First() : 0;
            }
        }

        public static ShowUp Open(RequestAgent agent, int scienceId)
        {
            var url = agent.BuildUrl(
                TCMod.science,
                TCSubMod.science,
                TCOperation.Show,
                TCFunc.up,
                new TCRequestArgument(TCElement.science_id, scienceId));
            return new ShowUp(agent.WebClient.OpenUrl(url));
        }

        protected ShowUp(string page) : base(page)
        {
        }
    }
}
