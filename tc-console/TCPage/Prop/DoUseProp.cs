namespace TC.TCPage.Prop
{
    internal class DoUseProp : TCPage
    {
        public const string CallBack = "shop.reflash_res_depot(5,%201)";

        protected DoUseProp(string page)
            : base(page)
        {
        }

        public static DoUseProp Open(
            RequestAgent agent,
            int propId,
            int userPropId,
            int heroId,
            int queueId,
            int tid,
            int propNum = 1)
        {
            var callBack = string.Format("get_build_task_queue%28undefined%2C+{0}%2C+true%29", tid);
            var url = agent.BuildUrl(
                TCMod.prop,
                TCSubMod.prop,
                TCOperation.Do,
                TCFunc.use_prop,
                new TCRequestArgument(TCElement.prop_id, propId),
                new TCRequestArgument(TCElement.user_prop_id, userPropId),
                new TCRequestArgument(TCElement.hero_id, heroId),
                new TCRequestArgument(TCElement.queue_id, queueId),
                new TCRequestArgument(TCElement.call_back, callBack));
            return new DoUseProp(agent.WebClient.OpenUrl(url));
        }

        public static DoUseProp Open(
            RequestAgent agent,
            int propId,
            int userPropId,
            int propNum,
            int pType = 7,
            int from = 1)
        {
            var url = agent.BuildUrl(
                TCMod.prop,
                TCSubMod.prop,
                TCOperation.Do,
                TCFunc.use_prop,
                new TCRequestArgument(TCElement.p_type, pType),
                new TCRequestArgument(TCElement.prop_id, propId),
                new TCRequestArgument(TCElement.prop_num, propNum),
                new TCRequestArgument(TCElement.user_prop_id, userPropId),
                new TCRequestArgument(TCElement.from, from),
                new TCRequestArgument(TCElement.call_back, CallBack));
            return new DoUseProp(agent.WebClient.OpenUrl(url));
        }
    }
}