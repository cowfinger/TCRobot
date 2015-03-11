namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class DijstraHelper
    {
        private readonly Dictionary<string, HashSet<string>> map;

        private readonly Dictionary<string, int> roadLevelCache = new Dictionary<string, int>();

        private readonly Dictionary<string, int> sNodeMap = new Dictionary<string, int>();

        private readonly List<NodeInfo> sNodeQueue = new List<NodeInfo>();

        private readonly HashSet<string> uNodes = new HashSet<string>();

        public string account = "";

        public Func<string, string, string, Dictionary<string, int>, int> DistanceCalculate = null;

        public DijstraHelper(Dictionary<string, HashSet<string>> map)
        {
            this.map = map;
        }

        public int GetDistance(string from, string to, int speed)
        {
            if (this.DistanceCalculate == null)
            {
                return 1;
            }

            var dist = this.DistanceCalculate(@from, to, this.account, this.roadLevelCache);
            return dist * 24 / speed;
        }

        public IEnumerable<string> GetPath(string from, string to, IEnumerable<Soldier> army)
        {
            if (!this.map.ContainsKey(from) || !this.map.ContainsKey(to))
            {
                yield break;
            }

            var speed = army != null ? army.Min(a => FormMain.SoldierTable[a.SoldierType].Speed) : 24;

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
                if (curNode.Name == to)
                {
                    break;
                }

                var subNodes = this.map[curNode.Name];
                var subNodeGroups = subNodes.GroupBy(name => this.uNodes.Contains(name)).ToList();

                foreach (var group in subNodeGroups)
                {
                    if (!group.Key)
                    {
                        foreach (var subNodeName in group)
                        {
                            var subNodePos = this.sNodeMap[subNodeName];
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

            var lastNodePos = this.sNodeMap[to];
            var iterNode = this.sNodeQueue[lastNodePos];
            while (iterNode.Prev != null)
            {
                yield return iterNode.Name;
                iterNode = iterNode.Prev;
            }
        }

        private class NodeInfo
        {
            public int Distance;

            public string Name = "";

            public NodeInfo Prev;
        }
    }
}