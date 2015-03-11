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

        public Dictionary<string, CityInfo> InfluenceCityList = null;

        public Dictionary<string, HashSet<string>> InfluenceMap = null;

        public int Level;

        public string LoginStatus;

        public CityInfo MainCity = null;

        public string Password;

        public int UnionId;

        public string UserName;
    }
}