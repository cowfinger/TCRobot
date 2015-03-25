﻿namespace TC.TCPage.Hero
{
    class ShowMyHeroesPage
    {
        public static ShowMyHeroesPage Open(RequestAgent agent, int heroId, int soldierId, int option)
        {
            var url = agent.BuildUrl(TCMod.hero, TCSubMod.hero, TCOperation.Show, TCFunc.my_heros);
            var rawPage = agent.WebClient.OpenUrl(url);
            return new ShowMyHeroesPage(rawPage);
        }

        public ShowMyHeroesPage(string page)
        {
        }
    }
}
