namespace TC.TCPage.Union
{
    class DoOutUnionPage
    {
        public static DoOutUnionPage Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(
                TCMod.union,
                TCSubMod.union,
                TCOperation.Do,
                TCFunc.out_union);
            var rawPage = agent.WebClient.OpenUrl(url);
            return new DoOutUnionPage(rawPage);
        }

        public DoOutUnionPage(string page)
        {
        }
    }
}
