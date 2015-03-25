namespace TC.TCPage.Union
{
    class DoApplyUnionPage
    {
        public static DoApplyUnionPage Open(RequestAgent agent, int unionId)
        {
            var url = agent.BuildUrl(
                TCMod.union,
                TCSubMod.union,
                TCOperation.Do,
                TCFunc.apply_union,
                new TCRequestArgument(TCElement.union_id, unionId));
            var rawPage = agent.WebClient.OpenUrl(url);
            return new DoApplyUnionPage(rawPage);
        }

        public DoApplyUnionPage(string page)
        {
        }
    }
}
