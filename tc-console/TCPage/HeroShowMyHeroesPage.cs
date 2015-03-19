using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage
{
    class HeroShowMyHeroesPage
    {
        public static HeroShowMyHeroesPage Open(RequestAgent agent, int heroId, int soldierId, int option)
        {
            var url = agent.BuildUrl(TCMod.hero, TCSubMod.hero, TCOperation.Show, TCFunc.my_heros);
            var rawPage = agent.WebClient.OpenUrl(url);
            return new HeroShowMyHeroesPage(rawPage);
        }

        public HeroShowMyHeroesPage(string page)
        {
        }
    }
}
