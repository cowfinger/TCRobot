namespace TC.TCPage.Union
{
    internal class DoOutUnion
    {
        public DoOutUnion(string page)
        {
        }

        public static DoOutUnion Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(TCMod.union, TCSubMod.union, TCOperation.Do, TCFunc.out_union);
            var rawPage = agent.WebClient.OpenUrl(url);
            return new DoOutUnion(rawPage);
        }
    }
}