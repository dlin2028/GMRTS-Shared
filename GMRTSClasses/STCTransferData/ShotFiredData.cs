using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSClasses.STCTransferData
{
    public class ShotFiredData
    {
        public Guid ShooterID { get; set; }
        public Guid ShotUnitID { get; set; }
        public float Damage { get; set; }
    }
}
