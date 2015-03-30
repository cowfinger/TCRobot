namespace TC.TCPage.Influence
{
    internal class DoCheckMember
    {
        public enum Action
        {
            pass,

            refuse
        }

        public DoCheckMember(string rawPage)
        {
        }

        public static DoCheckMember Open(RequestAgent agent, Action act, int unionId)
        {
            var url = agent.BuildUrl(
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Do,
                TCFunc.check_member,
                new TCRequestArgument(TCElement.action, act.ToString()),
                new TCRequestArgument(TCElement.union_id, unionId));
            var rawPage = agent.WebClient.OpenUrl(url);
            return new DoCheckMember(rawPage);
        }
    }
}