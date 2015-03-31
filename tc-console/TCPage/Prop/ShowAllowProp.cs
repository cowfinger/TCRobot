using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TC.TCPage.Prop
{
    class ShowAllowProp : TCPage
    {
        public const string pattern =
                "<li onclick=\"shop\\.choose_prop\\((?<prop_id>\\d+), " +
                "(?<user_prop_id>\\d+), 29\\);\">";

        public DepotItem Item
        {
            get
            {
                var match = Regex.Match(this.RawPage, pattern);
                if (!match.Success)
                {
                    return null;
                }

                return new DepotItem
                           {
                               GoodsType = 29,
                               PropertyId = int.Parse(match.Groups["prop_id"].Value),
                               UserPropertyId = int.Parse(match.Groups["user_prop_id"].Value)
                           };
            }
        }

        public static ShowAllowProp Open(RequestAgent agent, int tid, int queueId, int type = 29)
        {
            var callBack = string.Format("get_build_task_queue(undefined,%20{0},%20true)", tid);
            var url = agent.BuildUrl(
                TCMod.prop,
                TCSubMod.prop,
                TCOperation.Show,
                TCFunc.allow_prop,
                new TCRequestArgument(TCElement.type, 29),
                new TCRequestArgument(TCElement.queue_id, queueId),
                new TCRequestArgument(TCElement.call_back, callBack));
            return new ShowAllowProp(agent.WebClient.OpenUrl(url));
        }

        protected ShowAllowProp(string page)
            : base(page)
        {
        }
    }
}
