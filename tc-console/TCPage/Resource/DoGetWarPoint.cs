using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TC.TCPage.Resource
{
    class DoGetWarPoint : TCPage
    {
        public const string WarPointPattern = @"我的战魂值：(\d+)";

        public const string WaitSecondsPattern = @"'next_get_warpoint_time', (\d+)";

        public const string GetResultPattern = @"'祭魂成功举行，获得 (\d+) 点战魂值。'";

        public int TotalWarPoint
        {
            get
            {
                var match = Regex.Match(this.RawPage, WarPointPattern);
                return match.Success ? int.Parse(match.Groups[1].Value) : -1;
            }
        }

        public int GetResultPoint
        {
            get
            {
                var match = Regex.Match(this.RawPage, GetResultPattern);
                return match.Success ? int.Parse(match.Groups[1].Value) : -1;
            }
        }

        public int WaitSeconds
        {
            get
            {
                var match = Regex.Match(this.RawPage, WaitSecondsPattern);
                return match.Success ? int.Parse(match.Groups[1].Value) : -1;
            }
        }

        public DateTime NextGetPointTime
        {
            get
            {
                return FormMain.RemoteTime.AddSeconds(this.WaitSeconds);
            }
        }


        public static DoGetWarPoint Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(
                TCMod.city,
                TCSubMod.resource,
                TCOperation.Do,
                TCFunc.get_warpoint);
            return new DoGetWarPoint(agent.WebClient.OpenUrl(url));
        }

        protected DoGetWarPoint(string page) : base(page)
        {
        }
    }
}
