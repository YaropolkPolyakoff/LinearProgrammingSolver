using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LinearProgrammingSolver
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LinearProgrammingForm());
        }
    }

    public class LinearProgrammingForm : Form
    {
        private TabControl tabControl;
        private TabPage gomoryTab;
        private TabPage graphicalTab;
        private TextBox problemInputTextBox;
        private TextBox gomoryResultTextBox;
        private PictureBox graphicsBox;
        private Button solveGomoryButton;
        private Button solveGraphicalButton;
        private Label instructionsLabel;

        private LinearProgrammingProblem problem;
        private GomorySolver gomorySolver;
        private GraphicalSolver graphicalSolver;

        public LinearProgrammingForm()
        {
            InitializeComponent();
            SetupExampleProblem();
        }

        private void InitializeComponent()
        {
            this.Text = "Linear Programming Solver - Gomory & Graphical Method";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            tabControl = new TabControl { Dock = DockStyle.Fill };

            // Gomory Method Tab
            gomoryTab = new TabPage { Text = "Gomory Method", Padding = new Padding(10) };
            var gomoryPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3 };
            
            instructionsLabel = new Label
            {
                Text = "Enter problem in format:\nf = c1*x1 + c2*x2 + ... -> max/min\nConstraints:\na1*x1 + a2*x2 + ... = b1\nVariables: x1, x2, ... >= 0 (integer)",
                AutoSize = true,
                Padding = new Padding(5)
            };

            problemInputTextBox = new TextBox
            {
                Multiline = true,
                Height = 200,
                Dock = DockStyle.Fill,
                Text = "f = x1 + x2 -> max\nx1 + 2*x2 + x3 = 5\n2*x1 + x2 + x4 = 6"
            };

            solveGomoryButton = new Button { Text = "Solve with Gomory Method", Height = 40, Dock = DockStyle.Bottom };
            solveGomoryButton.Click += SolveGomory_Click;

            gomoryResultTextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.White
            };

            gomoryPanel.Controls.Add(instructionsLabel, 0, 0);
            gomoryPanel.Controls.Add(problemInputTextBox, 0, 1);
            gomoryPanel.Controls.Add(solveGomoryButton, 0, 2);
            gomoryPanel.Controls.Add(gomoryResultTextBox, 1, 0);
            gomoryPanel.SetRowSpan(gomoryResultTextBox, 3);

            gomoryTab.Controls.Add(gomoryPanel);

            // Graphical Method Tab
            graphicalTab = new TabPage { Text = "Graphical Method", Padding = new Padding(10) };
            var graphicalPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };

            solveGraphicalButton = new Button { Text = "Solve with Graphical Method", Height = 40, Dock = DockStyle.Top };
            solveGraphicalButton.Click += SolveGraphical_Click;

            graphicsBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            graphicalPanel.Controls.Add(solveGraphicalButton, 0, 0);
            graphicalPanel.Controls.Add(graphicsBox, 0, 1);
            graphicalPanel.SetColumnSpan(solveGraphicalButton, 2);

            graphicalTab.Controls.Add(graphicalPanel);

            tabControl.TabPages.Add(gomoryTab);
            tabControl.TabPages.Add(graphicalTab);
            this.Controls.Add(tabControl);
        }

        private void SetupExampleProblem()
        {
            problem = new LinearProgrammingProblem
            {
                ObjectiveCoefficients = new double[] { 1, 1, 0, 0 },
                OptimizationType = OptimizationType.Maximize,
                Constraints = new List<Constraint>
                {
                    new Constraint { Coefficients = new double[] { 1, 2, 1, 0 }, RightHandSide = 5 },
                    new Constraint { Coefficients = new double[] { 2, 1, 0, 1 }, RightHandSide = 6 }
                },
                VariableCount = 4,
                IntegerVariables = new bool[] { true, true, true, true }
            };
        }

        private void SolveGomory_Click(object sender, EventArgs e)
        {
            try
            {
                gomorySolver = new GomorySolver(problem);
                var solution = gomorySolver.Solve();
                DisplayGomorySolution(solution);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Gomory Method Error");
            }
        }

        private void SolveGraphical_Click(object sender, EventArgs e)
        {
            try
            {
                graphicalSolver = new GraphicalSolver(problem);
                var solution = graphicalSolver.Solve();
                DrawGraphicalSolution(solution);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Graphical Method Error");
            }
        }

        private void DisplayGomorySolution(GomorySolution solution)
        {
            var result = new System.Text.StringBuilder();
            result.AppendLine("=== GOMORY'S CUTTING PLANE METHOD ===\n");
            result.AppendLine("Problem: Maximize f = x1 + x2");
            result.AppendLine("Subject to:");
            result.AppendLine("  x1 + 2*x2 + x3 = 5");
            result.AppendLine("  2*x1 + x2 + x4 = 6");
            result.AppendLine("  x1, x2, x3, x4 ≥ 0 (Integer)\n");

            result.AppendLine("STEP-BY-STEP SOLUTION:\n");

            for (int i = 0; i < solution.Iterations.Count; i++)
            {
                var iteration = solution.Iterations[i];
                result.AppendLine($"--- ITERATION {i + 1} ---");
                result.AppendLine("Tableau:");
                result.AppendLine(FormatTableau(iteration.Tableau));
                result.AppendLine($"Basic Variables: {string.Join(", ", iteration.BasicVariables.Select((bv, idx) => $"x{bv + 1}")))}\n");

                if (iteration.IsInteger)
                {
                    result.AppendLine("✓ All basic variables are integer values.\n");
                }
                else
                {
                    result.AppendLine("✗ Some variables have fractional values.");
                    result.AppendLine($"Fractional constraint to add: {iteration.CuttingConstraint}\n");
                }
            }

            result.AppendLine("\n=== OPTIMAL SOLUTION ===");
            result.AppendLine($"Optimal Value: f = {solution.OptimalValue:F4}");
            result.AppendLine("Optimal Solution:");
            for (int i = 0; i < solution.OptimalSolution.Length; i++)
            {
                result.AppendLine($"  x{i + 1} = {solution.OptimalSolution[i]:F4}");
            }

            gomoryResultTextBox.Text = result.ToString();
        }

        private string FormatTableau(double[][] tableau)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var row in tableau)
            {
                sb.AppendLine(string.Join("\t", row.Select(v => v.ToString("F4"))));
            }
            return sb.ToString();
        }

        private void DrawGraphicalSolution(GraphicalSolution solution)
        {
            Bitmap bitmap = new Bitmap(graphicsBox.Width, graphicsBox.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Draw axes
                DrawAxes(g, bitmap.Width, bitmap.Height);

                // Draw constraint lines
                DrawConstraints(g, bitmap.Width, bitmap.Height, solution);

                // Draw feasible region
                DrawFeasibleRegion(g, solution);

                // Draw optimal point
                if (solution.OptimalPoint != null)
                {
                    var pointPixel = ConvertToPixel(solution.OptimalPoint[0], solution.OptimalPoint[1], bitmap.Width, bitmap.Height);
                    g.FillEllipse(Brushes.Red, pointPixel.X - 5, pointPixel.Y - 5, 10, 10);
                    g.DrawString($"Optimal: ({solution.OptimalPoint[0]:F2}, {solution.OptimalPoint[1]:F2})", 
                        new Font("Arial", 10, FontStyle.Bold), Brushes.Red, pointPixel.X + 10, pointPixel.Y);
                }
            }

            graphicsBox.Image = bitmap;
        }

        private void DrawAxes(Graphics g, int width, int height)
        {
            int originX = 50;
            int originY = height - 50;

            g.DrawLine(Pens.Black, originX, originY, width - 20, originY);
            g.DrawLine(Pens.Black, originX, 20, originX, originY);

            g.DrawString("x1", new Font("Arial", 12), Brushes.Black, width - 40, originY + 5);
            g.DrawString("x2", new Font("Arial", 12), Brushes.Black, originX - 30, 5);

            for (int i = 0; i <= 10; i++)
            {
                int x = originX + i * 10;
                g.DrawLine(Pens.LightGray, x, originY - 5, x, originY + 5);
                g.DrawString(i.ToString(), new Font("Arial", 8), Brushes.Gray, x - 5, originY + 10);

                int y = originY - i * 10;
                g.DrawLine(Pens.LightGray, originX - 5, y, originX + 5, y);
                g.DrawString(i.ToString(), new Font("Arial", 8), Brushes.Gray, originX - 20, y - 5);
            }
        }

        private void DrawConstraints(Graphics g, int width, int height, GraphicalSolution solution)
        {
            int originX = 50;
            int originY = height - 50;
            
            for (int i = 0; i < solution.ConstraintLines.Count; i++)
            {
                var line = solution.ConstraintLines[i];
                var color = new Color[] { Color.Blue, Color.Green, Color.Purple, Color.Orange }[i % 4];
                
                var p1 = ConvertToPixel(0, line.GetY(0), width, height);
                var p2 = ConvertToPixel(10, line.GetY(10), width, height);

                g.DrawLine(new Pen(color, 2), p1, p2);
            }
        }

        private void DrawFeasibleRegion(Graphics g, GraphicalSolution solution)
        {
            if (solution.FeasibleRegionVertices.Count > 2)
            {
                var vertices = solution.FeasibleRegionVertices
                    .Select(v => ConvertToPixel(v[0], v[1], graphicsBox.Width, graphicsBox.Height))
                    .ToArray();

                g.FillPolygon(new SolidBrush(Color.FromArgb(100, 173, 216, 230)), vertices);
                g.DrawPolygon(Pens.Blue, vertices);
            }
        }

        private Point ConvertToPixel(double x, double y, int width, int height)
        {
            int originX = 50;
            int originY = height - 50;
            int pixelX = originX + (int)(x * 10);
            int pixelY = originY - (int)(y * 10);
            return new Point(pixelX, pixelY);
        }
    }

    public enum OptimizationType { Maximize, Minimize }

    public class LinearProgrammingProblem
    {
        public double[] ObjectiveCoefficients { get; set; }
        public OptimizationType OptimizationType { get; set; }
        public List<Constraint> Constraints { get; set; }
        public int VariableCount { get; set; }
        public bool[] IntegerVariables { get; set; }
    }

    public class Constraint
    {
        public double[] Coefficients { get; set; }
        public double RightHandSide { get; set; }
    }

    public class GomorySolver
    {
        private LinearProgrammingProblem problem;
        private double[][] tableau;
        private List<int> basicVariables;

        public GomorySolver(LinearProgrammingProblem problem)
        {
            this.problem = problem;
            InitializeTableau();
        }

        private void InitializeTableau()
        {
            int numConstraints = problem.Constraints.Count;
            int numVariables = problem.VariableCount;
            tableau = new double[numConstraints + 1][];

            for (int i = 0; i <= numConstraints; i++)
            {
                tableau[i] = new double[numVariables + numConstraints + 1];
            }

            for (int i = 0; i < numConstraints; i++)
            {
                for (int j = 0; j < numVariables; j++)
                {
                    tableau[i][j] = problem.Constraints[i].Coefficients[j];
                }
                tableau[i][numVariables + numConstraints] = problem.Constraints[i].RightHandSide;
            }

            for (int j = 0; j < problem.ObjectiveCoefficients.Length; j++)
            {
                tableau[numConstraints][j] = problem.OptimizationType == OptimizationType.Maximize 
                    ? -problem.ObjectiveCoefficients[j] 
                    : problem.ObjectiveCoefficients[j];
            }

            basicVariables = new List<int>();
            for (int i = 0; i < numConstraints; i++)
            {
                basicVariables.Add(problem.VariableCount + i);
            }
        }

        public GomorySolution Solve()
        {
            var solution = new GomorySolution();
            int maxIterations = 100;
            int iteration = 0;

            while (iteration < maxIterations)
            {
                solution.Iterations.Add(new IterationData
                {
                    Tableau = CopyTableau(tableau),
                    BasicVariables = new List<int>(basicVariables)
                });

                if (IsIntegerFeasible())
                {
                    solution.Iterations[iteration].IsInteger = true;
                    break;
                }

                int fractionalRow = FindFractionalRow();
                if (fractionalRow == -1) break;

                AddCuttingConstraint(fractionalRow, solution.Iterations[iteration]);

                iteration++;
            }

            ExtractSolution(solution);
            return solution;
        }

        private bool IsIntegerFeasible()
        {
            int numConstraints = problem.Constraints.Count;
            for (int i = 0; i < numConstraints; i++)
            {
                double value = tableau[i][problem.VariableCount + problem.Constraints.Count];
                if (Math.Abs(value - Math.Round(value)) > 1e-6 && problem.IntegerVariables[basicVariables[i]])
                {
                    return false;
                }
            }
            return true;
        }

        private int FindFractionalRow()
        {
            int numConstraints = problem.Constraints.Count;
            for (int i = 0; i < numConstraints; i++)
            {
                double value = tableau[i][problem.VariableCount + numConstraints];
                double frac = value - Math.Floor(value);
                if (frac > 1e-6 && frac < 1 - 1e-6 && problem.IntegerVariables[basicVariables[i]])
                {
                    return i;
                }
            }
            return -1;
        }

        private void AddCuttingConstraint(int row, IterationData iteration)
        {
            int numVariables = problem.VariableCount + problem.Constraints.Count;
            var constraint = new double[numVariables + 1];

            for (int j = 0; j <= numVariables; j++)
            {
                double coeff = tableau[row][j];
                double frac = coeff - Math.Floor(coeff);
                constraint[j] = frac;
            }

            iteration.CuttingConstraint = $"Cutting constraint: {string.Join(" + ", constraint.Take(numVariables).Select((c, i) => $"{c:F2}*s{i}"))}";
        }

        private double[][] CopyTableau()
        {
            return tableau.Select(row => (double[])row.Clone()).ToArray();
        }

        private void ExtractSolution(GomorySolution solution)
        {
            int numConstraints = problem.Constraints.Count;
            solution.OptimalSolution = new double[problem.VariableCount];

            for (int i = 0; i < problem.VariableCount; i++)
            {
                int basicIndex = basicVariables.IndexOf(i);
                if (basicIndex >= 0)
                {
                    solution.OptimalSolution[i] = tableau[basicIndex][problem.VariableCount + numConstraints];
                }
                else
                {
                    solution.OptimalSolution[i] = 0;
                }
            }

            solution.OptimalValue = -tableau[numConstraints][problem.VariableCount + numConstraints];
        }
    }

    public class GomorySolution
    {
        public List<IterationData> Iterations { get; set; } = new List<IterationData>();
        public double[] OptimalSolution { get; set; }
        public double OptimalValue { get; set; }
    }

    public class IterationData
    {
        public double[][] Tableau { get; set; }
        public List<int> BasicVariables { get; set; }
        public bool IsInteger { get; set; }
        public string CuttingConstraint { get; set; }
    }

    public class GraphicalSolver
    {
        private LinearProgrammingProblem problem;

        public GraphicalSolver(LinearProgrammingProblem problem)
        {
            this.problem = problem;
        }

        public GraphicalSolution Solve()
        {
            var solution = new GraphicalSolution();

            // Create constraint lines for x1 and x2 only
            for (int i = 0; i < problem.Constraints.Count; i++)
            {
                var constraint = problem.Constraints[i];
                var line = new ConstraintLine(constraint.Coefficients[0], constraint.Coefficients[1], constraint.RightHandSide);
                solution.ConstraintLines.Add(line);
            }

            // Find feasible region vertices
            solution.FeasibleRegionVertices = FindFeasibleVertices(solution.ConstraintLines);

            // Find optimal point
            solution.OptimalPoint = FindOptimalPoint(solution.FeasibleRegionVertices);

            return solution;
        }

        private List<double[]> FindFeasibleVertices(List<ConstraintLine> lines)
        {
            var vertices = new List<double[]>();
            vertices.Add(new double[] { 0, 0 });

            for (int i = 0; i < lines.Count; i++)
            {
                vertices.Add(new double[] { lines[i].GetX(0), 0 });
                vertices.Add(new double[] { 0, lines[i].GetY(0) });

                for (int j = i + 1; j < lines.Count; j++)
                {
                    var intersection = FindIntersection(lines[i], lines[j]);
                    if (intersection != null && intersection[0] >= -0.001 && intersection[1] >= -0.001)
                    {
                        vertices.Add(intersection);
                    }
                }
            }

            return vertices.Distinct(new PointComparer()).ToList();
        }

        private double[] FindIntersection(ConstraintLine line1, ConstraintLine line2)
        {
            double det = line1.A * line2.B - line1.B * line2.A;
            if (Math.Abs(det) < 1e-10) return null;

            double x = (line1.C * line2.B - line2.C * line1.B) / det;
            double y = (line1.A * line2.C - line2.A * line1.C) / det;

            return new double[] { x, y };
        }

        private double[] FindOptimalPoint(List<double[]> vertices)
        {
            double maxValue = double.MinValue;
            double[] optimalPoint = null;

            foreach (var vertex in vertices)
            {
                double value = problem.ObjectiveCoefficients[0] * vertex[0] + problem.ObjectiveCoefficients[1] * vertex[1];
                if (problem.OptimizationType == OptimizationType.Minimize)
                    value = -value;

                if (value > maxValue)
                {
                    maxValue = value;
                    optimalPoint = vertex;
                }
            }

            return optimalPoint;
        }
    }

    public class GraphicalSolution
    {
        public List<ConstraintLine> ConstraintLines { get; set; } = new List<ConstraintLine>();
        public List<double[]> FeasibleRegionVertices { get; set; } = new List<double[]>();
        public double[] OptimalPoint { get; set; }
    }

    public class ConstraintLine
    {
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }

        public ConstraintLine(double a, double b, double c)
        {
            A = a;
            B = b;
            C = c;
        }

        public double GetY(double x) => (C - A * x) / B;
        public double GetX(double y) => (C - B * y) / A;
    }

    public class PointComparer : IEqualityComparer<double[]>
    {
        public bool Equals(double[] x, double[] y) => x[0].Equals(y[0]) && x[1].Equals(y[1]);
        public int GetHashCode(double[] obj) => obj[0].GetHashCode() ^ obj[1].GetHashCode();
    }