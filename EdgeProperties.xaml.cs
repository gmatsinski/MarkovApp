using System;
using System.Windows;
using System.Windows.Controls;

namespace MarkovApp
{
    public partial class EdgeProperties : Window
    {
        public double EdgeValue { get; set; }

        public EdgeProperties()
        {
            InitializeComponent();
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(valueTextBox.Text) && double.Parse(valueTextBox.Text) <= 1 && double.TryParse(valueTextBox.Text, out double probOfStayingValue))
            {
                EdgeValue = double.Parse(valueTextBox.Text);
            }
            else
            {
                EdgeValue = 0;
            }
            this.DialogResult = true;
            Close();
        }
    }
}
