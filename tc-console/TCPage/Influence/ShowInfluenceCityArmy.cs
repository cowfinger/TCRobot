using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TC.TCPage.Influence
{
    class ShowInfluenceCityArmy : TCPage
    {
        public IEnumerable<string> Cities
        {
            get
            {
                var pattern = new Regex(@"<td width=""12%"">(.*)</td>");
                var matches = pattern.Matches(this.RawPage);
                return from Match match in matches select match.Groups[1].Value;
            }
        }

        public static ShowInfluenceCityArmy Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Show,
                TCFunc.influence_city_army);
            return new ShowInfluenceCityArmy(agent.WebClient.OpenUrl(url));
        }

        protected ShowInfluenceCityArmy(string page)
            : base(page)
        {
        }
    }
}
