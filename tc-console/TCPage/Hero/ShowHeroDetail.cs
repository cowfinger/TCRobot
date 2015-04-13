using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage.Hero
{
    class ShowHeroDetail : TCPage
    {

        public static ShowHeroDetail Open(RequestAgent agent, int heroId)
        {
            var url = agent.BuildUrl(
                TCMod.hero,
                TCSubMod.hero,
                TCOperation.Show,
                TCFunc.hero_detail,
                new TCRequestArgument(TCElement.hero_id, heroId));
            return new ShowHeroDetail(agent.WebClient.OpenUrl(url));
        }

        protected ShowHeroDetail(string page) : base(page)
        {
        }
    }
}
