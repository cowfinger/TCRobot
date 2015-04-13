using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TC.TCPage.Pub
{
    class ShowPubHeros : TCPage
    {
        public const string HeroAttributePattern =
            @"function\(\)\{ hero.empolyHero\( (?<sort>\d+) \); \} \).*?" +
            @"jstitle::hero\.hero_tip\( \['.+?','\d'," +
            @"'(?<lValue>\d+\.\d+)'," +
            @"'(?<mValue>\d+\.\d+)'," +
            @"'(?<iValue>\d+\.\d+)'," +
            @"'(?<pValue>\d+\.\d+)'," +
            @"'(?<cValue>\d+\.\d+)'," +
            @"'(?<lInc>\w)'," +
            @"'(?<mInc>\w)'," +
            @"'(?<pInc>\w)'," +
            @"'(?<iInc>\w)'," +
            @"'(?<cInc>\w)'," +
            @"\d+," +
            @"'\d+'," +
            @"'(?<slot>\d)'," +
            @"'\d'," +
            @"'\[\{";

        public const string RefreshCoolDownPattern = @"wee\.timer\.show\( 'refresh_time', (\d+), wee\.empty \);";

        public class HeroAttribute
        {
            public int Sort { get; set; }

            public int LInc { get; set; }

            public int MInc { get; set; }

            public int IInc { get; set; }

            public int PInc { get; set; }

            public int CInc { get; set; }

            public int Slot { get; set; }
        }

        public static int GetHeroIncValue(string str)
        {
            switch (str)
            {
                case "SS":
                    return 8;
                case "S":
                    return 7;
                case "A":
                    return 6;
                case "B":
                    return 5;
                case "C":
                    return 4;
                case "D":
                    return 3;
                case "E":
                    return 2;
                case "F":
                    return 1;
            }
            return 0;
        }

        public int RefreshCoolDown
        {
            get
            {
                var match = Regex.Match(this.RawPage, RefreshCoolDownPattern);
                return match.Success ? int.Parse(match.Groups[1].Value) : 10 * 60;
            }
        }

        public IEnumerable<HeroAttribute> Heroes
        {
            get
            {
                var matches = Regex.Matches(this.RawPage, HeroAttributePattern, RegexOptions.Singleline);
                return from Match match in matches
                    select new HeroAttribute()
                    {
                        Sort = int.Parse(match.Groups["sort"].Value),
                        LInc = GetHeroIncValue(match.Groups["lInc"].Value),
                        MInc = GetHeroIncValue(match.Groups["mInc"].Value),
                        IInc = GetHeroIncValue(match.Groups["iInc"].Value),
                        PInc = GetHeroIncValue(match.Groups["pInc"].Value),
                        CInc = GetHeroIncValue(match.Groups["cInc"].Value),
                        Slot = int.Parse(match.Groups["slot"].Value)
                    };
            }
        }

        public static ShowPubHeros Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(
                TCMod.hero,
                TCSubMod.pub,
                TCOperation.Show,
                TCFunc.pub_heros);
            return new ShowPubHeros(agent.WebClient.OpenUrl(url));
        }

        protected ShowPubHeros(string page)
            : base(page)
        {
        }
    }
}
