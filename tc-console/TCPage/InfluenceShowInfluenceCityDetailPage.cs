using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.TCPage
{
    class InfluenceShowInfluenceCityDetailPage
    {
        public static InfluenceShowInfluenceCityDetailPage Open(AccountInfo account, int cityNodeId)
        {
            var url = RequestAgent.BuildUrl(
                account.AccountType,
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Show,
                TCFunc.influence_city_detail,
                new TCRequestArgument(TCElement.node_id, cityNodeId));
            var webClient = new HttpClient(account.CookieStr);
            var rawPage = webClient.OpenUrl(url);
            var page = new InfluenceShowInfluenceCityDetailPage(rawPage);
            account.CookieStr = webClient.Cookie.CookieString;
            return page;
        }

        public InfluenceShowInfluenceCityDetailPage(string page)
        {
        }
    }
}
