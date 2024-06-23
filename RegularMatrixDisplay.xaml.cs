using MathNet.Numerics.Providers.SparseSolver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// <summary>
    /// Interaction logic for RegularMatrixDisplay.xaml
    /// </summary>
    public partial class RegularMatrixDisplay : Window
    {
        public string RegularMatrixText { get; set; }

        public RegularMatrixDisplay()
        {
            InitializeComponent();
        }

        public void SetRegularMatrix(double[,] regularMatrix,List <double> stateVector)
        {
            RegularMatrixText = ConvertMatrixToString(regularMatrix);
            string matrixStr = RegularMatrixText;
            for (int i = 0; i < stateVector.Count; i++)
            {
                stateVector[i] = Math.Round(stateVector[i], 2);
            }

            string statesStr = string.Join(", ", stateVector);

            matrixTextBox.Text = matrixStr;
            statesTextBox.Text = statesStr;

            WriteResultToFile(matrixStr, statesStr);
        }

        private static void WriteResultToFile(string matrixStr,string statesStr)
        {
            string allResults = "Матрица резултат:" + "\n" + matrixStr + "\n\n" + "Състояния:" + "\n" + statesStr;

            string relativePath = @"results.txt";
            string directory = Directory.GetCurrentDirectory();
            while (!Directory.GetFiles(directory, "*.csproj").Any() && !Directory.GetFiles(directory, "*.sln").Any())
            {
                directory = Directory.GetParent(directory).FullName;
            }

            string filePath = System.IO.Path.Combine(directory, relativePath);

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.Write(allResults);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while writing to the file: {ex.Message}");
            }
        }

        private static string ConvertMatrixToString(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            string matrixString = "  i,j    ";

            for (int j = 0; j < cols; j++)
            {
                matrixString += (j + 1).ToString().PadRight(10);
            }
            matrixString += "\n";

            for (int i = 0; i < rows; i++)
            {
                matrixString += (i + 1).ToString().PadRight(7);

                for (int j = 0; j < cols; j++)
                {
                    matrixString += matrix[i, j].ToString("F2").PadRight(10);
                }
                matrixString += "\n";
            }

            return matrixString;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
