using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MauiApp.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private string _inputDisplay = "0";
    private string _resultDisplay = "32";
    private string _sourceUnit = "Celsius °C";
    private string _targetUnit = "Fahrenheit °F";

    public MainViewModel()
    {
        DigitCommand = new Command<string>(OnDigit);
        ClearCommand = new Command(OnClear);
        DecimalCommand = new Command(OnDecimal);
        ToggleSignCommand = new Command(OnToggleSign);
        BackspaceCommand = new Command(OnBackspace);
    }

    public List<string> Units { get; } = new()
    {
        "Celsius °C",
        "Fahrenheit °F",
        "Kelvin K"
    };

    public string InputDisplay
    {
        get => _inputDisplay;
        private set { if (_inputDisplay != value) { _inputDisplay = value; OnPropertyChanged(); } }
    }

    public string ResultDisplay
    {
        get => _resultDisplay;
        private set { if (_resultDisplay != value) { _resultDisplay = value; OnPropertyChanged(); } }
    }

    public string SourceUnit
    {
        get => _sourceUnit;
        set
        {
            if (_sourceUnit != value && value is not null)
            {
                _sourceUnit = value;
                OnPropertyChanged();
                ConvertTemperature();
            }
        }
    }

    public string TargetUnit
    {
        get => _targetUnit;
        set
        {
            if (_targetUnit != value && value is not null)
            {
                _targetUnit = value;
                OnPropertyChanged();
                ConvertTemperature();
            }
        }
    }

    public ICommand DigitCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand DecimalCommand { get; }
    public ICommand ToggleSignCommand { get; }
    public ICommand BackspaceCommand { get; }

    private double Current => double.TryParse(InputDisplay, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 0;

    private static string Format(double v)
        => double.IsNaN(v) || double.IsInfinity(v) ? "Error" : Math.Round(v, 4).ToString("G12", CultureInfo.InvariantCulture);

    private void OnDigit(string? d)
    {
        if (d is null) return;
        if (InputDisplay == "Error") OnClear();

        if (InputDisplay == "0")
        {
            InputDisplay = d == "00" ? "0" : d;
        }
        else if (InputDisplay == "-0")
        {
            InputDisplay = d == "00" ? "-0" : "-" + d;
        }
        else if (InputDisplay.Length + d.Length <= 15)
        {
            InputDisplay += d;
        }

        ConvertTemperature();
    }

    private void OnDecimal()
    {
        if (InputDisplay == "Error") OnClear();
        if (!InputDisplay.Contains('.'))
        {
            InputDisplay += ".";
            ConvertTemperature();
        }
    }

    private void OnClear()
    {
        InputDisplay = "0";
        ConvertTemperature();
    }

    private void OnToggleSign()
    {
        if (InputDisplay == "Error" || InputDisplay == "0") return;
        InputDisplay = InputDisplay.StartsWith('-') ? InputDisplay[1..] : "-" + InputDisplay;
        ConvertTemperature();
    }

    private void OnBackspace()
    {
        if (InputDisplay == "Error") return;
        var s = InputDisplay[..^1];
        InputDisplay = s is "" or "-" ? "0" : s;
        ConvertTemperature();
    }

    private void ConvertTemperature()
    {
        double value = Current;

        // 1. Convertir unidad origen a Celsius
        double celsius = SourceUnit switch
        {
            "Fahrenheit °F" => (value - 32.0) * 5.0 / 9.0,
            "Kelvin K" => value - 273.15,
            _ => value
        };

        // 2. Convertir de Celsius a unidad destino
        double result = TargetUnit switch
        {
            "Fahrenheit °F" => (celsius * 9.0 / 5.0) + 32.0,
            "Kelvin K" => celsius + 273.15,
            _ => celsius
        };

        ResultDisplay = Format(result);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}