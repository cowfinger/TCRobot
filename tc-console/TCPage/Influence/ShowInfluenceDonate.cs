namespace TC.TCPage.Influence
{
    internal class ShowInfluenceDonate : TCPage
    {
        public const string ResourcePattern = @"value=this\.innerHTML;"">(\d+)</span>\)";

        public ShowInfluenceDonate(string page)
            : base(page)
        {
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