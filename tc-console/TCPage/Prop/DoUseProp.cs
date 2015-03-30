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