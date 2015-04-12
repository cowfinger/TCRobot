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
                const string pattern = @"hero_select\.init\(\{""(\d+)"":";
                var match = Regex.Match(this.RawPage, pattern, RegexOptions.Singleline);
                return match.Success ? int.Parse(match.Groups[1].Value) : 0;
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
