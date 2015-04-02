using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace TC.TCPage
{
    class DoGetData : TCPage
    {
        public const string ResPattern = 
            @"""hoard"":" +
            @"""(?\d+):\d+\|(?\d+):\d\|(?\d+):\d+\|(?\d+):\d+\|\d+""";

        public const string MaxResPattern = @"""max_hoard"":""(\d+)\|(\d+)\|(\d+)\|(\d+)""";

        public const string BuildTaskEndTimePattern = @"""endtime"":(\d+)";

        public const string RequestString = "mod=get_data&op=do";

        public DateTime BuildEndTime
        {
            get
            {
                var match = Regex.Match(this.RawPage, BuildTaskEndTimePattern);
                var elapse = match.Success ? int.Parse(match.Groups[1].Value) : 0;
                return DateTime.FromFileTimeUtc(elapse);
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
