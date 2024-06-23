using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace MarkovApp
{
    /// <summary>
    /// Interaction logic for AbsorbingMatrixCalculations.xaml
    /// </summary>
    public partial class AbsorbingMatrixCalculations : Window
    {
        public AbsorbingMatrixCalculations()
        {
            InitializeComponent();
        }
        public void DisplayMatrices(double[,] averageTransitions, double[,] probabilities,List<int> absorbingStates)
        {
            string averageTransitionsStr = ConvertAverageTransitionsMatrixToString(averageTransitions);
            string probabilitiesStr = ConvertProbabilitiesMatrixToString(probabilities, absorbingStates);

            averageTransitionsTextBox.Text = averageTransitionsStr;
            probabilityTextBox.Text = probabilitiesStr;

            WriteResultToFile(averageTransitionsStr,probabilitiesStr);

        }

        private static void WriteResultToFile(string averageTransitionsStr,string probabilitiesStr)
        {
            string allResults = "Среден брой на преминаванията от всяко непоглъщащо състояние:" + "\n" + averageTransitionsStr + "\n\n" + "Вероятностни оценки процес започнал от състояние i да завърши в абсорбиращо състояние j" + "\n" + probabilitiesStr;

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

        private static string ConvertAverageTransitionsMatrixToString(double[,] matrix)
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
        private static string ConvertProbabilitiesMatrixToString(double[,] matrix, List<int> absorbingNodes)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            string matrixString = "  i,j    ";

            for (int j = 0; j < absorbingNodes.Count; j++)
            {
                matrixString += (absorbingNodes[j]+1).ToString().PadRight(10);
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
