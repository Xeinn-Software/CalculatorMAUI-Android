using System.Globalization;

namespace CalculatorMAUI
{
    public partial class MainPage : ContentPage
    {
        private string current = "0";
        private string expression = "";
        private double stored = 0;
        private string op = "";
        private bool newInput = true;
        private bool powMode = false;
        private double powBase = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void UpdateDisplay()
        {
            if (double.TryParse(current, NumberStyles.Any, CultureInfo.InvariantCulture, out double v))
            {
                if (Math.Abs(v) >= 1e14 || (Math.Abs(v) < 1e-6 && v != 0))
                    lblDisplay.Text = v.ToString("G10", CultureInfo.InvariantCulture);
                else
                    lblDisplay.Text = current;
            }
            else lblDisplay.Text = current;

            lblExpression.Text = expression;
            lblDisplay.FontSize = lblDisplay.Text.Length > 14 ? 24 : lblDisplay.Text.Length > 10 ? 32 : 42;
        }

        private void Err(string msg = "Error")
        {
            current = msg; expression = ""; op = ""; newInput = true; powMode = false;
            UpdateDisplay();
        }

        private void NumClicked(object sender, EventArgs e)
        {
            string d = ((Button)sender).Text;
            if (newInput) { current = d; newInput = false; }
            else current = current == "0" ? d : current + d;
            UpdateDisplay();
        }

        private void DotClicked(object sender, EventArgs e)
        {
            if (newInput) { current = "0."; newInput = false; }
            else if (!current.Contains('.')) current += ".";
            UpdateDisplay();
        }

        private void AcClicked(object sender, EventArgs e)
        {
            current = "0"; expression = ""; op = ""; stored = 0; newInput = true; powMode = false;
            UpdateDisplay();
        }

        private void SignClicked(object sender, EventArgs e)
        {
            if (current.StartsWith("-")) current = current[1..];
            else if (current != "0") current = "-" + current;
            UpdateDisplay();
        }

        private void BackClicked(object sender, EventArgs e)
        {
            if (newInput) return;
            current = current.Length > 1 ? current[..^1] : "0";
            UpdateDisplay();
        }

        private void ParenClicked(object sender, EventArgs e) { }

        private void OpClicked(object sender, EventArgs e)
        {
            string o = ((Button)sender).CommandParameter.ToString()!;
            if (!newInput && op != "") Calculate();
            if (double.TryParse(current, NumberStyles.Any, CultureInfo.InvariantCulture, out double v))
                stored = v;
            op = o;
            expression = $"{Fmt(stored)} {o}";
            newInput = true;
            UpdateDisplay();
        }

        private void EqClicked(object sender, EventArgs e)
        {
            if (powMode)
            {
                if (!double.TryParse(current, NumberStyles.Any, CultureInfo.InvariantCulture, out double exp))
                { Err(); return; }
                double res = Math.Pow(powBase, exp);
                expression = $"{Fmt(powBase)} ^ {Fmt(exp)} =";
                current = FmtResult(res);
                newInput = true; powMode = false;
                UpdateDisplay(); return;
            }
            Calculate();
        }

        private void Calculate()
        {
            if (op == "" || newInput) return;
            if (!double.TryParse(current, NumberStyles.Any, CultureInfo.InvariantCulture, out double right))
            { Err(); return; }

            double result = op switch
            {
                "+"  => stored + right,
                "−"  => stored - right,
                "×"  => stored * right,
                "÷"  => right == 0 ? double.NaN : stored / right,
                _    => right
            };

            if (double.IsNaN(result))      { Err("Div/0"); return; }
            if (double.IsInfinity(result)) { Err("Overflow"); return; }

            expression = $"{Fmt(stored)} {op} {Fmt(right)} =";
            current = FmtResult(result);
            stored = result; op = ""; newInput = true;
            UpdateDisplay();
        }

        private void FnClicked(object sender, EventArgs e)
        {
            string fn = ((Button)sender).CommandParameter.ToString()!;
            if (!double.TryParse(current, NumberStyles.Any, CultureInfo.InvariantCulture, out double v))
            { Err(); return; }

            switch (fn)
            {
                case "sin":
                    expression = $"sin({Fmt(v)})";
                    current = FmtResult(Math.Sin(v * Math.PI / 180)); break;
                case "cos":
                    expression = $"cos({Fmt(v)})";
                    current = FmtResult(Math.Cos(v * Math.PI / 180)); break;
                case "tan":
                    expression = $"tan({Fmt(v)})";
                    double t = Math.Tan(v * Math.PI / 180);
                    if (double.IsInfinity(t)) { Err("Undefined"); return; }
                    current = FmtResult(t); break;
                case "log":
                    if (v <= 0) { Err("Domain error"); return; }
                    expression = $"log({Fmt(v)})";
                    current = FmtResult(Math.Log10(v)); break;
                case "ln":
                    if (v <= 0) { Err("Domain error"); return; }
                    expression = $"ln({Fmt(v)})";
                    current = FmtResult(Math.Log(v)); break;
                case "sqrt":
                    if (v < 0) { Err("Domain error"); return; }
                    expression = $"√{Fmt(v)}";
                    current = FmtResult(Math.Sqrt(v)); break;
                case "sq":
                    expression = $"({Fmt(v)})²";
                    current = FmtResult(v * v); break;
                case "pow":
                    powBase = v; powMode = true;
                    expression = $"{Fmt(v)} ^"; newInput = true;
                    UpdateDisplay(); return;
                case "pi":
                    current = Math.PI.ToString("G10", CultureInfo.InvariantCulture);
                    newInput = false; UpdateDisplay(); return;
                case "inv":
                    if (v == 0) { Err("Div/0"); return; }
                    expression = $"1/{Fmt(v)}";
                    current = FmtResult(1.0 / v); break;
            }

            newInput = true;
            UpdateDisplay();
        }

        private static string Fmt(double v) => v.ToString("G10", CultureInfo.InvariantCulture);

        private static string FmtResult(double v)
        {
            if (v == Math.Floor(v) && Math.Abs(v) < 1e15)
                return ((long)v).ToString(CultureInfo.InvariantCulture);
            return v.ToString("G10", CultureInfo.InvariantCulture);
        }
    }
}
