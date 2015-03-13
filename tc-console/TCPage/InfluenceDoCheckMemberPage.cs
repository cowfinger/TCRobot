using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.TCPage
{
    class InfluenceDoCheckMemberPage
    {
        public enum Action
        {
            pass, refuse,
        }

        public static InfluenceDoCheckMemberPage Open(AccountInfo account, Action act, int unionId)
        {
            var url = RequestAgent.BuildUrl(
                account.AccountType,
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Do,
                TCFunc.check_member,
                new TCRequestArgument(TCElement.action, act.ToString()),
                new TCRequestArgument(TCElement.union_id, unionId));
            var webClient = new HttpClient(account.CookieStr);
            var rawPage = webClient.OpenUrl(url);
            return new InfluenceDoCheckMemberPage(rawPage);
        }

        public InfluenceDoCheckMemberPage(string rawPage)
        {
        }
    }
}
