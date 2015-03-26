namespace TC.TCPage.Influence
{
    class DoCancelApplyInfluence
    {
        public bool Success { get; private set; }

        public static DoCancelApplyInfluence Open(RequestAgent agent, int influenceId)
        {
            var url = agent.BuildUrl(
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Do,
                TCFunc.cancel_apply_influence,
                new TCRequestArgument(TCElement.influence_id, influenceId));
            var page = agent.WebClient.OpenUrl(url);
            return new DoCancelApplyInfluence(page);
        }

        public DoCancelApplyInfluence(string page)
        {
            this.Success = page.Contains("wee.lang('yes')");
        }
    }
}
