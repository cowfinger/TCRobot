using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC
{
    class TCInfluence
    {
        public IList<TCAccount> AccountList
        {
            get;
            private set;
        }

        public IList<TCCity> CityList
        {
            get;
            private set;
        }

        public TCInfluence()
        {
            this.AccountList = new List<TCAccount>();
        }
    }
}
