using System;

namespace TC
{
    using System.Collections.Generic;

    public class AccountInfo
    {
        public string AccountType;

        public List<string> CityIdList = new List<string>();

        public List<string> CityNameList = new List<string>();

        public string FirstStepCityId;

        public Dictionary<string, CityInfo> InfluenceCityList = null;

        public Dictionary<string, HashSet<string>> InfluenceMap = null;

        public int Level;

        public string LoginStatus;

        public CityInfo MainCity = null;

        public string Password;

        public int UnionId;

        public string UserName;

        public RequestAgent WebAgent { get; set; }

        public int Tid
        {
            get
            {
                if (this.WebAgent == null)
                {
                    return 0;
                }

                var url = new Uri(string.Format("http://{0}/", this.AccountType));
                var cookie = this.WebAgent.WebClient.Cookies.GetCookies(url)["tmp_mid"];
                return int.Parse(cookie != null ? cookie.Value : "0");
            }
        }
    }
}