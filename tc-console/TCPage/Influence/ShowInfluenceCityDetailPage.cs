using System.Text.RegularExpressions;

namespace TC.TCPage.Influence
{
    class ShowInfluenceCityDetailPage
    {
        public const string CityNamePattern = "<!--开始-->\\s*<div class=\"title\">(.+?)</div>";

        public const string CityNodeIdPattern = @"node_id=(\d+)";

        public const string FortressEndurePattern = @"<td>要塞耐久度</td>\s*<td>(\d+)/(\d+)</td>";

        public const string WallEndurePattern = @"<td>城墙耐久度</td>\s*<td>(\d+)/(\d+)</td>";

        public string CityName { get; private set; }

        public int CityNodeId { get; private set; }

        public int FortressEndure { get; private set; }

        public int MaxFortressEndure { get; private set; }

        public int WallEndure { get; private set; }

        public int MaxWallEndure { get; private set; }

        public static ShowInfluenceCityDetailPage Open(RequestAgent agent, int cityNodeId)
        {
            var url = agent.BuildUrl(
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Show,
                TCFunc.influence_city_detail,
                new TCRequestArgument(TCElement.node_id, cityNodeId));
            var rawPage = agent.WebClient.OpenUrl(url);
            return new ShowInfluenceCityDetailPage(rawPage);
        }

        public ShowInfluenceCityDetailPage(string page)
        {
            var cityNameMatch = Regex.Match(page, CityNamePattern, RegexOptions.Singleline);
            if (cityNameMatch.Success)
            {
                this.CityName = cityNameMatch.Groups[1].Value;
            }

            var cityNodeMatch = Regex.Match(page, CityNodeIdPattern, RegexOptions.Singleline);
            if (cityNodeMatch.Success)
            {
                this.CityNodeId = int.Parse(cityNodeMatch.Groups[1].Value);
            }

            var fortressEndureMatch = Regex.Match(page, FortressEndurePattern, RegexOptions.Singleline);
            if (fortressEndureMatch.Success)
            {
                this.FortressEndure = int.Parse(fortressEndureMatch.Groups[1].Value);
                this.MaxFortressEndure = int.Parse(fortressEndureMatch.Groups[2].Value);
            }

            var wallEndureMatch = Regex.Match(page, WallEndurePattern, RegexOptions.Singleline);
            if (wallEndureMatch.Success)
            {
                this.WallEndure = int.Parse(wallEndureMatch.Groups[1].Value);
                this.MaxWallEndure = int.Parse(wallEndureMatch.Groups[2].Value);
            }
        }
    }
}
