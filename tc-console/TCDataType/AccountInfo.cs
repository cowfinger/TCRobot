namespace TC
{
    using System.Collections.Generic;

    public class AccountInfo
    {
        public string AccountType;

        public List<string> CityIDList = new List<string>();

        public List<string> CityNameList = new List<string>();

        public string CookieStr;

        public string FirstStepCityID;

        public CityInfo MainCity = null;

        public Dictionary<string, HashSet<string>> InfluenceMap = null;

        public Dictionary<string, CityInfo> InfluenceCityList = null;

        public string LoginStatus;

        public string Password;

        public string UserName;

        public int UnionId;

        public int Level;
    }
}