using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TC.TCPage
{
    class DoGetData : TCPage
    {
        public const string ResPattern =
            @"""hoard"":""(\d+):\d+\|(\d+):\d+\|(\d+):\d+\|(\d+):(\d+)\|\d+""";

        public const string MaxResPattern =
            @"""max_hoard"":""(\d+)\|(\d+)\|(\d+)\|(\d+)""";

        public const string BuildTaskEndTimePattern = @"""endtime"":(\d+)";

        public const string RequestString = "mod=get_data&op=do";

        public int BuildEndTimeHex
        {
            get
            {
                var match = Regex.Match(this.RawPage, BuildTaskEndTimePattern);
                return match.Success ? int.Parse(match.Groups[1].Value) : 0;
            }
        }

        public DateTime BuildEndTime
        {
            get
            {
                var elapse = this.BuildEndTimeHex;
                return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(elapse).ToLocalTime();
            }
        }

        public IList<int> ResourceTabe
        {
            get
            {
                var match = Regex.Match(this.RawPage, ResPattern);
                return !match.Success ? new List<int>() {0, 0, 0, 0} :
                    (from Group i in match.Groups select i.Value).Skip(1).Select(int.Parse).ToList();
            }
        }

        public IList<int> MaxResourceTable
        {
            get
            {
                var match = Regex.Match(this.RawPage, MaxResPattern);
                return !match.Success ? new List<int>() {0, 0, 0, 0} :
                    (from Group i in match.Groups select i.Value).Skip(1).Select(int.Parse).ToList();
            }
        }

        public static DoGetData Open(RequestAgent agent, int re, int task, int taskHint = 1)
        {
            var url = agent.BuildUrl(RequestString);
            var body = string.Format(
                "module=%7B%22re%22%3A%5B{0}%5D%2C%22task%22%3A%5B{1}%2C{2}%5D%7D",
                re, task, taskHint);
            return new DoGetData(agent.WebClient.OpenUrl(url, body));
        }

        protected DoGetData(string page) : base(page)
        {
        }
    }
}
