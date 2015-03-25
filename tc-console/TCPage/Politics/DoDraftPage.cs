using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage
{
    class PoliticsDoDraftPage
    {
        public bool Success { get; private set; }

        public static PoliticsDoDraftPage Open(RequestAgent agent, int heroId, int soldierId, int option)
        {
            var url = agent.BuildUrl(TCMod.civil, TCSubMod.politics, TCOperation.Do, TCFunc.draft);
            var body = string.Format("hero_id={0}&soldier_id={1}&option={2}", heroId, soldierId, option);
            var rawPage = agent.WebClient.OpenUrl(url, body);
            return new PoliticsDoDraftPage(rawPage);
        }

        public PoliticsDoDraftPage(string page)
        {
            this.Success = page.Contains("临时征兵成功");
        }
    }
}
