namespace TC.TCTasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Timers;

    using TC.TCPage.Influence;
    using TC.TCUtility;

    internal class InfluenceGuard : TCTask
    {
        private const int CheckInterval = 1 * 1000;

        public List<string> RecentRequstUnionList;

        public Timer RefuseTimer;

        public HashSet<int> UnionIdSet = new HashSet<int>();

        public InfluenceGuard(AccountInfo account)
            : base(account, CheckInterval)
        {
            this.RefuseTimer = new Timer(500) { AutoReset = true };
            this.RefuseTimer.Elapsed += (sender, args) =>
                {
                    List<int> unionIdList;
                    lock (this.UnionIdSet)
                    {
                        unionIdList = this.UnionIdSet.ToList();
                    }

                    Parallel.Dispatch(
                        unionIdList,
                        unionId =>
                            {
                                DoCheckMember.Open(
                                    account.WebAgent,
                                    DoCheckMember.Action.refuse, unionId);
                            });
                };
            this.RefuseTimer.Start();
        }

        public override string TaskId
        {
            get
            {
                return "Refuse Union";
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override string GetTaskHint()
        {
            return this.RecentRequstUnionList != null ? string.Join(", ", this.RecentRequstUnionList.ToArray()) : "";
        }

        public override void TaskWorker()
        {
            var page = ShowCheckMember.Open(this.Account.WebAgent);
            this.RecentRequstUnionList = page.RequestMemberList.Select(item => item.UnionName).ToList();

            lock (this.UnionIdSet)
            {
                foreach (
                    var member in
                        page.RequestMemberList.Where(member => !this.UnionIdSet.Contains(member.UnionId)))
                {
                    this.UnionIdSet.Add(member.UnionId);
                }
            }

            if (page.RequestMemberList.Any())
            {
                Logger.Verbose(
                    "Refuse {0}.",
                    string.Join(
                        ",",
                        page.RequestMemberList.Select(
                            item => string.Format("({0},{1})", item.UnionName, item.UnionId)).ToArray()));
            }
        }
    }
}