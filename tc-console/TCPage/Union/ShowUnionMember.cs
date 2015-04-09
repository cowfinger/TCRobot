using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TC.TCPage.Union
{
    class ShowUnionMember : TCPage
    {
        public const string UserInfoPattern = @"user_info\.show\(1, (\d+) \)";

        public IEnumerable<int> RequestUsers
        {
            get
            {
                var matches = Regex.Matches(this.RawPage, UserInfoPattern, RegexOptions.Singleline);
                return from Match match in matches
                    select int.Parse(match.Groups[1].Value);
            }
        }

        public static ShowUnionMember Open(RequestAgent agent, int unionId, string action = "new")
        {
            var url = agent.BuildUrl(
                TCMod.union,
                TCSubMod.union,
                TCOperation.Show,
                TCFunc.union_member,
                new TCRequestArgument(TCElement.union_id, unionId),
                new TCRequestArgument(TCElement.action, action));
            return new ShowUnionMember(agent.WebClient.OpenUrl(url));
        }

        protected ShowUnionMember(string page) : base(page)
        {
        }
    }
}
