namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    partial class RequestAgent
    {
        private void OpenAccountFirstCity()
        {
            const string pattern =
                @"index\.php\?mod=influence/influence&op=show&func=influence_city_detail&node_id=(\d+)";

            var url = this.BuildUrl(TCMod.influence, TCSubMod.influence, TCOperation.Show, TCFunc.influence_city);

            var page = this.WebClient.OpenUrl(url);

            var match = Regex.Match(page, pattern);
            if (match.Success)
            {
                var cityUrl = string.Format(
                    "http://{0}/{1}&r={2}",
                    this.Account.AccountType,
                    match.Value,
                    RandGen.NextDouble());
                this.WebClient.OpenUrl(cityUrl);
            }
        }

        private string OpenMoveTroopPage()
        {
            var url = this.BuildUrl(TCMod.military, TCSubMod.world_war, TCOperation.Show, TCFunc.move_army);
            return this.WebClient.OpenUrl(url);
        }

        private string OpenMoveTroopPage(int cityId)
        {
            var url = this.BuildUrl(
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Show,
                TCFunc.move_army,
                new TCRequestArgument(TCElement.from_city_id, cityId));
            return this.WebClient.OpenUrl(url);
        }

        private IEnumerable<CityInfo> ParseCityListFromMoveTroopPage(string content)
        {
            const string cityPattern = "<option value=\"(?<nodeId>\\d+)\"\\s*>(?<name>[^<]+)</option>";
            const string selectedCityPattern = "<option value=\"(?<nodeId>\\d+)\" selected\\s*>(?<name>[^<]+)</option>";

            var contentParts = content.Split(new[] { "目的地：" }, StringSplitOptions.RemoveEmptyEntries);
            var fromCityMatches = Regex.Matches(contentParts[0], cityPattern);
            var selectedCityMatch = Regex.Match(contentParts[0], selectedCityPattern);

            if (selectedCityMatch.Success)
            {
                yield return
                    new CityInfo
                        {
                            Name = selectedCityMatch.Groups["name"].Value,
                            NodeId = int.Parse(selectedCityMatch.Groups["nodeId"].Value),
                            CityId = int.Parse(FormMain.CityList[selectedCityMatch.Groups["name"].Value])
                        };
            }

            foreach (Match cityMatch in fromCityMatches)
            {
                var cityName = cityMatch.Groups["name"].Value;
                var cityId = "0";
                if (!FormMain.CityList.TryGetValue(cityName, out cityId))
                {
                    cityId = "0";
                }

                yield return
                    new CityInfo
                        {
                            Name = cityName,
                            NodeId = int.Parse(cityMatch.Groups["nodeId"].Value),
                            CityId = int.Parse(cityId)
                        };
            }
        }
    }
}