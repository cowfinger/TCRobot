using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TC.TCPage
{
    class InfluenceCheckMemberPage
    {
        //<td><input type="checkbox" value="22616" /></td> <td>自由行</td> <td>秦楚</td>
        public const string MemberPattern = 
            "<td><input type=\"checkbox\" value=\"(?<UnionId>\\d+)\" />" + 
            "</td> <td>(?<UnionName>.+?)</td> <td>(?<LeadName>.+?)</td>";

        public class MemberInfo
        {
            public int UnionId { get; set; }

            public string UnionName { get; set; }

            public string LeadName { get; set; }
        }

        public IEnumerable<MemberInfo> RequestMemberList { get; private set; }

        public InfluenceCheckMemberPage(string page)
        {
            var matches = Regex.Matches(page, MemberPattern, RegexOptions.Singleline);
            this.RequestMemberList = from Match match in matches
                                     select new MemberInfo
                                     {
                                         UnionId = int.Parse(match.Groups["UnionId"].Value),
                                         UnionName = match.Groups["UnionName"].Value,
                                         LeadName = match.Groups["LeadName"].Value,
                                     };
        }
    }
}
