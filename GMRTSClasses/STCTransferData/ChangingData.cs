using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSClasses.STCTransferData
{
    public struct ChangingData<T>
    {
        public ulong StartTime { get; set; }
        public T Value { get; set; }
        public T Delta { get; set; }
        public ChangingData(ulong startTime, T startValue, T delta)
        {
            StartTime = startTime;
            Value = startValue;
            Delta = delta;
        }

    }
}
