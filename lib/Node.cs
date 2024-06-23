using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace MarkovApp.libs
{
    public class Node
    {
        public string Index { get; set; }
        public Ellipse Ellipse { get; set; }
        public double? InitialProbability { get; set; }
        public double? ProbabilityOfStaying { get; set; }
        public bool? IsAbsorbing { get; set; } 

        public Node(string index, Ellipse ellipse)
        {
            Index = index;
            Ellipse = ellipse;
            IsAbsorbing = false;
        }
       
    }
}
