using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MarkovApp.libs
{
    public class Cell
    {
        public string FirstIndex { get; set; }
        public string SecondIndex { get; set; }

        public Node StartNode { get; set; }

        public Node EndNode { get; set; }
        
        public  TextBox TextBox { get; set; }
        public double Value { get; set; }

        public Cell(string firstIndex, string secondIndex, TextBox textBox, double value)
        {
            FirstIndex = firstIndex;
            SecondIndex = secondIndex;
            TextBox = textBox;
            Value = value;
        }

        public Cell(string firstIndex, string secondIndex ,Node startNode,Node endNode,TextBox textBox,double value)
        {
            FirstIndex = firstIndex;
            SecondIndex = secondIndex;
            StartNode=startNode;
            EndNode=endNode;
            TextBox =textBox;
            Value = value;
        }
    }

}
