using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TC.TCPage.Build;

namespace TC.TCTasks
{
    class UpgradeBuildDog : TCTask
    {
        public const int DefaultInterval = 5 * 1000;

        public string TargetBuildName { get; set; }

        public int TargetBuildLevel { get; set; }

        public UpgradeBuildDog(AccountInfo account)
            : base(account, DefaultInterval)
        {
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
            var duration = this.PickOnlineBuildTaskTime();
            if (duration > 0)
            {
                return;
            }

            var toBuild = this.PickValidBuild();
            if (toBuild == null)
            {
                return;
            }

            this.DoBuildTask(toBuild);
        }

        private ShowInnerBuildList.Build PickValidBuild()
        {
            throw new NotImplementedException();
        }

        private void DoBuildTask(ShowInnerBuildList.Build build)
        {
            throw new NotImplementedException();
        }

        private int PickOnlineBuildTaskTime()
        {
            throw new NotImplementedException();
        }

        private bool CollectBuildResource(ShowInnerBuildList.Build build)
        {
            throw new NotImplementedException();
        }
    }
}
