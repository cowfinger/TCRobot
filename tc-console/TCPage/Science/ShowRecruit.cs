using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TC.TCPage.Science
{
    class ShowRecruit : TCPage
    {
        public IList<int> RequiredResourceTable
        {
            get
            {
                var match = Regex.Match(
                    this.RawPage,
                    Build.ShowInnerBuildList.ResourcePattern,
                    RegexOptions.Singleline);
                if (match.Success)
                {
                    var groups = from Group g in match.Groups select g;
                    return groups.Skip(1).Select(t => int.Parse(t.Value)).ToList();
                }

                return new List<int>() { 0, 0, 0, 0};
            }
        }

        public static ShowRecruit Open(RequestAgent agent, int type = 0)
        {
            var url = agent.BuildUrl(
                TCMod.science,
                TCSubMod.science,
                TCOperation.Show,
                TCFunc.recruit,
                new TCRequestArgument(TCElement.type, type));
            return new ShowRecruit(agent.WebClient.OpenUrl(url));
        }

        protected ShowRecruit(string page)
            : base(page)
        {
        }
    }
}
