namespace TC.TCPage.Influence
{
    using System.Text.RegularExpressions;

    internal class ShowInfluenceDonate : TCPage
    {
        public const string ResourcePattern = @"value=this\.innerHTML;"">(\d+)</span>\)";

        public long Wood { get; private set; }

        public long MaxWood { get; private set; }

        public long Mud { get; private set; }

        public long MaxMud { get; private set; }

        public long Iron { get; private set; }

        public long MaxIron { get; private set; }

        public long Food { get; private set; }

        public long MaxFood { get; private set; }


        public ShowInfluenceDonate(string page)
            : base(page)
        {
            var matches = Regex.Matches(page, pattern);

            foreach (Match match in matches)
            {
                yield return long.Parse(match.Groups[1].Value);
            }
            this.Wood = resList[0].Key;
            this.MaxWood = resList[0].Value;
            this.Mud = resList[1].Key;
            this.MaxMud = resList[1].Value;
            this.Iron = resList[2].Key;
            this.MaxIron = resList[2].Value;
            this.Food = resList[3].Key;
            this.MaxFood = resList[3].Value;
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