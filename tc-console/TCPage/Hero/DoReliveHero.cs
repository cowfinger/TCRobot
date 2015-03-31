using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage.Hero
{
    class DoReliveHero : TCPage
    {
        public static DoReliveHero Open(RequestAgent agent, int heroId)
        {
            var url = agent.BuildUrl(
                TCMod.hero,
                TCSubMod.hero,
                TCOperation.Do,
                TCFunc.relive_hero,
                new TCRequestArgument(TCElement.hero_id, heroId));
            return new DoReliveHero(agent.WebClient.OpenUrl(url));
        }

        protected DoReliveHero(string page) : base(page)
        {
        }
    }
}
