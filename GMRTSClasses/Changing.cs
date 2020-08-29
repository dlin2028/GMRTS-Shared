using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSClasses
{
    public struct Changing<T>
    {
        public IChanger<T> Changer { get; }

        public T Value { get; set; }
        public T Change { get; set; }

        public Changing(T value, T change, IChanger<T> changer)
        {
            Value = value;
            Change = change;
            Changer = changer;
        }

        public void Update(float elapsedSeconds)
        {
            Value = Changer.Add(Value, Changer.Scale(elapsedSeconds, Change));
        }
    }
}
