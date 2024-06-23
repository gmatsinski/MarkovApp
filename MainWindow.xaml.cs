using MarkovApp.libs;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MathNet.Numerics.LinearAlgebra;
using System.Linq;
using System.Text;

namespace MarkovApp
{
    public partial class MainWindow : Window
    {
        //Validation variables
        bool isValidTransitionMatrix=false;
        bool isValidStateVector=false;

        //Figure construction variables
        private Line drawingLine = null;
        private Ellipse startEllipse = null;

        //Node counter:
        private int nodeCounter = 0;

        //Node dictionaries:
        private readonly Dictionary<Ellipse, Node> nodeDictionaryByEllipse = new();
        private readonly Dictionary<string, Node> nodeDictionaryByIndex= new();

        //Edge dictionaries:
        private readonly Dictionary<Line, Edge> edgeDictionaryByLine = new();
        private readonly Dictionary<Path, Edge> edgeDictionaryByPath = new();

        //Cell dictionaries
        private readonly Dictionary<string, Cell> cellDictionaryByIndex = new();
        private readonly Dictionary<TextBox, Cell> cellDictionaryByTextBox = new();

        //Steady Vector
        List<double> initialStateValues = new();


        public MainWindow()
        {
            InitializeComponent();
            graphCanvas.MouseMove += GraphCanvas_MouseMove;
            graphCanvas.MouseRightButtonUp += GraphCanvas_MouseRightButtonUp;
        }

        //Mouse-figure events:
        private void GraphCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (e.Source is Ellipse)
                {
                    Ellipse currentEllipse = e.Source as Ellipse;
                    Node currentNode = nodeDictionaryByEllipse[currentEllipse];
                    NodeProperties nodePropertiesWindow = new ();
                    nodePropertiesWindow.CurrentNode = currentNode;

                    bool? dialogResult = nodePropertiesWindow.ShowDialog();

                    if (dialogResult == true)
                    {
                        double initialProbability = nodePropertiesWindow.InitialProbability;
                        double probabilityOfStaying = nodePropertiesWindow.ProbabilityOfStaying;

                        currentNode.InitialProbability = initialProbability;
                        currentNode.ProbabilityOfStaying = probabilityOfStaying;
                        UpdateEdgeValueInGrid(currentNode.Index, currentNode.Index, probabilityOfStaying);
                        UpdateVector();
                        IsValidInitialStateVector(initialStateValues);
                        int matrixSize = cellDictionaryByIndex.Count;
                        IsValidTransitionMatrix(matrixSize);
                    }

                }
                if (e.Source is Canvas)
                {
                    Point position = e.GetPosition(graphCanvas);
                    if (!IsNearCurve(position))
                    {
                        AddEllipse(position);
                    }

                }
            }
           
        }

        private void GraphCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (e.Source is Ellipse)
            {
                startEllipse = (Ellipse)e.Source;
                drawingLine = new Line
                {
                    Stroke = Brushes.Purple,
                    StrokeThickness = 2,
                    X1 = Canvas.GetLeft(startEllipse) + startEllipse.Width / 2,
                    Y1 = Canvas.GetTop(startEllipse) + startEllipse.Height / 2,
                    X2 = Canvas.GetLeft(startEllipse) + startEllipse.Width / 2,
                    Y2 = Canvas.GetTop(startEllipse) + startEllipse.Height / 2
                };
                graphCanvas.Children.Add(drawingLine);

                string firstIndex = nodeDictionaryByEllipse[startEllipse].Index;
                //Add edge by drawingLine for initial state of the edge
                Edge initialEdge = new Edge(firstIndex, null, null, 0.0);
                edgeDictionaryByLine.Add(drawingLine, initialEdge);
            }

        }

        private void GraphCanvas_MouseMove(object sender, MouseEventArgs e)
        {
           if (drawingLine != null)
           {
                Point position = e.GetPosition(graphCanvas);
                drawingLine.X2 = position.X;
                drawingLine.Y2 = position.Y;
           }

        }

        private void GraphCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (drawingLine != null)
            {
                Point endPosition = e.GetPosition(graphCanvas);

                Ellipse endEllipse = null;
                foreach (UIElement element in graphCanvas.Children)
                {
                    //Searching for the ending ellipse
                    if (element is Ellipse ellipse)
                    {
                        if (IsPointInsideEllipse(endPosition, ellipse))
                        {
                            endEllipse = ellipse;
                            break;
                        }
                    }
                }
                if (endEllipse != null)
                {
                    Point start = new(drawingLine.X1, drawingLine.Y1);
                    Point end = new(drawingLine.X2, drawingLine.Y2);

                    double distance = Math.Sqrt(Math.Pow(end.X - start.X, 2) + Math.Pow(end.Y - start.Y, 2));

                    double curvatureFactor = 0.05;

                    Vector perpendicular = new Vector(start.Y - end.Y, end.X - start.X);
                    perpendicular.Normalize();

                    Point control1_1 = new(start.X + perpendicular.X * distance * curvatureFactor, start.Y + perpendicular.Y * distance * curvatureFactor);
                    Point control2_1 = new (end.X + perpendicular.X * distance * curvatureFactor, end.Y + perpendicular.Y * distance * curvatureFactor);

                    Point control1_2 = new(start.X - perpendicular.X * distance * curvatureFactor, start.Y - perpendicular.Y * distance * curvatureFactor);
                    Point control2_2 = new(end.X - perpendicular.X * distance * curvatureFactor, end.Y - perpendicular.Y * distance * curvatureFactor);

                    Random rnd = new();
                    byte red = 0;
                    byte green = 0;
                    byte blue = 0;

                    switch (rnd.Next(5))
                    {
                        case 0:
                            red = (byte)rnd.Next(170, 256);
                            break;
                        case 1:
                            green = (byte)rnd.Next(170, 256);
                            break;
                        case 2:
                            red = (byte)rnd.Next(170, 256);
                            green = (byte)rnd.Next(170, 256);
                            break;
                        case 3:
                            blue = (byte)rnd.Next(170, 256);
                            break;
                        case 4:
                            red = (byte)rnd.Next(100, 200);
                            green = (byte)rnd.Next(50, 150);
                            break;
                        case 5:
                            red = (byte)rnd.Next(170, 256);
                            blue = (byte)rnd.Next(170, 256);
                            break;

                    }

                    Color randomColor = Color.FromRgb(red, green, blue);

                    //Creating first curve(edge)
                    PathFigure curveFigure1 = new PathFigure
                    {
                        StartPoint = start,
                        Segments = { new BezierSegment(control1_1, control2_1, end, true) }
                    };
                    PathGeometry curveGeometry1 = new PathGeometry { Figures = { curveFigure1 } };
                    Path curvePath1 = new Path { Stroke = new SolidColorBrush(randomColor), StrokeThickness = 4, Data = curveGeometry1 };
                    graphCanvas.Children.Add(curvePath1);

                    string secondIndex = nodeDictionaryByEllipse[endEllipse].Index;
                    Edge edge = edgeDictionaryByLine[drawingLine];
                    edge.SecondIndex = secondIndex;
                    edge.Curve = curvePath1;

                    //Creating second curve(edge) in oposite direction
                    PathFigure curveFigure2 = new PathFigure
                    {
                        StartPoint = start,
                        Segments = { new BezierSegment(control1_2, control2_2, end, true) }
                    };

                    PathGeometry curveGeometry2 = new PathGeometry { Figures = { curveFigure2 } };
                    Path curvePath2 = new Path { Stroke = new SolidColorBrush(randomColor), StrokeThickness = 4, Data = curveGeometry2 };
                    graphCanvas.Children.Add(curvePath2);

                    //Creating the second edge
                    Edge followingEdge = new Edge(edge.SecondIndex, edge.FirstIndex, curvePath2, 0.0);

                    //Adding the 2 edges
                    edgeDictionaryByPath.Add(curvePath2, edge);
                    edgeDictionaryByPath.Add(curvePath1, followingEdge);
                    
                }
                else
                {
                    graphCanvas.Children.Remove(drawingLine);
                }
            }
           

            drawingLine = null;
            startEllipse = null;
        }

        private void Curve_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && e.Source is Path path)
            {
                Path clickedCurve = path;
                EdgeProperties edgePropertiesWindow = new EdgeProperties();
                bool? dialogResult=edgePropertiesWindow.ShowDialog();
                if(dialogResult== true)
                {
                    Edge clickedEdge = edgeDictionaryByPath[clickedCurve];
                    clickedEdge.Value = edgePropertiesWindow.EdgeValue;
                    UpdateEdgeValueInGrid(edgeDictionaryByPath[clickedCurve].FirstIndex, edgeDictionaryByPath[clickedCurve].SecondIndex, clickedEdge.Value);
                    int matrixSize = cellDictionaryByIndex.Count;
                    IsValidTransitionMatrix(matrixSize);
                }
            }
        }

        private void RegularMCButton_Click(object sender, RoutedEventArgs e)
        {
            if(isValidTransitionMatrix && isValidStateVector && IsGraphAbsorbing() == false)
            {
                int matrixSize = (int)Math.Sqrt(cellDictionaryByIndex.Count);
                double[,] transitionMatrix = new double[matrixSize, matrixSize];
                List<double> stateVector = new List<double>(initialStateValues);

                foreach (var cell in cellDictionaryByIndex.Values)
                {
                    int rowIndex = int.Parse(cell.FirstIndex);
                    int colIndex = int.Parse(cell.SecondIndex);

                    transitionMatrix[rowIndex-1, colIndex-1] = cell.Value;
                }

                for (int i = 0; i < matrixSize; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < matrixSize; j++)
                    {
                        sum += transitionMatrix[i, j] * initialStateValues[j];
                    }
                    stateVector[i] = sum;
                }

                double[,] regularMatrix = CalculateRegularMarkovChain(transitionMatrix);
                DisplayRegularMatrix(regularMatrix,stateVector);


            }
            else
            {
                MessageBox.Show("Не може да продължите към изчисленията преди да оправите всички грешки.Уверете се ,че графът НЕ Е абсорбиращ,също", "Грешка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AbsorbingMCButton_Click(object sender, RoutedEventArgs e)
        {
            if (isValidTransitionMatrix && isValidStateVector && IsGraphAbsorbing() == true)
            {
                int matrixSize = (int)Math.Sqrt(cellDictionaryByIndex.Count);

                double[,] transitionMatrix = new double[matrixSize, matrixSize];

                foreach (var cell in cellDictionaryByIndex.Values)
                {
                    int rowIndex = int.Parse(cell.FirstIndex);
                    int colIndex = int.Parse(cell.SecondIndex);

                    transitionMatrix[rowIndex - 1, colIndex - 1] = cell.Value;
                }

          
                CalculateAbsorbingMatrix(transitionMatrix, out double[,] averageTransitions, out double[,] probabilities,out List<int> absorbingStates);
                DisplayAbsorbingMatrices(averageTransitions, probabilities,absorbingStates);


            }
            else
            {
                MessageBox.Show("Не може да продължите към изчисленията преди да оправите всички грешки.Уверете се , че графът е абсорбиращ,също.", "Грешка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void DisplayRegularMatrix(double[,] regularMatrix,List <double> stateVector)
        {
            RegularMatrixDisplay regularMatrixDisplay = new RegularMatrixDisplay();

            regularMatrixDisplay.SetRegularMatrix(regularMatrix,stateVector);

            regularMatrixDisplay.ShowDialog();
        }
        public static double[,] CalculateRegularMarkovChain(double[,] transitionMatrix)
        {
            int matrixSize = transitionMatrix.GetLength(0);
            double[,] currentMatrix = (double[,])transitionMatrix.Clone();
            double[,] nextMatrix = new double[matrixSize, matrixSize];
            double epsilon = 0.0001;
            int maxIterations = 50;

            bool isRegular = false;

            for (int iteration = 1; iteration <= maxIterations; iteration++)
            {
                nextMatrix = MatrixMultiply(transitionMatrix, currentMatrix);

                bool isStable = true;
                for (int i = 0; i < matrixSize; i++)
                {
                    for (int j = 0; j < matrixSize; j++)
                    {
                        if (Math.Abs(nextMatrix[i, j] - currentMatrix[i, j]) >= epsilon)
                        {
                            isStable = false;
                            break;
                        }
                    }
                    if (!isStable)
                        break;
                }

                if (isStable)
                {
                    isRegular = true;
                    break;
                }

                currentMatrix = (double[,])nextMatrix.Clone();
            }

            if (!isRegular)
            {
                double[,] emptyMatrix = new double[matrixSize, matrixSize];
                return emptyMatrix;
            }

            return nextMatrix;
        }

        public static double[,] MatrixMultiply(double[,] matrix1, double[,] matrix2)
        {
            int size = matrix1.GetLength(0);
            double[,] result = new double[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    for (int k = 0; k < size; k++)
                    {
                        result[i, j] += matrix1[i, k] * matrix2[k, j];
                    }
                }
            }

            return result;
        }

        private static void DisplayAbsorbingMatrices(double[,] averageTransitions, double[,] probabilities,List <int> absorbingStates)
        {
            AbsorbingMatrixCalculations window = new();
            window.DisplayMatrices(averageTransitions, probabilities,absorbingStates);
            window.ShowDialog();
        }
        private static void CalculateAbsorbingMatrix(double[,] transitionMatrix, out double[,] averageTransitions, out double[,] probabilities,out List<int> absorbingStates)
        {
            int matrixSize = transitionMatrix.GetLength(0);

            absorbingStates = new List<int>();
            for (int i = 0; i < matrixSize; i++)
            {
                bool isAbsorbing = true;
                for (int j = 0; j < matrixSize; j++)
                {
                    if (i != j && transitionMatrix[i, j] != 0)
                    {
                        isAbsorbing = false;
                        break;
                    }
                }
                if (isAbsorbing)
                    absorbingStates.Add(i);
            }

            int nonAbsorbingCount = matrixSize - absorbingStates.Count;

            double[,] Q = new double[nonAbsorbingCount, nonAbsorbingCount];
            double[,] R = new double[nonAbsorbingCount, absorbingStates.Count];
            int row = 0;
            for (int i = 0; i < matrixSize; i++)
            {
                if (absorbingStates.Contains(i))
                    continue;

                int qCol = 0;
                for (int j = 0; j < matrixSize; j++)
                {
                    if (!absorbingStates.Contains(j))
                        Q[row, qCol++] = transitionMatrix[i, j];
                    else
                        R[row, absorbingStates.IndexOf(j)] = transitionMatrix[i, j];
                }
                row++;
            }

            var QMatrix = Matrix<double>.Build.DenseOfArray(Q);
            var RMatrix = Matrix<double>.Build.DenseOfArray(R);

            var identityMatrix = Matrix<double>.Build.DenseIdentity(nonAbsorbingCount);
            var TMatrix = (identityMatrix - QMatrix).Inverse();

            probabilities = (TMatrix * RMatrix).ToArray();
            averageTransitions = TMatrix.ToArray();
        }


        //Checks
        private bool IsNearCurve(Point position)
        {
            // Define the distance threshold from the curve
            double threshold = 5; // Adjust this value as needed

            foreach (UIElement element in graphCanvas.Children)
            {
                if (element is Path path)
                {
                    // Get the bounding box of the curve
                    Rect bounds = path.RenderedGeometry.Bounds;

                    // Inflate the bounding box to create a boundary around the curve
                    bounds.Inflate(threshold, threshold);

                    // Check if the position is within the boundary
                    if (bounds.Contains(position))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private static bool IsPointInsideEllipse(Point point, Ellipse ellipse)
        {
            double ellipseLeft = Canvas.GetLeft(ellipse);
            double ellipseTop = Canvas.GetTop(ellipse);
            double ellipseRight = ellipseLeft + ellipse.Width;
            double ellipseBottom = ellipseTop + ellipse.Height;

            return point.X >= ellipseLeft && point.X <= ellipseRight &&
                   point.Y >= ellipseTop && point.Y <= ellipseBottom;
        }
        
        //Initializing
        private void AddEllipse(Point position)
        {
            Ellipse ellipse = new ()
            {
                Width = 40,
                Height = 40,
                Fill = Brushes.Purple,
                Stroke = Brushes.Black,
                StrokeThickness = 3
            };
            Canvas.SetLeft(ellipse, position.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, position.Y - ellipse.Height / 2);
            graphCanvas.Children.Add(ellipse);
            nodeCounter++;
            Label label = new ()
            {
                Content = nodeCounter.ToString(),
                Foreground = Brushes.Purple,
                FontSize = 18,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(label, Canvas.GetLeft(ellipse) + ellipse.Width + 5);
            Canvas.SetTop(label, Canvas.GetTop(ellipse) + ellipse.Height / 2 - label.ActualHeight / 2);
            graphCanvas.Children.Add(label);
            Node node = new (nodeCounter.ToString(), ellipse);
            nodeDictionaryByIndex.Add(nodeCounter.ToString(), node);
            nodeDictionaryByEllipse.Add(ellipse, node);
            GenerateMatrix(nodeCounter);
            
        }

        private void GenerateMatrix(int counter)
        {
            matrixGrid.Children.Clear();
            matrixGrid.RowDefinitions.Clear();
            matrixGrid.ColumnDefinitions.Clear();

            cellDictionaryByIndex.Clear();

            int numRows = counter;
            int numCols = counter;

            for (int i = 0; i <= numRows; i++)
            {
                matrixGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (int j = 0; j <= numCols; j++)
            {
                matrixGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 1; i <= numRows; i++)
            {
                for (int j = 1; j <= numCols; j++)
                {
                    TextBox textBox = new TextBox();
                    Grid.SetRow(textBox, i);
                    textBox.Text = "0";
                    textBox.IsReadOnly = true;
                    Grid.SetColumn(textBox, j);
                    matrixGrid.Children.Add(textBox);
                    Cell cell = new(i.ToString(), j.ToString(), textBox, 0.0);
                    cellDictionaryByIndex.Add($"{i},{j}", cell);
                }
            }
        }
        //Updates
        private void UpdateEdgeValueInGrid(string firstIndex, string secondIndex, double value)
        {

            if (cellDictionaryByIndex.ContainsKey($"{firstIndex},{secondIndex}"))
            {
                Node startNode = nodeDictionaryByIndex[firstIndex];
                Node endNode = nodeDictionaryByIndex[secondIndex];
                cellDictionaryByIndex[$"{firstIndex},{secondIndex}"].StartNode = startNode;
                cellDictionaryByIndex[$"{firstIndex},{secondIndex}"].EndNode = endNode;
                cellDictionaryByIndex[$"{firstIndex},{secondIndex}"].Value = value;
                cellDictionaryByIndex[$"{firstIndex},{secondIndex}"].TextBox.Text = value.ToString();
            }
        }
        private void UpdateVector()
        {
            textBoxVector.Clear();

            List<double> currentInitialValues = new();

            foreach (var kvp in nodeDictionaryByIndex)
            {
                Node node = kvp.Value;

                if (node.InitialProbability.HasValue)
                {
                    double value = (double)node.InitialProbability;
                    currentInitialValues.Add(value);
                }
                else
                {
                    double value = 0;
                    currentInitialValues.Add(value);
                }
            }
            initialStateValues = currentInitialValues;
            int cnt = 1;
            foreach (double value in initialStateValues)
            {
                textBoxVector.Text += cnt.ToString()+":"+" "+value.ToString() + " ; ";
                cnt++;
            }
           
        }
        
        //Restarting
        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            matrixGrid.Children.Clear();
            matrixGrid.RowDefinitions.Clear();
            matrixGrid.ColumnDefinitions.Clear();
            nodeDictionaryByEllipse.Clear();
            nodeDictionaryByIndex.Clear();
            edgeDictionaryByLine.Clear();
            edgeDictionaryByPath.Clear();
            cellDictionaryByIndex.Clear();
            cellDictionaryByTextBox.Clear();
            absorbingMcButton.Visibility = Visibility.Collapsed;
            regularMcButton.Visibility = Visibility.Collapsed;
            textBoxVector.Text = "";

            Clear();

            nodeCounter = 0;
        }
        private void Clear()
        {

            foreach (UIElement element in graphCanvas.Children.OfType<Ellipse>().ToList())
            {
                graphCanvas.Children.Remove(element);
            }

            foreach (UIElement element in graphCanvas.Children.OfType<Path>().ToList())
            {
                graphCanvas.Children.Remove(element);
            }

            foreach (UIElement element in graphCanvas.Children.OfType<Line>().ToList())
            {
                graphCanvas.Children.Remove(element);
            }

            foreach (UIElement element in graphCanvas.Children.OfType<Label>().ToList())
            {
                graphCanvas.Children.Remove(element);
            }

            foreach (UIElement element in bottomGrid.Children.OfType<Label>().ToList())
            {
                if (!(element is Label label && label.Content.ToString() == "Вектор на състоянията"))
                {
                    element.Visibility = Visibility.Collapsed;
                }
            }
        }

        //Evaluates

        private void IsValidInitialStateVector(List<double> initialValues)
        {
            if(initialValues.Sum()==1)
            {
                isValidStateVector = true;
                validLabel.Visibility=Visibility.Visible;
                invalidLabel.Visibility = Visibility.Collapsed;
            }
            else
            {
                invalidLabel.Visibility = Visibility.Visible;
                validLabel.Visibility = Visibility.Collapsed;
            }
        }

        private void EvaluateGraphButton_Click(object sender, RoutedEventArgs e)
        {  
            if(nodeDictionaryByIndex.Count != 0 && edgeDictionaryByPath.Count != 0 && isValidStateVector && isValidTransitionMatrix)
            {
                regularMcButton.Visibility = Visibility.Visible;
                absorbingMcButton.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Невалиден граф", "Грешка", MessageBoxButton.OK, MessageBoxImage.Error);
                regularMcButton.Visibility = Visibility.Collapsed;
                absorbingMcButton.Visibility = Visibility.Collapsed;
            }
            
        }

        private void IsValidTransitionMatrix(int matrixSize)
        {
            bool isValid = true;

            for (int i = 1; i <= Math.Sqrt(matrixSize); i++)
            {
                decimal rowSum = 0;

                for (int j = 1; j <= Math.Sqrt(matrixSize); j++)
                {
                    string cellIndex = $"{i},{j}";
                    rowSum += (decimal) cellDictionaryByIndex[cellIndex].Value;
                }

                if (rowSum!=1)
                {
                    isValid = false;
                    break;
                }
            }

            if (isValid)
            {
                isValidTransitionMatrix = isValid;
                validMatrixLabel.Visibility = Visibility.Visible;
                invalidMatrixLabel.Visibility = Visibility.Collapsed;
            }
            else
            {
                validMatrixLabel.Visibility = Visibility.Collapsed;
                invalidMatrixLabel.Visibility = Visibility.Visible;
            }
            
        }

        private bool IsGraphAbsorbing()
        {
            bool isGraphAbsorbing = false;
            foreach (var kvp in nodeDictionaryByIndex)
            {
                var node = kvp.Value;
                if(node.ProbabilityOfStaying==1)
                {
                    isGraphAbsorbing = true;
                }
            }
            return isGraphAbsorbing;
        }
        
    }
}



