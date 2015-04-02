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

        private string taskHint = "";

        private enum DogAction
        {
            CollectResource,
            UpgradeBuild,
            UpgradeCity,
            Completed
        }

        private int buildId = 0;

        private int heroId = 0;

        private int pid = 0;

        private List<int> requiredResource = null;

        public int TargetBuildId { get; set; }

        public int TargetBuildLevel { get; set; }

        public UpgradeBuildDog(AccountInfo account)
            : base(account, DefaultInterval)
        {
        }

        public override string TaskId
        {
            get { return "Upgrade Build Dog"; }
            set { throw new NotImplementedException(); }
        }

        public override string GetTaskHint()
        {
            return this.taskHint;
        }

        public override void TaskWorker()
        {
            var duration = this.PickOnlineBuildTaskTime();
            if (duration > 0)
            {
                return;
            }

            var dogTask = this.PickValidBuild();
            switch (dogTask)
            {
                case DogAction.CollectResource:
                    this.DoCollectBuildResource(this.requiredResource);
                    break;
                case DogAction.UpgradeBuild:
                    this.DoUpgradeBuild(this.buildId, this.heroId);
                    break;
                case DogAction.UpgradeCity:
                    this.DoUpgradeCity(this.heroId);
                    break;
                case DogAction.Completed:
                    this.IsCompleted = true;
                    break;
            }
        }

        private DogAction PickValidBuild()
        {
            var buildPage = ShowInnerBuildList.Open(this.Account.WebAgent);
            var buildList = buildPage.BuildList.ToList();

            if (buildList.Any(b => b.BuildId == this.TargetBuildId && b.BuildLevel == this.TargetBuildLevel))
            {
                return DogAction.Completed;
            }



            return DogAction.UpgradeBuild;
        }

        private int PickOnlineBuildTaskTime()
        {
            throw new NotImplementedException();
        }

        private bool DoCollectBuildResource(IList<int> requireRes)
        {
            throw new NotImplementedException();
        }

        private bool DoUpgradeBuild(int buildId, int heroId)
        {
            throw new NotImplementedException();
        }

        private bool DoUpgradeCity(int heroId)
        {
            throw new NotImplementedException();
        }
    }
}
