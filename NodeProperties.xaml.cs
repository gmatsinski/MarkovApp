using MarkovApp.libs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MarkovApp
{
    public partial class NodeProperties : Window
    {
        public double InitialProbability { get; set; }
        public double ProbabilityOfStaying { get; set; }

        public Node CurrentNode { get; set; }

        public NodeProperties()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtInitialProbability.Text) && double.TryParse(txtInitialProbability.Text, out double initialProbValue) && initialProbValue <= 1)
            {
                InitialProbability = initialProbValue;
            }

            if (!string.IsNullOrEmpty(txtProbabilityOfStaying.Text) && double.TryParse(txtProbabilityOfStaying.Text, out double probOfStayingValue) && probOfStayingValue <= 1)
            {
                ProbabilityOfStaying = probOfStayingValue;
                if (probOfStayingValue == 1)
                {
                    CurrentNode.IsAbsorbing = true;
                }
            }
            this.DialogResult = true;
            Close();
        }

        

    }

}