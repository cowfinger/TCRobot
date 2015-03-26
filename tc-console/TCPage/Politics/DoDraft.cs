namespace TC.TCPage.Politics
{
    class DoDraft
    {
        public bool Success { get; private set; }

        public static DoDraft Open(RequestAgent agent, int heroId, int soldierId, int option)
        {
            var url = agent.BuildUrl(TCMod.civil, TCSubMod.politics, TCOperation.Do, TCFunc.draft);
            var body = string.Format("hero_id={0}&soldier_id={1}&option={2}", heroId, soldierId, option);
            var rawPage = agent.WebClient.OpenUrl(url, body);
            return new DoDraft(rawPage);
        }

        public DoDraft(string page)
        {
            this.Success = page.Contains("临时征兵成功");
        }
    }
}
