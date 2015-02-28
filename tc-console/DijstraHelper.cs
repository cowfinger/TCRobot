using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC
{
    class DijstraHelper
    {
        private class NodeInfo
        {
            public string Name = "";
            public NodeInfo Prev = null;
            public int Distance = 0;
        }

        private Dictionary<string, HashSet<string>> map = null;

        private List<NodeInfo> sNodeQueue = new List<NodeInfo>();
        private Dictionary<string, int> sNodeMap = new Dictionary<string, int>();
        private HashSet<string> uNodes = new HashSet<string>();

        public DijstraHelper(Dictionary<string, HashSet<string>> map)
        {
            this.map = map;
        }

        public int GetDistance(string from, string to)
        {
            return 1;
        }

        public IEnumerable<string> GetPath(string from, string to)
        {
            if (!map.ContainsKey(from) || !map.ContainsKey(to))
            {
                yield break;
            }

            foreach (var item in this.map.Keys)
            {
                this.uNodes.Add(item);
            }

            this.sNodeQueue.Add(new NodeInfo() { Name = from, Distance = 0, Prev = null, });
            this.sNodeMap.Add(from, this.sNodeQueue.Count - 1);
            this.uNodes.Remove(from);

            for (int cursor = 0; cursor < this.sNodeQueue.Count(); ++cursor)
            {
                var curNode = this.sNodeQueue[cursor];
                var subNodes = this.map[curNode.Name];
                var subNodeGroups = subNodes.GroupBy(name => this.uNodes.Contains(name)).ToList();

                foreach (var group in subNodeGroups)
                {
                    if (!group.Key)
                    {
                        foreach (var subNodeName in group)
                        {
                            int subNodePos = this.sNodeMap[subNodeName];
                            if (subNodePos > cursor)
                            {
                                int distance0 = curNode.Distance + GetDistance(curNode.Name, subNodeName);
                                int distance1 = curNode.Distance + GetDistance(subNodeName, curNode.Name);

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
                    }
                    else
                    {
                        foreach (var subNodeName in group)
                        {
                            int distance = curNode.Distance + GetDistance(curNode.Name, subNodeName);
                            this.sNodeQueue.Add(new NodeInfo() { Name = subNodeName, Distance = distance, Prev = curNode, });
                            this.sNodeMap.Add(subNodeName, this.sNodeQueue.Count - 1);
                            this.uNodes.Remove(subNodeName);
                        }
                    }
                }
            }

            int lastNodePos = this.sNodeMap[to];
            var iterNode = this.sNodeQueue[lastNodePos];
            while (iterNode.Prev != null)
            {
                yield return iterNode.Name;
                iterNode = iterNode.Prev;
            }
        }
    }
}
