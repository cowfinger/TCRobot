using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCTasks
{
    class UpgradeBuildDog : TCTask
    {
        public const int DefaultInterval = 5*1000;

        private enum TaskStatus
        {
            Initialized,
            WaitBuild
        }

        private TaskStatus Status { get; set; }

        public UpgradeBuildDog(AccountInfo account)
            : base(account, DefaultInterval)
        {
            this.Status = TaskStatus.Initialized;
        }

        public override string TaskId
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override string GetTaskHint()
        {
            throw new NotImplementedException();
        }

        public override void TaskWorker()
        {
            switch (this.Status)
            {
                case TaskStatus.Initialized:

                    break;
                case TaskStatus.WaitBuild:
                    break;
            }
        }
    }
}
