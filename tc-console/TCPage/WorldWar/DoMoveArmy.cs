namespace TC.TCPage.WorldWar
{
    internal class DoMoveArmy
    {
        public DoMoveArmy(string page)
        {
            this.Success = !page.Contains("alert");
        }

        public bool Success { get; set; }

        public static DoMoveArmy Open(
            RequestAgent agent,
            int fromCityId,
            int toCityId,
            string soldierString,
            string heroString,
            int brickCount)
        {
            var url = agent.BuildUrl(TCMod.military, TCSubMod.world_war, TCOperation.Do, TCFunc.move_army);

            var body = string.Format(
                "from_city_id={0}&to_city_id={1}&soldier={2}&hero={3}&brick_num={4}",
                fromCityId,
                toCityId,
                soldierString,
                heroString,
                brickCount > 0 ? brickCount.ToString() : "");
            var rawPage = agent.WebClient.OpenUrl(url, body);
            return new DoMoveArmy(rawPage);
        }
    }
}