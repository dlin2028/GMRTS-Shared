using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSClasses.Units
{
    public abstract class Building : Unit
    {
        public Building() : base()
        {

        }

        public Building(Guid id) : base(id)
        {

        }
    }
}
