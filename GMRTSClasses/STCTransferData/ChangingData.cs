using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSClasses.STCTransferData
{
    public struct ChangingData<T>
    {
        public T Value { get; set; }
        public T Delta { get; set; }
    }
}
