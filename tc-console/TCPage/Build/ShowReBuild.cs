using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TC.TCPage.Build
{
    class ShowReBuild : TCPage
    {
        public static int FarmPidStart = 1;

        public static int MinePidStart = 13;

        public static int WoodPidStart = 17;

        public static int MudPidStart = 9;

        public static int FarmBid = 1;

        public static int MineBid = 2;

        public static int WoodBid = 3;

        public static int MudBid = 4;

        public class PidBidPair
        {
            public int Pid;

            public int Bid;

            public int Level;
        }

        public static IEnumerable<PidBidPair> PidBidPairs = new List<PidBidPair>()
        {
            new PidBidPair {Pid = FarmPidStart, Bid = FarmBid},
            new PidBidPair {Pid = MinePidStart, Bid = MineBid},
            new PidBidPair {Pid = WoodPidStart, Bid = WoodBid},
            new PidBidPair {Pid = MudPidStart, Bid = MudBid},
        };

        public const string UrlPattern = @"func=build&is_re=1&pid=(\d+)&bt=1&bid=(\d+)&tab_id=";

        public const string BuildLevelPattern = @"等级(\d+)";

        public const string BuildItemPattern = UrlPattern + ".+?" + BuildLevelPattern;


        public IEnumerable<PidBidPair> ItemPidList
        {
            get
            {
                var matches = Regex.Matches(this.RawPage, BuildItemPattern);
                return from Match match in matches
                    select new PidBidPair()
                    {
                        Pid = int.Parse(match.Groups[1].Value),
                        Bid = int.Parse(match.Groups[2].Value),
                        Level = int.Parse(match.Groups[3].Value)
                    };
            }
        }

        public static ShowReBuild Open(RequestAgent agent, int pid, int bid, int bt = 1)
        {
            var url = agent.BuildUrl(
                TCMod.city,
                TCSubMod.build,
                TCOperation.Show,
                TCFunc.re_build,
                new TCRequestArgument(TCElement.pid, pid),
                new TCRequestArgument(TCElement.bid, bid),
                new TCRequestArgument(TCElement.bt, bt));
            return new ShowReBuild(agent.WebClient.OpenUrl(url));
        }

        protected ShowReBuild(string page) : base(page)
        {
        }
    }
}
