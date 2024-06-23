using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MarkovApp.libs
{
    public class Edge
    {
        public string FirstIndex { get; set; }
        public string SecondIndex { get; set; }
        public Path Curve { get; set; }
        public double Value { get; set; }

        public Edge(string firstIndex,string secondIndex, Path curve, double value)
        {
            FirstIndex= firstIndex; 
            SecondIndex= secondIndex;
            Curve = curve;
            Value = value;
        }
    }
}
