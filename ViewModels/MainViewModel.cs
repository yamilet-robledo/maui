using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MauiApp.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private string _display = "0";
    private string _expression = "";
    private double? _accumulator;
    private string? _operator;
    private bool _resetOnNextDigit;

    public MainViewModel()
    {
        DigitCommand = new Command<string>(OnDigit);
        OperatorCommand = new Command<string>(OnOperator);
        EqualsCommand = new Command(OnEquals);
        ClearCommand = new Command(OnClear);
        DecimalCommand = new Command(OnDecimal);
        ToggleSignCommand = new Command(OnToggleSign);
        PercentCommand = new Command(OnPercent);
        BackspaceCommand = new Command(OnBackspace);
    }

    public string Display
    {
        get => _display;
        private set { if (_display != value) { _display = value; OnPropertyChanged(); } }
    }

    public string Expression
    {
        get => _expression;
        private set { if (_expression != value) { _expression = value; OnPropertyChanged(); } }
    }

    public ICommand DigitCommand { get; }
    public ICommand OperatorCommand { get; }
    public ICommand EqualsCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand DecimalCommand { get; }
    public ICommand ToggleSignCommand { get; }
    public ICommand PercentCommand { get; }
    public ICommand BackspaceCommand { get; }

    private double Current => double.Parse(Display, CultureInfo.InvariantCulture);

    private static string Format(double v)
        => double.IsNaN(v) || double.IsInfinity(v) ? "Error" : v.ToString("G12", CultureInfo.InvariantCulture);

    private void OnDigit(string? d)
    {
        if (d is null) return;
        if (Display == "Error") OnClear();
        if (_resetOnNextDigit || Display == "0") { Display = d; _resetOnNextDigit = false; }
        else if (Display.Length < 15) Display += d;
    }

    private void OnDecimal()
    {
        if (Display == "Error") OnClear();
        if (_resetOnNextDigit) { Display = "0."; _resetOnNextDigit = false; return; }
        if (!Display.Contains('.')) Display += ".";
    }

    private void OnOperator(string? op)
    {
        if (op is null || Display == "Error") return;
        if (_operator is not null && !_resetOnNextDigit) Compute();
        else _accumulator = Current;
        _operator = op;
        Expression = $"{Format(_accumulator ?? 0)} {op}";
        _resetOnNextDigit = true;
    }

    private void OnEquals()
    {
        if (_operator is null || _accumulator is null || Display == "Error") return;
        var left = Format(_accumulator.Value);
        var right = Display;
        var op = _operator;
        Compute();
        Expression = $"{left} {op} {right} =";
        _operator = null;
        _resetOnNextDigit = true;
    }

    private void Compute()
    {
        double a = _accumulator ?? 0, b = Current;
        double r = _operator switch
        {
            "+" => a + b,
            "−" => a - b,
            "×" => a * b,
            "÷" => b == 0 ? double.NaN : a / b,
            _ => b
        };
        Display = Format(r);
        _accumulator = double.IsNaN(r) ? null : r;
    }

    private void OnClear()
    {
        Display = "0";
        Expression = "";
        _accumulator = null;
        _operator = null;
        _resetOnNextDigit = false;
    }

    private void OnToggleSign()
    {
        if (Display == "Error" || Display == "0") return;
        Display = Display.StartsWith('-') ? Display[1..] : "-" + Display;
    }

    private void OnPercent()
    {
        if (Display == "Error") return;
        Display = Format(Current / 100);
        _resetOnNextDigit = true;
    }

    private void OnBackspace()
    {
        if (Display == "Error" || _resetOnNextDigit) return;
        var s = Display[..^1];
        Display = s is "" or "-" ? "0" : s;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
