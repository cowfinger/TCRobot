using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TC.TCPage.Science
{
    class ShowScienceType : TCPage
    {
        public class ScienceType
        {
            public int ScienceTypeId { get; set; }

            public int ScienceLevel { get; set; }
        }

        public IEnumerable<ScienceType> ScienceList
        {
            get
            {
                const string pattern = @"\[\[jslang\('science_(\d+)'\)\]\]\(等级(\d+)\)";
                var matches = Regex.Matches(this.RawPage, pattern, RegexOptions.Singleline);
                return from Match match in matches
                    let scienceId = int.Parse(match.Groups[1].Value)
                    let level = int.Parse(match.Groups[2].Value)
                    select new ScienceType
                    {
                        ScienceLevel = level,
                        ScienceTypeId = scienceId
                    };
            }

        }

        public static ShowScienceType Open(RequestAgent agent, int scienceType)
        {
            var url = agent.BuildUrl(
                TCMod.science,
                TCSubMod.science,
                TCOperation.Show,
                TCFunc.science_type,
                new TCRequestArgument(TCElement.type, scienceType));
            return new ShowScienceType(agent.WebClient.OpenUrl(url));
        }

        protected ShowScienceType(string page) : base(page)
        {
        }
    }
}
