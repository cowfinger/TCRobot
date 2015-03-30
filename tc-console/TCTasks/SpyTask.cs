namespace TC.TCTasks
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    internal class SpyTask : TCTask
    {
        private const int CheckInterval = 2 * 1000;

        private readonly object enemyCityInfoListLock = new object();

        public int Counter = 0;

        private List<CityInfo> enemyCityInfoList = new List<CityInfo>();

        public Dictionary<string, CityMilitaryInfo> EnemyCityList = new Dictionary<string, CityMilitaryInfo>();

        public SpyTask(AccountInfo account)
            : base(account, CheckInterval)
        {
        }

        public List<CityInfo> EnemyCityInfoList
        {
            get
            {
                lock (this.enemyCityInfoListLock)
                {
                    return this.enemyCityInfoList;
                }
            }
            set
            {
                lock (this.enemyCityInfoListLock)
                {
                    this.enemyCityInfoList = value;
                }
            }
        }

        public override string TaskId
        {
            get
            {
                return "spy";
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override string GetTaskHint()
        {
            return "spy";
        }

        public class CityTroopInfo
        {
            public int HeroNum { get; set; }

            public int AttackPower { get; set; }

            public int DefendPower { get; set; }
        }

        public class CityMilitaryInfo
        {
            public string Name { get; set; }

            public int CityId { get; set; }

            public int FortressEndure { get; set; }

            public int MaxFortressEndure { get; set; }

            public int WallEndure { get; set; }

            public int MaxWallEndure { get; set; }

            public int TotalArmy { get; set; }

            public int TotalHeroNum { get; set; }

            public List<CityTroopInfo> AttackTroops { get; set; }

            public List<CityTroopInfo> DefendTroops { get; set; }

            public RequestAgent WebAgent { get; set; }

            public ListViewItem UiItem { get; set; }

            public CityInfo RawData { get; set; }

            public int Counter { get; set; }
        }
    }
}