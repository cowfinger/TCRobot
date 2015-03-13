using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.TCPage
{
    class InfluenceDoBuildRepairPage
    {
        public static InfluenceDoBuildRepairPage Open(
            AccountInfo account,
            int cityId,
            int buildId,
            int brickNum)
        {
            var url = RequestAgent.BuildUrl(
                account.AccountType,
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Do,
                TCFunc.build_repair,
                new TCRequestArgument(TCElement.build_id, cityId),
                new TCRequestArgument(TCElement.node_id, buildId),
                new TCRequestArgument(TCElement.brick_num, brickNum));
            var webClient = new HttpClient(account.CookieStr);
            var rawPage = webClient.OpenUrl(url);
            return new InfluenceDoBuildRepairPage(rawPage);
        }

        public InfluenceDoBuildRepairPage(string page)
        {
        }
    }
}
