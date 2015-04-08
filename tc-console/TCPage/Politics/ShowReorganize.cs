using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TC.TCPage.Politics
{
    class ShowReorganize : TCPage
    {
        public int FirstHeroId
        {
            get
            {
                const string pattern = @"hero_select\.init\(\{""(\d+)""";
                var match = Regex.Match(this.RawPage, pattern);
                return match.Success ? int.Parse(match.Groups[1].Value) : 0;
            }
        }

        public static ShowReorganize Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(
                TCMod.civil,
                TCSubMod.politics,
                TCOperation.Show,
                TCFunc.reorganize);
            return new ShowReorganize(agent.WebClient.OpenUrl(url));
        }

        protected ShowReorganize(string page) : base(page)
        {
        }
    }
}
