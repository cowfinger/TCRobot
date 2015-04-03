using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TC.TCPage;
using TC.TCPage.Build;
using TC.TCPage.City;

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

        private class BuildPack
        {
            public DogAction Action = DogAction.Completed;

            public int buildId = 0;

            public int buildLevel = 0;

            public List<int> requiredResource = null;
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
            if (this.HasOnlineTask())
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
            var cityPage = ShowUpdateCity.Open(this.Account.WebAgent);
            var dataPage = DoGetData.Open(this.Account.WebAgent, this.Account.Tid, this.Account.Tid);
            var buildPack = new BuildPack()
            {
                Action = DogAction.UpgradeBuild,
                buildId = this.TargetBuildId,
                buildLevel = this.TargetBuildLevel
            };

            var result = this.GetDepBuildPack(buildPack, buildPage, cityPage, dataPage);
            if (result == null)
            {
                return DogAction.Completed;
            }

            switch (result.Action)
            {
                case DogAction.CollectResource:
                    this.requiredResource = result.requiredResource;
                    break;
                case DogAction.UpgradeBuild:
                    this.buildId = result.buildId;
                    var showBuildPage = ShowBuild.Open(this.Account.WebAgent, 0, 0, this.buildId);
                    var heroList = showBuildPage.HeroBuildTimes.ToList();
                    heroList.Sort((x, y) => y.BuildTimeInSeconds.CompareTo(x));
                    this.heroId = heroList.First().HeroId;
                    break;
                case DogAction.UpgradeCity:
                    this.heroId = cityPage.EfficientHeroId;
                    break;
            }
            return result.Action;
        }

        private BuildPack GetDepBuildPack(
            BuildPack pack,
            ShowInnerBuildList buildPage,
            ShowUpdateCity cityPage,
            DoGetData dataPage)
        {
            switch (pack.Action)
            {
                case DogAction.UpgradeBuild:
                    var buildList = buildPage.BuildList.ToList();

                    var targetBuilds = buildList.Where(b => b.BuildId == pack.buildId).ToList();
                    if (targetBuilds.Any())
                    {
                        var targetBuild = targetBuilds.First();
                        if (targetBuild.BuildLevel <= pack.buildId)
                        {
                            return null;
                        }

                        var resPack = new BuildPack
                        {
                            Action = DogAction.CollectResource,
                            requiredResource = targetBuild.RequiredResourceTable.ToList()
                        };

                        var resPackResult = this.GetDepBuildPack(resPack, buildPage, cityPage, dataPage);
                        return resPackResult ?? pack;
                    }

                    var disabledBuilds = buildPage.DisabledBuildList.Where(b => b.BuildId == pack.buildId).ToList();
                    if (disabledBuilds.Any())
                    {
                        var targetBuild = disabledBuilds.First();
                        foreach (var preBuild in targetBuild.PreBuilds)
                        {
                            var preBuildPack = new BuildPack()
                            {
                                Action = DogAction.UpgradeBuild,
                                buildId = preBuild.PreBuildId,
                                buildLevel = preBuild.PreBuildLevel
                            };

                            var preBuildPackResult = this.GetDepBuildPack(preBuildPack, buildPage, cityPage, dataPage);
                            if (preBuildPackResult != null)
                            {
                                return preBuildPackResult;
                            }
                        }

                        var cityPack = new BuildPack
                        {
                            Action = DogAction.UpgradeCity
                        };
                        return this.GetDepBuildPack(cityPack, buildPage, cityPage, dataPage);
                    }

                    break;

                case DogAction.UpgradeCity:
                    var depPackBuild = new BuildPack
                    {
                        Action = DogAction.UpgradeBuild,
                        buildId = cityPage.RequiredBuildId,
                        buildLevel = cityPage.RequiredBuildLevel
                    };

                    var depPackBuildResult = this.GetDepBuildPack(depPackBuild, buildPage, cityPage, dataPage);
                    if (depPackBuildResult != null)
                    {
                        return depPackBuildResult;
                    }

                    var userInfoPage = TCPage.UserInfo.ShowUserSurvey.Open(this.Account.WebAgent);
                    if (cityPage.RequiredCreditPoint < userInfoPage.CreditPoint)
                    {
                        foreach (var build in buildPage.BuildList)
                        {
                            var depPackBuild1 = new BuildPack
                            {
                                Action = DogAction.UpgradeBuild,
                                buildId = build.BuildId,
                                buildLevel = build.BuildLevel + 1
                            };

                            var depPackBuildResult1 = this.GetDepBuildPack(depPackBuild1, buildPage, cityPage, dataPage);
                            if (depPackBuildResult1 != null)
                            {
                                return depPackBuildResult1;
                            }
                        }
                    }

                    var depPackResource = new BuildPack
                    {
                        Action = DogAction.CollectResource,
                        requiredResource = cityPage.RequiredResourceTable.ToList()
                    };

                    var depPackResourceResult = this.GetDepBuildPack(depPackResource, buildPage, cityPage, dataPage);
                    return depPackResourceResult ?? pack;

                case DogAction.CollectResource:
                    if (this.CompareResourceTable(pack.requiredResource, dataPage.ResourceTabe))
                    {
                        return null;
                    }

                    if (this.CompareResourceTable(pack.requiredResource, dataPage.MaxResourceTable))
                    {
                        return pack;
                    }

                    var depPack = new BuildPack()
                    {
                        Action = DogAction.UpgradeCity,
                    };

                    var depPackResult = this.GetDepBuildPack(depPack, buildPage, cityPage, dataPage);
                    return depPackResult ?? depPack;
            }

            return null;
        }

        private bool CompareResourceTable(IList<int> requiredRes, IList<int> resBox)
        {
            var resCheck = requiredRes.Zip(resBox, (x, y) => y - x);
            return !resCheck.Any(r => r < 0);
        }

        private bool HasOnlineTask()
        {
            var dataPage = DoGetData.Open(this.Account.WebAgent, this.Account.Tid, this.Account.Tid);
            return dataPage.BuildEndTime > FormMain.RemoteTime;
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
