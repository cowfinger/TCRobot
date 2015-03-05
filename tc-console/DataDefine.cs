namespace TC
{
    using System;
    using System.Collections.Generic;

    public class TroopInfo
    {
        public string AccountName = "";

        public int Duration = 0;

        public string GroupId = "";

        public bool isDefendTroop = false;

        public bool IsGroupHead = false;

        public bool isGroupTroop = false;

        public string Leader = "";

        public string Name = "";

        public int PowerIndex = 0;

        public string ToCityNodeId;

        public string TroopId = "";
    }

    public class AccountInfo
    {
        public string AccountType;

        public IEnumerable<string> CityIDList = new List<string>();

        public IEnumerable<string> CityNameList = new List<string>();

        public string CookieStr;

        public string FirstStepCityID;

        public Dictionary<string, HashSet<string>> InfluenceMap = null;

        public Dictionary<string, CityInfo> InfluenceCityList = null;

        public string LoginStatus;

        public string Password;

        public string UserName;
    }

    internal class LoginParam
    {
        public string HomeTitle;

        public string LoginTitle;

        public string LoginURL;

        public string Name;

        public string PasswordElmID;

        public string UsernameElmID;
    }

    internal class AttackTask
    {
        public string AccountName;

        public DateTime EndTime;

        public string FromCity;

        public string TaskId = "";

        public string TaskType = "";

        public string ToCity;

        public TroopInfo Troop = null;
    }

    internal class MoveTask
    {
        public DateTime EndTime;
        public string TaskId = "";
    }

    internal class HeroInfo
    {
        public string AccountName = "";

        public string HeroId = "";

        public bool IsDead = false;

        public string Name = "";
    }

    public class CityInfo
    {
        public int CityId = 0;

        public string Name = "";

        public int NodeId = 0;

        public int RoadLevel = 0;
    }
}