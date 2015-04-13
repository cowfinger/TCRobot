using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCDataType
{
    class CityScienceId
    {
        public int ScienceType { get; private set; }

        public int ScienceId { get; private set; }

        public CityScienceId(int scienceType, int scienceId)
        {
            this.ScienceId = scienceId;
            this.ScienceType = scienceType;
        }
    }
}
