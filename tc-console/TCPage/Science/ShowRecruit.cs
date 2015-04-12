using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TC.TCPage.Science
{
    class ShowRecruit : TCPage
    {
        public int IdleCount
        {
            get
            {
                const string pattern = @"空闲博士数 <span class=""\w+"">(\d+)</span>";
                var match = Regex.Match(this.RawPage, pattern, RegexOptions.Singleline);
                return match.Success ? int.Parse(match.Groups[1].Value) : 0;
            }
        }

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

        public bool CanRecruit
        {
            get
            {
                const string pattern = @"<a id=""science_recruit_a"" href=""javascript:void\(0\);"" class=""disabled"">";
                return !this.RawPage.Contains(pattern);
            }
        }

        public int RecruitCoolDown
        {
            get
            {
                //<p id="science_recruit_cd">00:02:36</p>
                const string pattern = @"<p id=""science_recruit_cd"">(\d\d:\d\d:\d\d)</p>";
                var match = Regex.Match(this.RawPage, pattern, RegexOptions.Singleline);
                return match.Success ? FormMain.TimeStr2Sec(match.Groups[1].Value) : -1;
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
