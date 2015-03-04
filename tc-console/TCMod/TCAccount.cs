using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TC
{
    class TCAccount
    {
        private const string CookieFolder = @".\Cookie";

        public enum AccountLoginStatus
        {
            Offline, Submitting, Logining, Online, LoginFailed
        }

        public TCInfluence Influence
        {
            get;
            private set;
        }

        public string UserName
        {
            get;
            private set;
        }

        public string Password
        {
            get;
            private set;
        }

        public string AccountType
        {
            get;
            private set;
        }

        public CookieLite Cookie
        {
            get;
            private set;
        }

        public AccountLoginStatus LoginStatus
        {
            get;
            set;
        }

        public IEnumerable<TCCity> CityList
        {
            get;
            private set;
        }

        public TCAccount(string userName, string password, string accountType)
        {
            this.CityList = new List<TCCity>();
            this.Cookie = new CookieLite();
            this.UserName = userName;
            this.Password = password;
            this.AccountType = accountType;
            this.LoginStatus = AccountLoginStatus.Offline;
        }

        public bool LoadCookie()
        {
            string accountCookieFileName = Path.Combine(CookieFolder, this.UserName);
            return this.Cookie.Load(accountCookieFileName);
        }

        public void SaveCookie()
        {
            if (!Directory.Exists(CookieFolder))
            {
                Directory.CreateDirectory(CookieFolder);
            }

            string accountCookieFileName = Path.Combine(CookieFolder, this.UserName);

            this.Cookie.Save(accountCookieFileName);
        }

        public void Login(LoginHelper helper)
        {
            helper.LoginAccount(this);
        }
    }
}
