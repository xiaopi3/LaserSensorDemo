using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo
{
    public class Data
    {
        public int Index { get; set; }
        public long Time { get; set; }
        public long Dist { get; set; }

        public Data(int Index, long Time, long Dist)
        {
            this.Index = Index;
            this.Time = Time;
            this.Dist = Dist;
        }
    }
}
