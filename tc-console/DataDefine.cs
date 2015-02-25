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
    public class TroopInfo
    {
        public string Name = "";
        public string AccountName = "";
        public string TroopId = "";
        public string GroupId = "";
        public string Leader = "";
        public string ToCityNodeId;
        public int Duration = 0;
        public bool isGroupTroop = false;
        public bool isDefendTroop = false;
        public bool IsGroupHead = false;
        public int PowerIndex = 0;
    }

    public class AccountInfo
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
    }

    class AttackTask
    {
        public string TaskId = "";
        public string TaskType = "";
        public string AccountName;
        public string FromCity;
        public string ToCity;
        public DateTime EndTime;
        public TroopInfo Troop = null;
    }

    class HeroInfo
    {
        public string AccountName = "";
        public string HeroId = "";
        public string Name = "";
        public bool IsDead = false;
    }
}
