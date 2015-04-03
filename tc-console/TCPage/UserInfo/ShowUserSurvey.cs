using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TC.TCPage.UserInfo
{
    class ShowUserSurvey : TCPage
    {
        public const string CreditPattern = @"<td>繁&nbsp;荣&nbsp;度 </td>.*?<td>(\d+)</td>";

        public int CreditPoint
        {
            get
            {
                var creditMatch = Regex.Match(this.RawPage, CreditPattern, RegexOptions.Singleline);
                return creditMatch.Success ? int.Parse(creditMatch.Groups[1].Value) : 0;
            }
        }

        public static ShowUserSurvey Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(
                TCMod.city,
                TCSubMod.attack,
                TCOperation.Show,
                TCFunc.allow_prop);
            return new ShowUserSurvey(agent.WebClient.OpenUrl(url));
        }

        protected ShowUserSurvey(string page)
            : base(page)
        {
        }
    }
}
