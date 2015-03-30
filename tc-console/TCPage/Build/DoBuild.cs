namespace TC.TCPage.Build
{
    internal class DoBuild
    {
        public DoBuild(string page)
        {
            this.RawPage = page;
        }

        public string RawPage { get; private set; }

        public bool Success
        {
            get
            {
                return this.RawPage.StartsWith(">[[jslang(\"endure\")]]:");
            }
        }

        public static DoBuild Open(RequestAgent agent, int pid, int bid, int hid)
        {
            var url = agent.BuildUrl(TCMod.city, TCSubMod.build, TCOperation.Show, TCFunc.inner_build_list);
            var body = string.Format("pid={0}&bid={1}&hid={2}", pid, bid, hid);
            var page = agent.WebClient.OpenUrl(url, body);
            return new DoBuild(page);
        }
    }
}