namespace TC.TCPage.Union
{
    class DoOutUnion
    {
        public static DoOutUnion Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(
                TCMod.union,
                TCSubMod.union,
                TCOperation.Do,
                TCFunc.out_union);
            var rawPage = agent.WebClient.OpenUrl(url);
            return new DoOutUnion(rawPage);
        }

        public DoOutUnion(string page)
        {
        }
    }
}
