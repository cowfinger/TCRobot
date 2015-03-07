using System;
using System.Collections.Generic;
using System.Text;

namespace TC
{
    public class TCResource
    {
        public int r1;
        public int r2;
        public int r3;
        public int r4;
        public const int MaxRes = 2250000;
        public int GetMin()
        {
            int min = r1;
            if (min > r2) min = r2;
            if (min > r3) min = r3;
            if (min > r4) min = r4;
            return min;
        }

        public int CanBuyMaxBoxNum()
        {
            return (GetMin() - 1000) / 1000;
        }

        public int CanOpenMaxBoxNum()
        {
            if ((MaxRes - 100000) > GetMax())
            {
                return (MaxRes - 50000 - GetMax()) / 100000;
            }
            else
                return 0;
        }

        public int GetMax()
        {
            int max = r1;
            if (max < r2) max = r2;
            if (max < r3) max = r3;
            if (max < r4) max = r4;
            return max;
        }
    }
}
