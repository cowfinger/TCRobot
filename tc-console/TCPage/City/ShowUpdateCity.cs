using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TC.TCTasks;

namespace TC.TCPage.City
{
    class ShowUpdateCity : TCPage
    {
        public const string PrerequsitePattern =
            @"<td>前置需求</td>.*?" +
            @"\[\[jslang\(""build_(?<buildId>\d+)""\)\]\].*?" +
            @"\[\[jslang\(""level"", ""(?<buildLevel>\d+)""\)\]\].*?" +
            @"<span>科技水平：</span>.*?(?<science>\d+).*?" +
            @"<span>繁荣度：</span>.*?(?<creditPoint>\d+)";

        public const string LevelPattern = @"\(等级(\d+) →　\d+\)";

        public const string RequiredResPattern = @"""\[\[jslang\('\w'\)\]\]""><span>(\d+)</span>";

        public const string HeroSelectPattern = @"hero_select\.init\( \{([""\d:,]+)\}";

        public const string HeroPattern = @"""(\d+)"":""([:\d]+)""";

        public int RequiredBuildId { get; private set; }

        public int RequiredBuildLevel { get; private set; }

        public int RequiredScience { get; private set; }

        public int RequiredCreditPoint { get; private set; }

        public int CurrentLevel { get; private set; }

        public int EfficientHeroId { get; private set; }

        public IList<int> RequiredResourceTable
        {
            get
            {
                var resMatches = Regex.Matches(this.RawPage, RequiredResPattern);
                return (from Match match in resMatches select int.Parse(match.Groups[1].Value)).ToList();
            }
        }

        public static ShowUpdateCity Open(RequestAgent agent, int cityId)
        {
            var url = agent.BuildUrl(
                TCMod.city,
                TCSubMod.city,
                TCOperation.Show,
                TCFunc.update_city,
                new TCRequestArgument(TCElement.city_id, cityId));
            return new ShowUpdateCity(agent.WebClient.OpenUrl(url));
        }

        protected ShowUpdateCity(string page) : base(page)
        {
            var cityLevelMatch = Regex.Match(page, LevelPattern);
            if (cityLevelMatch.Success)
            {
                this.CurrentLevel = int.Parse(cityLevelMatch.Groups[1].Value);
            }
            else if (page.Contains("已提升"))
            {
                this.CurrentLevel = 25;
                return;
            }

            var prerequisiteMatch = Regex.Match(page, PrerequsitePattern, RegexOptions.Singleline);
            if (prerequisiteMatch.Success)
            {
                this.RequiredBuildId = int.Parse(prerequisiteMatch.Groups["buildId"].Value);
                this.RequiredBuildLevel = int.Parse(prerequisiteMatch.Groups["buildLevel"].Value);
                this.RequiredScience = int.Parse(prerequisiteMatch.Groups["science"].Value);
                this.RequiredCreditPoint = int.Parse(prerequisiteMatch.Groups["creditPoint"].Value);
            }

            var heroSelectMatch = Regex.Match(page, HeroSelectPattern, RegexOptions.Singleline);
            if (heroSelectMatch.Success)
            {
                var heroMatches = Regex.Matches(heroSelectMatch.Value, HeroPattern);
                var heroes = from Match match in heroMatches
                    let heroId = int.Parse(match.Groups[1].Value)
                    let duration = FormMain.TimeStr2Sec(match.Groups[2].Value)
                    select new {heroId, duration};
                var heroList = heroes.ToList();
                heroList.Sort( (x, y) => y.duration.CompareTo(x.duration));
                this.EfficientHeroId = heroList.First().heroId;
            }
        }
    }
}
