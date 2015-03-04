using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCModels
{
    class TCArmy
    {
        public TCAccount Account
        {
            get;
            private set;
        }

        public TCCity City
        {
            get;
            set;
        }

        public TCArmy(TCAccount account)
        {
            this.Account = account;
        }
    }
}
