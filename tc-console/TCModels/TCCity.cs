using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC
{
    class TCCity
    {
        public string Name
        {
            get;
            private set;
        }

        public int CityId
        {
            get;
            private set;
        }

        public int NodeId
        {
            get;
            private set;
        }

        public TCInfluence Influence
        {
            get;
            private set;
        }

        public TCCity(TCInfluence influence)
        {
            this.Influence = influence;
        }
    }
}
