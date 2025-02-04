using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace atOpticalDecenter.Functions.Time
{
    public class TimeChecker
    {
        private long mTimeMarker = 0;
        private long mRequestedWaitTime = 0;

        public void SetTime(long mTimeInMS)
        {
            mRequestedWaitTime = mTimeInMS;
            mTimeMarker = GetSystemTimeInMS();
        }

        public bool IsTimeOver()
        {
            if (mRequestedWaitTime <= (GetSystemTimeInMS() - mTimeMarker))
                return true;
            else
                return false;
        }

        public long ElapseTime { get { return GetSystemTimeInMS() - mTimeMarker; } }

        private long GetSystemTimeInMS()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }
    }
}
