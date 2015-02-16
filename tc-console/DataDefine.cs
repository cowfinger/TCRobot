using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TC
{
    public class TeamInfo
    {
        public string Name;
        public string AccountName;
        public string TeamId;
        public string GroupId;
        public string Leader;
        public string DurationString;
        public int TimeLeft = 0;
        public int Duration = 0;
        public bool IsTroopSent = false;
        public bool isGroupTeam = false;
        public bool isDefendTeam = false;
        public int PowerIndex = 0;
    }

    class AccountInfo
    {
        public string UserName;
        public string Password;
        public string AccountType;
        public string CookieStr;
        public string LoginStatus;
        public IEnumerable<string> CityIDList = new List<string>();
    }

    class LoginParam
    {
        public string Name;
        public string LoginURL;
        public string UsernameElmID;
        public string PasswordElmID;
        public string LoginTitle;
        public string HomeTitle;
        public string LogoutURL;
    }

    class AttackTask
    {
        public AccountInfo Account;
        public List<TeamInfo> TeamList = new List<TeamInfo>();
        public Thread Worker;
        public string FromCity;
        public string ToCity;
        public DateTime StartTime;
        public DateTime EndTime;
    }
}
