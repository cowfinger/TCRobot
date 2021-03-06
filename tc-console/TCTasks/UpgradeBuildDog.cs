﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TC.TCPage;
using TC.TCPage.Build;
using TC.TCPage.City;
using TC.TCPage.UserInfo;

namespace TC.TCTasks
{
    using TC.TCDataType;
    using TC.TCPage.Depot;
    using TC.TCPage.Prop;
    using TC.TCUtility;

    class UpgradeBuildDog : TCTask
    {

        public const int DefaultInterval = 5 * 1000;

        private string taskHint = "";

        private readonly List<int> restPidList = new List<int>() { 15, 83, 79, 491, 555 };

        private enum DogAction
        {
            CollectResource,
            UpgradeBuild,
            UpgradeCity,
            Completed,
            Failed
        }

        private class BuildPack
        {
            public BuildPack PrevPack;

            public DogAction Action = DogAction.Completed;

            public int BuildId;

            public int BuildLevel;

            public List<int> RequiredResource;

            public ShowInnerBuildList.Build ActualBuild;
        }

        private int buildId;

        private int buildLevel;

        private int pid;

        private int heroId;

        private List<int> requiredResource;

        private bool buildingInProgress;

        private int reorganizingElapse;

        private int recruitCoolDown;

        private int hireHeroCoolDown;

        private int currentTargetCursor = 0;

        private int scienceTargetCursor = 0;

        private List<CityScienceId> scienceMilestones = new List<CityScienceId>
                                        {
                                            new CityScienceId(1, 1),
                                            new CityScienceId(1, 1),
                                            new CityScienceId(1, 1),
                                            new CityScienceId(1, 1),
                                            new CityScienceId(1, 1),
                                            new CityScienceId(1, 5),
                                            new CityScienceId(1, 5),
                                            new CityScienceId(1, 5),
                                            new CityScienceId(1, 5),
                                            new CityScienceId(1, 5),
                                            new CityScienceId(3, 46),
                                            new CityScienceId(3, 46),
                                            new CityScienceId(3, 46),
                                            new CityScienceId(3, 46),
                                            new CityScienceId(3, 46),
                                            new CityScienceId(3, 46),
                                        };

        private readonly List<BuildPack> mileStones = new List<BuildPack>();

        public int TargetBuildId
        {
            get
            {
                if (this.mileStones.Count <= this.currentTargetCursor)
                {
                    return -1;
                }
                else
                {
                    return this.mileStones[this.currentTargetCursor].BuildId;
                }
            }
        }

        public int TargetBuildLevel
        {
            get
            {
                if (this.mileStones.Count <= this.currentTargetCursor)
                {
                    return -1;
                }
                else
                {
                    return this.mileStones[this.currentTargetCursor].BuildLevel;
                }
            }
        }

        private IEnumerable<ShowInnerBuildList.Build> outBuildsCache;

        private ShowInnerBuildList buildPageCache;

        private ShowUpdateCity cityPageCache;

        private IEnumerable<ShowInnerBuildList.Build> OutBuilds
        {
            get
            {
                if (this.outBuildsCache == null)
                {
                    var pidBidList = ShowReBuild.PidBidPairs.SelectMany(
                        item =>
                        {
                            var page = ShowReBuild.Open(this.Account.WebAgent, item.Pid, item.Bid);
                            return page.ItemPidList;
                        });

                    this.outBuildsCache = pidBidList.Select(
                        item =>
                        {
                            var page = ShowBuild.Open(this.Account.WebAgent, 1, item.Pid, 1, item.Bid, 1, 0);
                            return new ShowInnerBuildList.Build()
                            {
                                Pid = item.Pid,
                                Bt = 1,
                                BuildId = item.Bid,
                                BuildLevel = item.Level,
                                UpgradeRequiredFood = page.BuildDetail.UpgradeRequiredFood,
                                UpgradeRequiredWood = page.BuildDetail.UpgradeRequiredWood,
                                UpgradeRequiredIron = page.BuildDetail.UpgradeRequiredIron,
                                UpgradeRequiredMud = page.BuildDetail.UpgradeRequiredMud,
                            };
                        }).ToList();
                }
                return this.outBuildsCache;
            }
        }

        private ShowInnerBuildList BuildPage
        {
            get
            {
                if (this.buildPageCache == null)
                {
                    this.buildPageCache = ShowInnerBuildList.Open(this.Account.WebAgent);
                }
                return this.buildPageCache;
            }
        }

        private ShowUpdateCity CityPage
        {
            get
            {
                if (this.cityPageCache == null)
                {
                    this.cityPageCache = ShowUpdateCity.Open(this.Account.WebAgent, this.Account.Tid);
                }
                return this.cityPageCache;
            }
        }

        private DoGetData DataPage
        {
            get
            {
                return DoGetData.Open(this.Account.WebAgent, this.Account.Tid, this.Account.Tid);
            }
        }

        public UpgradeBuildDog(AccountInfo account)
            : base(account, DefaultInterval)
        {
        }

        public override string TaskId
        {
            get { return "UpgradeBuildDog"; }
            set { throw new NotImplementedException(); }
        }

        public void AddMileStone(int buildId, int buildLevel)
        {
            this.mileStones.Add(new BuildPack()
            {
                Action = DogAction.UpgradeBuild,
                BuildId = buildId,
                BuildLevel = buildLevel
            });
        }

        // private void Verbose(string format, params object[] args)
        // {
        //     var fmt = string.Format("BuildDog[{0}]:{1}", this.Account.UserName, format);
        //     var log = string.Format(fmt, args);
        //     this.taskHint = log;
        //     Logger.Verbose(log);
        // }

        public override string GetTaskHint()
        {
            return this.taskHint;
        }

        public override void TaskWorker()
        {
            this.HandleHeroEvent();

            this.HandleScienceEvent();

            this.HandleReorganizeEvent();

            this.HandleBuildEvent();
        }

        private void HandleHeroEvent()
        {
            if (this.hireHeroCoolDown > 0)
            {
                this.hireHeroCoolDown -= 5;
                return;
            }

            var pubHeroPage = TCPage.Pub.ShowPubHeros.Open(this.Account.WebAgent);
            this.Verbose("Start Check Hero: Cool Down={0}", pubHeroPage.RefreshCoolDown);
            this.hireHeroCoolDown = pubHeroPage.RefreshCoolDown;
            var valuableHeroes = pubHeroPage.Heroes.Where(h => h.Slot >= 2 && h.IInc >= 5).ToList();
            valuableHeroes.Sort((x, y) => y.IInc.CompareTo(y.IInc));
            foreach (var hero in valuableHeroes)
            {
                var hirePage = TCPage.Pub.DoEmployHero.Open(this.Account.WebAgent, hero.Sort);
                this.Verbose("Employ Hero:{0}", hirePage.Success);
            }
        }

        private int CalcTargetScienceId()
        {
            if (this.scienceTargetCursor >= this.scienceMilestones.Count)
            {
                return 0;
            }

            var scienceId = this.scienceMilestones[this.scienceTargetCursor];
            this.scienceTargetCursor++;

            var sciencePage = TCPage.Science.ShowScienceType.Open(this.Account.WebAgent, scienceId.ScienceType);
            if (!sciencePage.ScienceList.Any())
            {
                return 0;
            }

            var scienceInfoes = sciencePage.ScienceList.Where(
                s => s.ScienceTypeId == scienceId.ScienceId).ToList();
            if (!scienceInfoes.Any())
            {
                return 0;
            }

            return scienceId.ScienceId;
        }

        private void HandleScienceEvent()
        {
            if (this.recruitCoolDown > 0)
            {
                this.recruitCoolDown -= 5;
                return;
            }

            var showRecruitPage = TCPage.Science.ShowRecruit.Open(this.Account.WebAgent);
            this.recruitCoolDown = showRecruitPage.RecruitCoolDown;

            // for (var i = 0; i < showRecruitPage.IdleCount; i++)
            // {
            //     while (true)
            //     {
            //         var targetScience = this.CalcTargetScienceId();
            //         if (targetScience == 0)
            //         {
            //             break;
            //         }

            //         var upPage = TCPage.Science.ShowUp.Open(this.Account.WebAgent, targetScience);
            //         if (upPage.HeroId == 0)
            //         {
            //             continue;
            //         }

            //         var doUpPage = TCPage.Science.DoUp.Open(this.Account.WebAgent, targetScience, upPage.HeroId);
            //         this.Verbose("Up Science {0} : {1}", targetScience, doUpPage.RawPage);
            //         if (doUpPage.Success)
            //         {
            //             break;
            //         }
            //     }
            // }

            if (!showRecruitPage.CanRecruit)
            {
                return;
            }

            var doDataPage = DoGetData.Open(this.Account.WebAgent, this.Account.Tid, this.Account.Tid);
            if (!CompareResourceTable(
                showRecruitPage.RequiredResourceTable,
                doDataPage.ResourceTabe
                ))
            {
                this.DoCollectBuildResource(showRecruitPage.RequiredResourceTable);
            }

            var doRecruitPage = TCPage.Science.DoRecruit.Open(this.Account.WebAgent);
            this.Verbose("Recruit:{0}", doRecruitPage.RawPage);
            if (doRecruitPage.RawPage.Contains("请先"))
            {
                this.recruitCoolDown = 30 * 60;
            }
        }

        private void HandleReorganizeEvent()
        {
            if (this.reorganizingElapse > 0)
            {
                this.reorganizingElapse -= 5;
                return;
            }

            var ordinancePage = TCPage.Politics.ShowOrdinance.Open(this.Account.WebAgent);
            if (!ordinancePage.CanReOrg)
            {
                return;
            }

            var reOrgHeroId = TCPage.Politics.ShowReorganize.Open(this.Account.WebAgent).FirstHeroId;
            if (reOrgHeroId == 0)
            {
                return;
            }

            var doPage = TCPage.Politics.DoReorganize.Open(this.Account.WebAgent, reOrgHeroId);
            if (!doPage.Success)
            {
                this.Verbose("ReOrg:Hero={0} failed:{1}", reOrgHeroId, doPage.RawPage);
                return;
            }

            this.Verbose("ReOrg:Hero={0} Success", reOrgHeroId);
            this.reorganizingElapse = 3600;
        }

        private void HandleBuildEvent()
        {
            // Clear cache.
            this.cityPageCache = null;
            this.outBuildsCache = null;
            this.buildPageCache = null;

            if (this.DataPage.BuildEndTimeHex > 0)
            {
                if (this.buildingInProgress) return;
                this.Verbose("Wait {0}", this.DataPage.BuildEndTime);
                this.buildingInProgress = true;
                return;
            }

            this.buildingInProgress = false;

            var dogTask = this.PickValidBuild();
            switch (dogTask)
            {
                case DogAction.CollectResource:
                    this.Verbose("{0} Start:{1}/{2}",
                        dogTask,
                        string.Join("|", this.requiredResource),
                        string.Join("|", this.DataPage.ResourceTabe));
                    if (!this.DoCollectBuildResource(this.requiredResource))
                    {
                        this.Verbose("Canceled Since No Resource.");
                        this.IsCompleted = true;
                    }
                    break;
                case DogAction.UpgradeBuild:
                    this.Verbose("{0}:{1}({2},{3})=>{4}",
                        dogTask,
                        FormMain.KeyWordMap["build_" + this.buildId],
                        this.buildId,
                        this.pid,
                        this.buildLevel);
                    if (!this.DoUpgradeBuild(this.buildId, this.pid, this.heroId))
                    {
                        this.Verbose("Canceled Since Upgrade Build Error.");
                        this.IsCompleted = true;
                    }
                    break;
                case DogAction.UpgradeCity:
                    this.Verbose("{0}", dogTask);
                    if (!this.DoUpgradeCity(this.heroId, this.CityPage.CurrentLevel, this.Account.Tid))
                    {
                        this.Verbose("Canceled Since Upgrade City Error.");
                        this.IsCompleted = true;
                    }
                    break;
                case DogAction.Completed:
                    this.Verbose("Completed Milestone {0}:build={1}({2}),level={3}",
                        this.currentTargetCursor,
                        FormMain.KeyWordMap["build_" + this.TargetBuildId.ToString()],
                        this.TargetBuildId,
                        this.TargetBuildLevel);

                    ++this.currentTargetCursor;
                    if (this.TargetBuildId == -1)
                    {
                        this.Stop();
                        this.Verbose("Completed All Milestones");
                    }
                    break;
            }
        }

        private DogAction PickValidBuild()
        {
            var cityUpdatePage = ShowUpdateCity.Open(this.Account.WebAgent, this.Account.Tid);
            var userInfoPage = ShowUserSurvey.Open(this.Account.WebAgent);

            this.Verbose("Resource:{0}, CreditPoint:{1}",
                string.Join("|", this.DataPage.ResourceTabe),
                userInfoPage.CreditPoint);
            var buildPack = new BuildPack()
            {
                Action = DogAction.UpgradeBuild,
                BuildId = this.TargetBuildId,
                BuildLevel = this.TargetBuildLevel
            };

            var result = this.GetDepBuildPack(buildPack);
            if (result == null)
            {
                return DogAction.Completed;
            }

            switch (result.Action)
            {
                case DogAction.CollectResource:
                    this.requiredResource = result.RequiredResource;
                    break;

                case DogAction.UpgradeBuild:
                    this.buildId = result.BuildId;
                    this.buildLevel = result.BuildLevel;
                    this.pid = result.ActualBuild.Pid;

                    var showBuildPage = ShowBuild.Open(
                        this.Account.WebAgent,
                        result.ActualBuild.Pid == 0 ? 1 : result.ActualBuild.Pid,
                        result.ActualBuild.Bt,
                        result.ActualBuild.BuildId);
                    var heroList = showBuildPage.HeroBuildTimes.ToList();
                    heroList.Sort((x, y) => y.BuildTimeInSeconds.CompareTo(x.BuildTimeInSeconds));
                    if (!heroList.Any())
                    {
                        this.Verbose("Cannot find hero.");
                        return DogAction.Failed;
                    }
                    this.heroId = heroList.First().HeroId;
                    break;

                case DogAction.UpgradeCity:
                    this.heroId = cityUpdatePage.EfficientHeroId;
                    break;
            }
            return result.Action;
        }

        private static bool IsLoopedPack(BuildPack pack)
        {
            var iterPack = pack.PrevPack;
            while (iterPack != null)
            {
                if (iterPack.Action != pack.Action)
                {
                    iterPack = iterPack.PrevPack;
                    continue;
                }

                switch (iterPack.Action)
                {
                    case DogAction.UpgradeCity:
                        return true;
                    case DogAction.CollectResource:
                        if (CompareResourceTable(pack.RequiredResource, iterPack.RequiredResource))
                        {
                            return true;
                        }
                        break;
                    case DogAction.UpgradeBuild:
                        if (iterPack.BuildId == pack.BuildId && iterPack.BuildLevel == pack.BuildId)
                        {
                            return true;
                        }
                        break;
                }
                iterPack = iterPack.PrevPack;
            }

            return false;
        }

        private BuildPack GetDepBuildPack(BuildPack pack)
        {
            if (IsLoopedPack(pack))
            {
                return null;
            }

            switch (pack.Action)
            {
                case DogAction.UpgradeBuild:
                    var buildList = this.BuildPage.BuildList.Concat(this.OutBuilds);

                    var targetBuilds = buildList.Where(b => b.BuildId == pack.BuildId).ToList();
                    targetBuilds.Sort((x, y) => x.BuildLevel.CompareTo(y.BuildLevel));
                    if (targetBuilds.Any())
                    {
                        var targetBuild = targetBuilds.First();
                        if (targetBuild.BuildLevel >= pack.BuildLevel)
                        {
                            return new BuildPack() { Action = DogAction.Completed };
                        }

                        var resPack = new BuildPack
                        {
                            Action = DogAction.CollectResource,
                            RequiredResource = targetBuild.RequiredResourceTable.ToList(),
                            PrevPack = pack
                        };

                        var resPackResult = this.GetDepBuildPack(resPack);
                        if (resPackResult == null)
                        {
                            return null;
                        }
                        if (resPackResult.Action != DogAction.Completed)
                        {
                            return resPackResult;
                        }
                        pack.ActualBuild = targetBuild;
                        return pack;
                    }

                    var disabledBuilds = this.BuildPage.DisabledBuildList.Where(
                        b => b.BuildId == pack.BuildId).ToList();
                    if (disabledBuilds.Any())
                    {
                        var targetBuild = disabledBuilds.First();
                        var completedBuildNum = 0;
                        foreach (var preBuild in targetBuild.PreBuilds)
                        {
                            var preBuildPack = new BuildPack()
                            {
                                Action = DogAction.UpgradeBuild,
                                BuildId = preBuild.PreBuildId,
                                BuildLevel = preBuild.PreBuildLevel,
                                PrevPack = pack
                            };

                            var preBuildPackResult = this.GetDepBuildPack(preBuildPack);
                            if (preBuildPackResult != null)
                            {
                                if (preBuildPackResult.Action == DogAction.Completed)
                                {
                                    ++completedBuildNum;
                                }
                                else
                                {
                                    return preBuildPackResult;
                                }
                            }
                        }

                        var cityPack = new BuildPack
                        {
                            Action = DogAction.UpgradeCity,
                            PrevPack = pack
                        };
                        return this.GetDepBuildPack(cityPack);
                    }

                    return null;

                case DogAction.UpgradeCity:
                    var depPackBuild = new BuildPack
                    {
                        Action = DogAction.UpgradeBuild,
                        BuildId = this.CityPage.RequiredBuildId,
                        BuildLevel = this.CityPage.RequiredBuildLevel,
                        PrevPack = pack
                    };

                    var depPackBuildResult = this.GetDepBuildPack(depPackBuild);
                    if (depPackBuildResult != null)
                    {
                        if (depPackBuildResult.Action != DogAction.Completed)
                        {
                            return depPackBuildResult;
                        }
                    }

                    var userInfoPage = ShowUserSurvey.Open(this.Account.WebAgent);
                    if (this.CityPage.RequiredCreditPoint > userInfoPage.CreditPoint)
                    {
                        var candidateBuildList = this.BuildPage.BuildList.Concat(this.OutBuilds).ToList();
                        candidateBuildList.Sort((x, y) => x.BuildLevel.CompareTo(y.BuildLevel));
                        foreach (var build in candidateBuildList)
                        {
                            if (build.BuildId == 17)
                            {
                                continue;
                            }

                            var depPackPreBuild = new BuildPack
                            {
                                Action = DogAction.UpgradeBuild,
                                BuildId = build.BuildId,
                                BuildLevel = build.BuildLevel + 1,
                                PrevPack = pack
                            };

                            var depPackPreBuildResult = this.GetDepBuildPack(depPackPreBuild);
                            if (depPackPreBuildResult != null)
                            {
                                return depPackPreBuildResult;
                            }
                        }
                    }

                    var depPackResource = new BuildPack
                    {
                        Action = DogAction.CollectResource,
                        RequiredResource = this.CityPage.RequiredResourceTable.ToList(),
                        PrevPack = pack
                    };

                    var depResRet = this.GetDepBuildPack(depPackResource);
                    if (depResRet == null)
                    {
                        return null;
                    }

                    return depResRet.Action == DogAction.Completed ? pack : depPackResource;

                case DogAction.CollectResource:
                    if (CompareResourceTable(pack.RequiredResource, this.DataPage.ResourceTabe))
                    {
                        return new BuildPack { Action = DogAction.Completed };
                    }

                    if (CompareResourceTable(pack.RequiredResource, this.DataPage.MaxResourceTable))
                    {
                        return pack;
                    }

                    var depPack = new BuildPack
                    {
                        Action = DogAction.UpgradeCity,
                        PrevPack = pack
                    };

                    return this.GetDepBuildPack(depPack);
            }

            return pack;
        }

        private static bool CompareResourceTable(
            IEnumerable<int> requiredRes,
            IEnumerable<int> haveRes)
        {
            var resCheck = requiredRes.Zip(haveRes, (x, y) => y - x);
            return !resCheck.Any(r => r < 0);
        }

        private bool DoCollectBuildResource(IList<int> requireRes)
        {
            DoBrick.Open(this.Account.WebAgent, -100);
            return true;
            // var count = 0;
            // var tempData = DoGetData.Open(this.Account.WebAgent, this.Account.Tid, this.Account.Tid);
            // while (!CompareResourceTable(requireRes, tempData.ResourceTabe))
            // {
            //     ++count;
            //     if (count > 100)
            //     {
            //         return false;
            //     }

            //     if (!OpenResourceBox(this.Account))
            //     {
            //         return false;
            //     }

            //     tempData = DoGetData.Open(this.Account.WebAgent, this.Account.Tid, this.Account.Tid);
            // }

            // return true;
        }

        public static bool OpenResourceBox(AccountInfo account)
        {
            var resBoxes =
                ShowMyDepot.EnumDepotItems(account.WebAgent)
                    .Where(prop => prop.PropertyId == (int)DepotItem.PropId.ResourceBox)
                    .ToList();
            if (!resBoxes.Any())
            {
                return false;
            }
            var firstBox = resBoxes.First();
            DoUseProp.Open(account.WebAgent, firstBox.PropertyId, firstBox.UserPropertyId, 1);
            return true;
        }

        private bool DoUpgradeBuild(int buildId, int pid, int heroId, bool isRetry = false)
        {
            if (pid != 0)
            {
                var page = DoBuild.Open(this.Account.WebAgent, pid, buildId, heroId);

                if (this.DataPage.BuildEndTimeHex == 0)
                {
                    if (!isRetry)
                    {
                        this.Verbose("UpgradeBuild Error:{0}", page.RawPage);
                    }
                    return false;
                }
                return true;
            }
            else
            {
                foreach (var restPid in this.restPidList)
                {
                    if (this.DoUpgradeBuild(buildId, restPid, heroId, true))
                    {
                        return true;
                    }
                }

                this.Verbose("UpgradeBuild Error");
                return false;
            }
        }

        private bool DoUpgradeCity(int heroId, int nowLevel, int cityId)
        {
            return DoUpdateLevel.Open(this.Account.WebAgent, nowLevel, heroId, cityId).Success;
        }
    }
}
