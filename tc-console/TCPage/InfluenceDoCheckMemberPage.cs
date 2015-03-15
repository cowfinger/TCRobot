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

        public static InfluenceDoCheckMemberPage Open(RequestAgent agent, Action act, int unionId)
        {
            var url = agent.BuildUrl(
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Do,
                TCFunc.check_member,
                new TCRequestArgument(TCElement.action, act.ToString()),
                new TCRequestArgument(TCElement.union_id, unionId));
            var rawPage = agent.WebClient.OpenUrl(url);
            return new InfluenceDoCheckMemberPage(rawPage);
        }

        public InfluenceDoCheckMemberPage(string rawPage)
        {
        }
    }
}
