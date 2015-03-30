using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage.Influence
{
    using System.Text.RegularExpressions;

    class ShowInfluenceCity : TCPage
    {
        public const string CityDetailUrlPattern = @"mod=influence/influence&op=show&func=influence_city_detail&node_id=(\d+)";

        public IEnumerable<int> Citys
        {
            get
            {
                var matches = Regex.Matches(this.RawPage, CityDetailUrlPattern);
                return from Match match in matches select int.Parse(match.Groups[1].Value);
            }
        }

        public static ShowInfluenceCity Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Show,
                TCFunc.influence_city);
            return new ShowInfluenceCity(agent.WebClient.OpenUrl(url));
        }

        protected ShowInfluenceCity(string page)
            : base(page)
        {
        }
    }
}
