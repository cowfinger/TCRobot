using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TC.TCPage.WorldWar
{
    class ShowJoinAttack : TCPage
    {
        public static ShowJoinAttack Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Show,
                TCFunc.join_attack);
            return new ShowJoinAttack(agent.WebClient.OpenUrl(url));
        }

        protected ShowJoinAttack(string page) : base(page)
        {
        }

        private static string ParseTargetCityId(string content, string keyword)
        {
            var re = new Regex(@"<option value=\""([0-9]+)\"">([^<]*)</option>");

            var ms = re.Matches(content);
            var cities = from Match match in ms
                where match.Groups[2].Value == keyword
                select match.Groups[1].Value;
            return cities.FirstOrDefault();
        }
    }
}
