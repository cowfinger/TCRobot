using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TC.TCTasks
{
    class SpyTask : TCTask
    {
        private const int CheckInterval = 10 * 1000;

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

        public Dictionary<string, CityMilitaryInfo> EnemyCityList = new Dictionary<string, CityMilitaryInfo>();

        private readonly object enemyCityInfoListLock = new object();
        private List<CityInfo> enemyCityInfoList = new List<CityInfo>();

        public int Counter = 0;

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

        public SpyTask(AccountInfo account)
            : base(account, CheckInterval)
        {
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
    }
}
