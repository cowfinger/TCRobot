using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace TC
{
    class RequestAgent
    {
        private Random randGen = new Random();

        public TCAccount Account { get; private set; }

        public RequestAgent(TCAccount account)
        {
            this.Account = account;
        }

        public string BuildUrl(string urlPathFormat, params object[] args)
        {
            string urlPath = string.Format(urlPathFormat, args);
            return string.Format("http://{0}/index.php?{1}&r={2}", this.Account.AccountType, urlPath, this.randGen.NextDouble());
        }
    }
}
