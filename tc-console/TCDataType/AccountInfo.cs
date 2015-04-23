using System;

namespace TC
{
    using System.Collections.Generic;
    using System.Text;

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

        public int Tid { get; set; }

        public string NickName { get; set; }

        public int CountryId { get; set; }

        public string CountryName
        {
            get
            {
                switch (this.CountryId)
                {
                    case 1: return "魏";
                    case 2: return "蜀";
                    case 3: return "吴";
                }
                return "N/A";
            }
        }

        public string HintString
        {
            get
            {
                var hintBuilder = new StringBuilder();
                hintBuilder.AppendFormat("{0}:", this.NickName);
                hintBuilder.AppendFormat("{0},", this.CountryName);
                return hintBuilder.ToString();
            }
        }

    }
}