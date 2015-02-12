using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace TC
{
    class TCAccount
    {
        public enum AccountLoginStatus
        {
            Offline, Submitting, Logining, Online, LoginFailed
        }

        private List<string> cityIDList = new List<string>();
        private CookieLite cookie = new CookieLite();

        public string UserName { get; private set; }
        public string Password { get; private set; }
        public string AccountType { get; private set; }
        public CookieLite Cookie { get { return this.cookie; } private set; }
        public AccountLoginStatus LoginStatus { get; set; }

        public IEnumerable<string> CityIDList
        {
            get { return cityIDList; }
            private set;
        }

        public TCAccount(string userName, string password, string accountType, string cookie)
        {
            this.UserName = userName;
            this.Password = password;
            this.AccountType = accountType;
            this.Cookie.CookieString = cookie;
        }

        public void Login(LoginHelper helper)
        {
        }
    }
}
