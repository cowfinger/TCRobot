using System.Text.RegularExpressions;

namespace TC.TCPage.Build
{
    internal class DoBuild
    {
        public const string GetRePattern = @"get_re\((\d+)\)";

        public const string GetTaskPattern = @"get_task\((?<task>\d+), (?<taskHint>\d+)\)";

        public DoBuild(string page)
        {
            this.RawPage = page;
        }

        public string RawPage { get; private set; }

        public bool Success
        {
            get
            {
                return this.RawPage.StartsWith(">[[jslang(\"endure\")]]:");
            }
        }

        public int Re
        {
            get
            {
                var match = Regex.Match(this.RawPage, GetRePattern);
                return match.Success ? int.Parse(match.Groups[1].Value) : 0;
            }
        }

        public int Task
        {
            get
            {
                var match = Regex.Match(this.RawPage, GetTaskPattern);
                return match.Success ? int.Parse(match.Groups["task"].Value) : 0;
            }
        }
        public int TaskHint
        {
            get
            {
                var match = Regex.Match(this.RawPage, GetTaskPattern);
                return match.Success ? int.Parse(match.Groups["taskHint"].Value) : 0;
            }
        }

        public static DoBuild Open(RequestAgent agent, int pid, int bid, int hid)
        {
            var url = agent.BuildUrl(TCMod.city, TCSubMod.build, TCOperation.Do, TCFunc.build);
            var body = string.Format("pid={0}&bid={1}&hid={2}", pid, bid, hid);
            var page = agent.WebClient.OpenUrl(url, body);
            return new DoBuild(page);
        }
    }
}