namespace TC
{
    public class TCResource
    {
        public const int MaxRes = 2250000;

        public int r1;

        public int r2;

        public int r3;

        public int r4;

        public int GetMin()
        {
            var min = this.r1;
            if (min > this.r2)
            {
                min = this.r2;
            }
            if (min > this.r3)
            {
                min = this.r3;
            }
            if (min > this.r4)
            {
                min = this.r4;
            }
            return min;
        }

        public int CanBuyMaxBoxNum()
        {
            return (this.GetMin() - 1000) / 1000;
        }

        public int CanOpenMaxBoxNum()
        {
            if ((MaxRes - 100000) > this.GetMax())
            {
                return (MaxRes - 50000 - this.GetMax()) / 100000;
            }
            return 0;
        }

        public int GetMax()
        {
            var max = this.r1;
            if (max < this.r2)
            {
                max = this.r2;
            }
            if (max < this.r3)
            {
                max = this.r3;
            }
            if (max < this.r4)
            {
                max = this.r4;
            }
            return max;
        }
    }
}