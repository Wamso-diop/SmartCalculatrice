using SmartCalculator.Services;

namespace SmartCalculator;

public partial class MainPage : ContentPage
{
    private readonly CalculatorEngine engine = new();
    private readonly MemoryService memory = new();
    private readonly HistoryService history = new();
    private bool? isLandscapeCache = null;

    public MainPage()
    {
        InitializeComponent();
        HistoryList.ItemsSource = history.Entries;
        Refresh();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width <= 0 || height <= 0) return;

        bool isLandscape = width > height;
        if (isLandscapeCache == isLandscape) return;
        isLandscapeCache = isLandscape;

        if (isLandscape) ApplyLandscapeLayout();
        else ApplyPortraitLayout();
    }

    // ---------- Déplace réellement les blocs dans la bonne grille ----------
    private void PlaceIn(Grid target, View view, int row, int col, int rowSpan = 1, int colSpan = 1)
    {
        if (view.Parent is Layout currentParent && currentParent != target)
        {
            if (currentParent is Grid g) g.Children.Remove(view);
        }
        if (!target.Children.Contains(view))
            target.Children.Add(view);

        Grid.SetRow(view, row);
        Grid.SetColumn(view, col);
        Grid.SetRowSpan(view, rowSpan);
        Grid.SetColumnSpan(view, colSpan);
    }

    private void ApplyPortraitLayout()
    {
        LandscapeGrid.IsVisible = false;

        PlaceIn(PortraitGrid, HeaderBar, 0, 0);
        PlaceIn(PortraitGrid, DisplayBorder, 1, 0);
        PlaceIn(PortraitGrid, ScientificPanel, 2, 0);
        PlaceIn(PortraitGrid, MemoryRow, 3, 0);
        PlaceIn(PortraitGrid, KeypadGrid, 4, 0);

        PortraitGrid.IsVisible = true;
    }

    private void ApplyLandscapeLayout()
    {
        PortraitGrid.IsVisible = false;

        PlaceIn(LandscapeGrid, HeaderBar, 0, 0, colSpan: 2);
        PlaceIn(LandscapeGrid, ScientificPanel, 1, 0, rowSpan: 3);
        PlaceIn(LandscapeGrid, MemoryRow, 4, 0);
        PlaceIn(LandscapeGrid, DisplayBorder, 1, 1);
        PlaceIn(LandscapeGrid, KeypadGrid, 2, 1, rowSpan: 3);

        LandscapeGrid.IsVisible = true;
    }

    private void Refresh()
    {
        var (top, bottom) = engine.GetDisplayState();
        ExpressionLabel.Text = string.IsNullOrEmpty(top) ? " " : top;
        ResultLabel.Text = string.IsNullOrEmpty(bottom) ? "0" : bottom;
        MemoryIndicator.IsVisible = memory.HasValue;
    }

    // ---------- Clavier ----------

    private void OnDigitClicked(object sender, EventArgs e)
    {
        engine.PressDigit(((Button)sender).Text);
        Refresh();
    }

    private void OnDecimalClicked(object sender, EventArgs e)
    {
        engine.PressDecimal();
        Refresh();
    }

    private void OnOperatorClicked(object sender, EventArgs e)
    {
        engine.PressOperator(((Button)sender).Text);
        Refresh();
    }

    private void OnPowerClicked(object sender, EventArgs e)
    {
        engine.PressOperator("^");
        Refresh();
    }

    private void OnPercentClicked(object sender, EventArgs e)
    {
        engine.PressPercent();
        Refresh();
    }

    private void OnPlusMinusClicked(object sender, EventArgs e)
    {
        engine.PressPlusMinus();
        Refresh();
    }

    private void OnAllClearClicked(object sender, EventArgs e)
    {
        engine.PressAllClear();
        Refresh();
    }

    private void OnBackspaceClicked(object sender, EventArgs e)
    {
        engine.PressBackspace();
        Refresh();
    }

    private void OnFunctionClicked(object sender, EventArgs e)
    {
        engine.PressFunction(((Button)sender).Text);
        Refresh();
    }

    private void OnConstantClicked(object sender, EventArgs e)
    {
        engine.PressConstant(((Button)sender).Text);
        Refresh();
    }

    private void OnParenClicked(object sender, EventArgs e)
    {
        string t = ((Button)sender).Text;
        if (t == "(") engine.PressOpenParen(); else engine.PressCloseParen();
        Refresh();
    }

    private void OnEqualsClicked(object sender, EventArgs e)
    {
        var (success, expr, result) = engine.PressEquals();
        if (success) history.Add(expr, result);
        Refresh();
    }

    // ---------- Mémoire ----------

    private void OnMemoryClicked(object sender, EventArgs e)
    {
        string action = ((Button)sender).Text;
        double current = double.TryParse(
            engine.CurrentEntryDisplay.Replace(" ", "").Replace(",", "."),
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out double v) ? v : 0;

        switch (action)
        {
            case "MS": memory.Store(current); break;
            case "MR":
                engine.PressAllClear();
                foreach (char c in memory.Recall().ToString(System.Globalization.CultureInfo.InvariantCulture))
                {
                    if (char.IsDigit(c)) engine.PressDigit(c.ToString());
                    else if (c == '.') engine.PressDecimal();
                    else if (c == '-') engine.PressPlusMinus();
                }
                break;
            case "MC": memory.Clear(); break;
            case "M+": memory.Add(current); break;
            case "M−": memory.Subtract(current); break;
        }
        Refresh();
    }

    // ---------- Angle ----------

    private void OnAngleModeToggle(object sender, EventArgs e)
    {
        engine.Angle = engine.Angle == Models.AngleMode.Deg ? Models.AngleMode.Rad : Models.AngleMode.Deg;
        AngleModeButton.Text = engine.Angle == Models.AngleMode.Deg ? "DEG" : "RAD";
    }

    // ---------- Historique ----------

    private void OnHistoryToggle(object sender, EventArgs e)
    {
        HistoryOverlay.IsVisible = !HistoryOverlay.IsVisible;
    }

    private void OnHistoryClear(object sender, EventArgs e)
    {
        history.Clear();
        HistoryList.ItemsSource = null;
        HistoryList.ItemsSource = history.Entries;
    }

    private void OnHistorySelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Models.HistoryEntry entry)
        {
            engine.PressAllClear();
            foreach (char c in entry.Result.Replace(" ", "").Replace(",", "."))
            {
                if (char.IsDigit(c)) engine.PressDigit(c.ToString());
                else if (c == '.') engine.PressDecimal();
                else if (c == '-') engine.PressPlusMinus();
            }
            Refresh();
            HistoryOverlay.IsVisible = false;
        }
        ((CollectionView)sender).SelectedItem = null;
    }
}