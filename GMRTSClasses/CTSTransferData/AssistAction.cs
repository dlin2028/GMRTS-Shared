using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSClasses.CTSTransferData
{
    public class AssistAction : ClientAction
    {
        public List<Guid> TargetUnits { get; set; }
    }
}
