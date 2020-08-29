using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSClasses.Units
{
    public class Tank : Unit
    {
        public Tank() : base()
        {
        }

        public Tank(Guid id) : base(id)
        {
        }

        public override void Update(float elapsedSeconds)
        {
            base.Update(elapsedSeconds);
        }
    }
}
