namespace TC.TCTasks
{
    using System;
    using System.Collections.Generic;
    using System.Timers;

    internal class InfluenceGuard : TCTask
    {
        private const int CheckInterval = 1 * 1000;

        public List<string> recentRequstUnionList = null;

        public Timer RefuseTimer = null;

        public HashSet<int> UnionIdSet = new HashSet<int>();

        public InfluenceGuard(AccountInfo account)
            : base(account, CheckInterval)
        {
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
            return this.recentRequstUnionList != null ? string.Join(", ", this.recentRequstUnionList.ToArray()) : "";
        }
    }
}