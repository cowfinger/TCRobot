using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCTasks
{
    class InfluenceGuard : TCTask
    {
        private const int CheckInterval = 3 * 1000;

        public List<string> recentRequstUnionList = null; 

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
            return this.recentRequstUnionList != null ? 
                string.Join(", ", this.recentRequstUnionList.ToArray()) : "";
        }
    }
}
