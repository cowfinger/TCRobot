using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage.Science
{
    using System.Text.RegularExpressions;

    class ShowScience : TCPage
    {
        public long Wood { get; private set; }

        public long MaxWood { get; private set; }

        public long Mud { get; private set; }

        public long MaxMud { get; private set; }

        public long Iron { get; private set; }

        public long MaxIron { get; private set; }

        public long Food { get; private set; }

        public long MaxFood { get; private set; }

        public static ShowScience Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(
                TCMod.influence,
                TCSubMod.science,
                TCOperation.Show,
                TCFunc.science);
            return new ShowScience(agent.WebClient.OpenUrl(url));
        }

        protected ShowScience(string page)
            : base(page)
        {
            var resList = ParseInfluenceResource(page).ToList();
            this.Wood = resList[0].Key;
            this.MaxWood = resList[0].Value;
            this.Mud = resList[1].Key;
            this.MaxMud = resList[1].Value;
            this.Iron = resList[2].Key;
            this.MaxIron = resList[2].Value;
            this.Food = resList[3].Key;
            this.MaxFood = resList[3].Value;
        }

        private static IEnumerable<KeyValuePair<long, long>> ParseInfluenceResource(string page)
        {
            const string Pattern = @"<div class=""num\"">(\d+)/(\d+)</div>";
            var matches = Regex.Matches(page, Pattern);
            return from Match match in matches
                   let key = long.Parse(match.Groups[1].Value)
                   let value = long.Parse(match.Groups[2].Value)
                   select new KeyValuePair<long, long>(key, value);
        }
    }
}
