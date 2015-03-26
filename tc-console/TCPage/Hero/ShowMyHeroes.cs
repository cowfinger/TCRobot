namespace TC.TCPage.Hero
{
    class ShowMyHeroes
    {
        public static ShowMyHeroes Open(RequestAgent agent, int heroId, int soldierId, int option)
        {
            var url = agent.BuildUrl(TCMod.hero, TCSubMod.hero, TCOperation.Show, TCFunc.my_heros);
            var rawPage = agent.WebClient.OpenUrl(url);
            return new ShowMyHeroes(rawPage);
        }

        public ShowMyHeroes(string page)
        {
        }
    }
}
