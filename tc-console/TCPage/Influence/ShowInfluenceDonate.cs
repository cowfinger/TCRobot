using System.Collections.Generic;
using System.Linq;

namespace TC.TCPage.Influence
{
    using System.Text.RegularExpressions;

    internal class ShowInfluenceDonate : TCPage
    {
        public const string ResourcePattern = @"value=this\.innerHTML;"">(\d+)</span>\)";

        public long Wood { get; private set; }

        public long Mud { get; private set; }

        public long Iron { get; private set; }

        public long Food { get; private set; }

        public IList<long> ResourcsTable { get; private set; }

        public ShowInfluenceDonate(string page)
            : base(page)
        {
            var matches = Regex.Matches(page, ResourcePattern);
            this.ResourcsTable = (from Match match in matches select long.Parse(match.Groups[1].Value)).ToList();
            this.Wood = this.ResourcsTable[0];
            this.Mud = this.ResourcsTable[1];
            this.Iron = this.ResourcsTable[2];
            this.Food = this.ResourcsTable[3];
        }

        public static ShowInfluenceDonate Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Show,
                TCFunc.influence_donate);
            return new ShowInfluenceDonate(agent.WebClient.OpenUrl(url));
        }
    }
}