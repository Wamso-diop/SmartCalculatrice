using System.Globalization;
using SmartCalculator.Models;

namespace SmartCalculator.Services;

public class CalculatorEngine
{
    private readonly List<string> tokens = new();
    private string currentNumber = "";
    private bool justEvaluated = false;

    public AngleMode Angle { get; set; } = AngleMode.Deg;
    public string? ErrorMessage { get; private set; }

    private static readonly HashSet<string> Operators = new() { "+", "−", "×", "÷", "^" };

    // ---------- Affichage ----------

    public string ExpressionDisplay
    {
        get
        {
            var parts = new List<string>(tokens);
            if (currentNumber.Length > 0) parts.Add(currentNumber);
            return parts.Count == 0 ? "" : string.Join(" ", parts.Select(FormatToken));
        }
    }

    public string CurrentEntryDisplay
    {
        get
        {
            if (currentNumber.Length > 0) return FormatDisplayNumber(currentNumber);
            if (tokens.Count > 0 && IsNumberToken(tokens[^1])) return FormatDisplayNumber(tokens[^1]);
            return "0";
        }
    }

    private string FormatToken(string t) => IsNumberToken(t) ? FormatDisplayNumber(t) : t;

    private static string FormatDisplayNumber(string raw)
    {
        if (!double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out double v))
            return raw;

        string sign = v < 0 ? "-" : "";
        v = Math.Abs(v);
        string s = v.ToString("0.##########", CultureInfo.InvariantCulture);
        var parts = s.Split('.');
        string intPart = System.Text.RegularExpressions.Regex.Replace(
            parts[0], @"\B(?=(\d{3})+(?!\d))", " ");
        return sign + intPart + (parts.Length > 1 ? "," + parts[1] : "");
    }

    // ---------- Saisie ----------

    public void PressDigit(string d)
    {
        ResetIfJustEvaluated();
        ErrorMessage = null;
        if (d == "0" && currentNumber == "0") return;
        currentNumber = currentNumber == "0" ? d : currentNumber + d;
    }

    public void PressDecimal()
    {
        ResetIfJustEvaluated();
        if (!currentNumber.Contains('.'))
            currentNumber += currentNumber.Length == 0 ? "0." : ".";
    }

    public void PressOperator(string op)
    {
        justEvaluated = false;
        ErrorMessage = null;
        if (currentNumber.Length == 0 && tokens.Count == 0) return;

        if (currentNumber.Length > 0)
        {
            CommitCurrentNumber();
        }
        else if (tokens.Count > 0 && Operators.Contains(tokens[^1]))
        {
            tokens[^1] = op;
            return;
        }
        tokens.Add(op);
    }

    public void PressOpenParen()
    {
        ResetIfJustEvaluated();
        CommitCurrentNumber();
        tokens.Add("(");
    }

    public void PressCloseParen()
    {
        CommitCurrentNumber();
        tokens.Add(")");
    }

    public void PressPercent()
    {
        CommitCurrentNumber();
        if (tokens.Count == 0 || !IsNumberToken(tokens[^1])) return;

        int idx = tokens.Count - 1;
        double b = ParseD(tokens[idx]);
        double result = b / 100.0;

        if (idx >= 2 && IsNumberToken(tokens[idx - 2]) && (tokens[idx - 1] == "+" || tokens[idx - 1] == "−"))
        {
            double a = ParseD(tokens[idx - 2]);
            result = a * (b / 100.0);
        }
        tokens[idx] = FormatD(result);
    }

    public void PressPlusMinus()
    {
        if (currentNumber.Length > 0)
        {
            currentNumber = currentNumber.StartsWith("-") ? currentNumber[1..] : "-" + currentNumber;
            return;
        }
        CommitCurrentNumber();
        if (tokens.Count == 0 || !IsNumberToken(tokens[^1])) return;
        tokens[^1] = FormatD(-ParseD(tokens[^1]));
    }

    public void PressFunction(string func)
    {
        CommitCurrentNumber();
        if (tokens.Count == 0 || !IsNumberToken(tokens[^1])) return;

        double x = ParseD(tokens[^1]);
        double? result = ComputeFunction(func, x, out string? error);
        if (error != null) { ErrorMessage = error; return; }
        tokens[^1] = FormatD(result!.Value);
    }

    public void PressConstant(string name)
    {
        ResetIfJustEvaluated();
        CommitCurrentNumber();
        double v = name == "π" ? Math.PI : Math.E;
        tokens.Add(FormatD(v));
    }

    public void PressAllClear()
    {
        tokens.Clear();
        currentNumber = "";
        ErrorMessage = null;
        justEvaluated = false;
    }

    public void PressClearEntry()
    {
        currentNumber = "";
        ErrorMessage = null;
    }

    public void PressBackspace()
    {
        if (currentNumber.Length > 0) { currentNumber = currentNumber[..^1]; return; }
        if (tokens.Count > 0) tokens.RemoveAt(tokens.Count - 1);
    }

    public (bool success, string expressionUsed, string result) PressEquals()
    {
        CommitCurrentNumber();
        string usedExpression = ExpressionDisplay;

        if (tokens.Count == 0) return (false, usedExpression, "0");

        try
        {
            double value = EvaluateTokens(new List<string>(tokens));
            if (double.IsNaN(value) || double.IsInfinity(value))
                return (false, usedExpression, "Résultat trop grand");

            string formatted = FormatD(value);
            tokens.Clear();
            tokens.Add(formatted);
            currentNumber = "";
            justEvaluated = true;
            return (true, usedExpression, FormatDisplayNumber(formatted));
        }
        catch (DivideByZeroException)
        {
            return (false, usedExpression, "Division impossible");
        }
        catch (CalcException ex)
        {
            return (false, usedExpression, ex.Message);
        }
        catch
        {
            return (false, usedExpression, "Expression invalide");
        }
    }

    // ---------- Interne ----------

    private void ResetIfJustEvaluated()
    {
        if (!justEvaluated) return;
        tokens.Clear();
        currentNumber = "";
        justEvaluated = false;
    }

    private void CommitCurrentNumber()
    {
        if (currentNumber.Length == 0) return;
        tokens.Add(currentNumber);
        currentNumber = "";
    }

    private static bool IsNumberToken(string t) =>
        double.TryParse(t, NumberStyles.Any, CultureInfo.InvariantCulture, out _);

    private static double ParseD(string t) => double.Parse(t, CultureInfo.InvariantCulture);

    private static string FormatD(double v) => v.ToString("R", CultureInfo.InvariantCulture);

    private double ToRadians(double deg) => Angle == AngleMode.Deg ? deg * Math.PI / 180.0 : deg;
    private double FromRadians(double rad) => Angle == AngleMode.Deg ? rad * 180.0 / Math.PI : rad;

    private double? ComputeFunction(string func, double x, out string? error)
    {
        error = null;
        switch (func)
        {
            case "√": if (x < 0) { error = "Racine négative"; return null; } return Math.Sqrt(x);
            case "x²": return x * x;
            case "1/x": if (x == 0) { error = "Division impossible"; return null; } return 1.0 / x;
            case "|x|": return Math.Abs(x);
            case "n!":
                if (x < 0 || x != Math.Floor(x) || x > 170) { error = "Factorielle invalide"; return null; }
                double r = 1; for (int i = 2; i <= (int)x; i++) r *= i; return r;
            case "sin": return Math.Sin(ToRadians(x));
            case "cos": return Math.Cos(ToRadians(x));
            case "tan": return Math.Tan(ToRadians(x));
            case "sin⁻¹": return FromRadians(Math.Asin(x));
            case "cos⁻¹": return FromRadians(Math.Acos(x));
            case "tan⁻¹": return FromRadians(Math.Atan(x));
            case "log": if (x <= 0) { error = "Logarithme indéfini"; return null; } return Math.Log10(x);
            case "ln": if (x <= 0) { error = "Logarithme indéfini"; return null; } return Math.Log(x);
            case "10ˣ": return Math.Pow(10, x);
            case "eˣ": return Math.Exp(x);
            default: return x;
        }
    }

    private static int Precedence(string op) => op switch
    {
        "^" => 3,
        "×" or "÷" => 2,
        "+" or "−" => 1,
        _ => 0
    };

    private double EvaluateTokens(List<string> input)
    {
        var output = new List<string>();
        var opStack = new Stack<string>();

        foreach (var t in input)
        {
            if (IsNumberToken(t)) output.Add(t);
            else if (t == "(") opStack.Push(t);
            else if (t == ")")
            {
                while (opStack.Count > 0 && opStack.Peek() != "(") output.Add(opStack.Pop());
                if (opStack.Count == 0) throw new CalcException("Parenthèses invalides");
                opStack.Pop();
            }
            else
            {
                while (opStack.Count > 0 && opStack.Peek() != "(" &&
                       (Precedence(opStack.Peek()) > Precedence(t) ||
                        (Precedence(opStack.Peek()) == Precedence(t) && t != "^")))
                    output.Add(opStack.Pop());
                opStack.Push(t);
            }
        }
        while (opStack.Count > 0)
        {
            var op = opStack.Pop();
            if (op == "(") throw new CalcException("Parenthèses invalides");
            output.Add(op);
        }

        var stack = new Stack<double>();
        foreach (var t in output)
        {
            if (IsNumberToken(t)) { stack.Push(ParseD(t)); continue; }

            if (stack.Count < 2) throw new CalcException("Expression invalide");
            double b = stack.Pop();
            double a = stack.Pop();
            double res = t switch
            {
                "+" => a + b,
                "−" => a - b,
                "×" => a * b,
                "÷" => b == 0 ? throw new DivideByZeroException() : a / b,
                "^" => Math.Pow(a, b),
                _ => throw new CalcException("Opérateur inconnu")
            };
            stack.Push(res);
        }

        if (stack.Count != 1) throw new CalcException("Expression invalide");
        return stack.Pop();
    }

    public (string Top, string Bottom) GetDisplayState()
    {
        var all = new List<string>(tokens);
        if (currentNumber.Length > 0) all.Add(currentNumber);

        if (ErrorMessage != null)
        {
            string topOnError = string.Join(" ", all.Select(FormatToken));
            return (topOnError, ErrorMessage);
        }

        bool hasOperator = all.Any(t => Operators.Contains(t));

        if (!hasOperator)
        {
            // Pas encore d'opérateur : comportement simple, un seul nombre affiché en bas
            string bottomSimple = currentNumber.Length > 0
                ? FormatDisplayNumber(currentNumber)
                : (tokens.Count > 0 && IsNumberToken(tokens[^1]) ? FormatDisplayNumber(tokens[^1]) : "0");
            return ("", bottomSimple);
        }

        // Au moins un opérateur : expression complète en haut, aperçu du résultat en bas
        string topExpr = string.Join(" ", all.Select(FormatToken));

        // On retire les éléments non évaluables en fin de liste (opérateur ou parenthèse ouvrante en attente)
        var evalList = new List<string>(all);
        while (evalList.Count > 0 && (Operators.Contains(evalList[^1]) || evalList[^1] == "("))
            evalList.RemoveAt(evalList.Count - 1);

        if (evalList.Count == 0) return (topExpr, "0");

        try
        {
            double value = EvaluateTokens(evalList);
            if (double.IsNaN(value) || double.IsInfinity(value)) return (topExpr, "");
            return (topExpr, FormatDisplayNumber(FormatD(value)));
        }
        catch
        {
            // Parenthèse non fermée, expression encore incomplète, etc. : on n'affiche rien de faux
            return (topExpr, "");
        }
    }
}



public class CalcException : Exception
{
    public CalcException(string message) : base(message) { }
}