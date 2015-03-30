namespace TC.TCPage.WorldWar
{
    internal class DoJoinAttack
    {
        public DoJoinAttack(string page)
        {
            this.RawPage = page;
        }

        public string RawPage { get; private set; }

        public static DoJoinAttack Open(RequestAgent agent, int groupId, int toCityId, int attackType = 1)
        {
            var url = agent.BuildUrl(TCMod.military, TCSubMod.world_war, TCOperation.Do, TCFunc.join_attack);
            var body = string.Format("group_id={0}&to_city_id={1}&join_attack_type={2}", groupId, toCityId, attackType);
            var page = agent.WebClient.OpenUrl(url, body);
            return new DoJoinAttack(page);
        }
    }
}