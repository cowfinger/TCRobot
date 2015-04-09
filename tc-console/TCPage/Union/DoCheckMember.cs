using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage.Union
{
    class DoCheckMember : TCPage
    {
        public enum Result
        {
            pass,
        }

        public bool Success
        {
            get { return this.RawPage.Contains("添加成员成功"); }
        }

        public static DoCheckMember Open(RequestAgent agent, int memberId, Result result, int inviteFlag = 0)
        {
            var url = agent.BuildUrl(
                TCMod.union,
                TCSubMod.union,
                TCOperation.Do,
                TCFunc.check_member);
            var body = string.Format(
                "member_id={0}&result={1}&invite_flag={2}",
                memberId, result, inviteFlag);
            return new DoCheckMember(agent.WebClient.OpenUrl(url, body));
        }

        protected DoCheckMember(string page) : base(page)
        {
        }
    }
}
