﻿namespace TC.TCPage.Influence
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal class ShowCheckMember
    {
        //<td><input type="checkbox" value="22616" /></td> <td>自由行</td> <td>秦楚</td>
        public const string MemberPattern =
            "<td>\\s*<input type=\"checkbox\" value=\"(?<UnionId>\\d+)\" />\\s*"
            + "</td>\\s*<td>(?<UnionName>.+?)</td>\\s*<td>(?<LeadName>.+?)</td>";

        public ShowCheckMember(string page)
        {
            var matches = Regex.Matches(page, MemberPattern, RegexOptions.Singleline);
            this.RequestMemberList = from Match match in matches
                                     select
                                         new MemberInfo
                                             {
                                                 UnionId = int.Parse(match.Groups["UnionId"].Value),
                                                 UnionName = match.Groups["UnionName"].Value,
                                                 LeadName = match.Groups["LeadName"].Value
                                             };
        }

        public IEnumerable<MemberInfo> RequestMemberList { get; private set; }

        public static ShowCheckMember Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(TCMod.influence, TCSubMod.influence, TCOperation.Show, TCFunc.check_member);
            var rawPage = agent.WebClient.OpenUrl(url);
            return new ShowCheckMember(rawPage);
        }

        public class MemberInfo
        {
            public int UnionId { get; set; }

            public string UnionName { get; set; }

            public string LeadName { get; set; }
        }
    }
}