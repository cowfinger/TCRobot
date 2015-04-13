using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TC.TCPage.Pub
{
    class DoEmployHero : TCPage
    {
        public bool Success
        {
            get { return this.RawPage.Contains("成功招募"); }
        }

        public static DoEmployHero Open(RequestAgent agent, int sort)
        {
            var url = agent.BuildUrl(
                TCMod.hero,
                TCSubMod.pub,
                TCOperation.Do,
                TCFunc.employ_hero,
                new TCRequestArgument(TCElement.sort, sort));
            return new DoEmployHero(agent.WebClient.OpenUrl(url));
        }

        protected DoEmployHero(string page) : base(page)
        {
        }
    }
}
