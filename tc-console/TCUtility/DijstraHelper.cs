namespace TC
{
    using System.Collections.Generic;
    using System.Linq;

    using TC.TCPage.Influence;

    internal class DijstraHelper
    {

        private readonly Dictionary<string, HashSet<string>> map;

        private readonly Dictionary<string, int> roadLevelCache = new Dictionary<string, int>();

        private readonly Dictionary<string, int> sNodeMap = new Dictionary<string, int>();

        private readonly List<NodeInfo> sNodeQueue = new List<NodeInfo>();

        private readonly HashSet<string> uNodes = new HashSet<string>();

        public DijstraHelper(Dictionary<string, HashSet<string>> map)
        {
            this.map = map;
        }

        public AccountInfo Account { get; set; }

        public int GetDistance(string from, string to, int speed)
        {
            var dist = CalculateDistance(from, to, this.Account, this.roadLevelCache);
            return dist * 24 / speed;
        }

        private static int CalculateDistance(
            string from,
            string to,
            AccountInfo accountInfo,
            IDictionary<string, int> roadLevelCache)
        {
            string fromCityIdValue;
            if (!FormMain.CityList.TryGetValue(from, out fromCityIdValue))
            {
                return 5;
            }
            var fromCityId = int.Parse(fromCityIdValue);

            string toCityIdValue;
            if (!FormMain.CityList.TryGetValue(to, out toCityIdValue))
            {
                var destVec = accountInfo.InfluenceMap[to];
                return destVec.Contains(from) ? 5 : 120;
            }

            int roadLevel;
            if (!roadLevelCache.TryGetValue(from, out roadLevel))
            {
                ShowInfluenceCityDetail.Open(accountInfo.WebAgent, fromCityId);
                var page = ShowCityBuild.Open(accountInfo.WebAgent, fromCityId);

                if (page.Road != null)
                {
                    roadLevelCache.Add(from, page.Road.Level);
                    roadLevel = page.Road.Level;
                }
                else
                {
                    return 200;
                }
            }

            return FormMain.RoadLevelToDistanceMap[roadLevel];
        }

        public IEnumerable<NodeInfo> GetPath(string from, string to, IEnumerable<Soldier> army)
        {
            if (!this.map.ContainsKey(from) || !this.map.ContainsKey(to))
            {
                yield break;
            }

            var speed = army != null && army.Any() ? army.Min(a => FormMain.SoldierTable[a.SoldierType].Speed) : 24;

            foreach (var item in this.map.Keys)
            {
                this.uNodes.Add(item);
            }

            this.sNodeQueue.Add(new NodeInfo { Name = from, Distance = 0, Prev = null });
            this.sNodeMap.Add(from, this.sNodeQueue.Count - 1);
            this.uNodes.Remove(from);

            for (var cursor = 0; cursor < this.sNodeQueue.Count(); ++cursor)
            {
                var curNode = this.sNodeQueue[cursor];
                var subNodes = this.map[curNode.Name].ToList();

                if (!FormMain.CityList.ContainsKey(to))
                {
                    subNodes.Add(to);
                }

                var subNodeGroups = subNodes.GroupBy(name => this.uNodes.Contains(name)).ToList();

                foreach (var group in subNodeGroups)
                {
                    if (!group.Key)
                    {
                        foreach (var subNodeName in group)
                        {
                            int subNodePos;
                            if (!this.sNodeMap.TryGetValue(subNodeName, out subNodePos))
                            {
                                continue;
                            }

                            if (subNodePos <= cursor)
                            {
                                continue;
                            }
                            var distance0 = curNode.Distance + this.GetDistance(curNode.Name, subNodeName, speed);
                            var distance1 = curNode.Distance + this.GetDistance(subNodeName, curNode.Name, speed);

                            var subNode = this.sNodeQueue[subNodePos];
                            if (subNode.Distance > distance0)
                            {
                                subNode.Distance = distance0;
                                subNode.Prev = curNode;
                            }

                            if (curNode.Distance > distance1)
                            {
                                curNode.Distance = distance1;
                                curNode.Prev = subNode;
                            }
                        }
                    }
                    else
                    {
                        foreach (var subNodeName in group)
                        {
                            var distance = curNode.Distance + this.GetDistance(curNode.Name, subNodeName, speed);
                            this.sNodeQueue.Add(
                                new NodeInfo { Name = subNodeName, Distance = distance, Prev = curNode });
                            this.sNodeMap.Add(subNodeName, this.sNodeQueue.Count - 1);
                            this.uNodes.Remove(subNodeName);
                        }
                    }
                }
            }

            if (!this.sNodeMap.ContainsKey(to))
            {
                yield break;
            }

            var lastNodePos = this.sNodeMap[to];
            var iterNode = this.sNodeQueue[lastNodePos];
            while (iterNode.Prev != null)
            {
                yield return iterNode;
                iterNode = iterNode.Prev;
            }
        }

        public class NodeInfo
        {
            public int Distance;

            public string Name = "";

            public NodeInfo Prev;
        }
    }
}