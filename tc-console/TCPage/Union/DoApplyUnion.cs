namespace TC.TCPage.Union
{
    class DoApplyUnion
    {
        public static DoApplyUnion Open(RequestAgent agent, int unionId)
        {
            var url = agent.BuildUrl(
                TCMod.union,
                TCSubMod.union,
                TCOperation.Do,
                TCFunc.apply_union,
                new TCRequestArgument(TCElement.union_id, unionId));
            var rawPage = agent.WebClient.OpenUrl(url);
            return new DoApplyUnion(rawPage);
        }

        public DoApplyUnion(string page)
        {
        }
    }
}
