using ColorPicker.ColorComponents;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ColorPicker;

public partial class ColorForm
{
    private readonly GradientImage _gradientImage;
    private readonly SelectorImage _selectorImage;
    private readonly ZoomImage _zoomImage;
    private ColorMode _mode = ColorMode.Hue;

    private System.Windows.Point _mainPosition = new(0, 0);

    private System.Windows.Point MainPosition
    {
        get => _mainPosition;
        set => _mainPosition = new System.Windows.Point(Math.Clamp(value.X, 0, SquareImageCanvas.Width - 1), Math.Clamp(value.Y, 0, SquareImageCanvas.Height - 1));
    }

    private int _selectorPosition;
    int SelectorPosition
    {
        get => _selectorPosition;
        set => _selectorPosition = Math.Clamp(value, 0, (int)ColumnImageCanvas.ActualHeight - 1);
    }
    private void SelectorImageDraw()
    {
        ColumnImage.Source = _selectorImage.Draw(_mode, BrightPointColor);
    }

    private void SquareImageDraw()
    {
        SquareImage.Source = _gradientImage.Draw(_mode, _selectorPosition / ColumnImageCanvas.ActualHeight);
    }

    private EColor BrightPointColor
    {
        get
        {
            EColor brightPointColor = new EColor(0, 255, 255, 255);

            double relativeMainX = _mainPosition.X / SquareImageCanvas.ActualWidth;
            double relativeMainY = _mainPosition.Y / SquareImageCanvas.ActualHeight;
            double relativeSelectorPosition = SelectorPosition / ColumnImageCanvas.ActualHeight;

            if (_mode == ColorMode.Hue) brightPointColor.SetHsv((1 - relativeSelectorPosition) * 360, relativeMainX * 100, (1 - relativeMainY) * 100);
            if (_mode == ColorMode.Saturation) brightPointColor.SetHsv(relativeMainX * 360, (1 - relativeSelectorPosition) * 100, (1 - relativeMainY) * 100);
            if (_mode == ColorMode.Value) brightPointColor.SetHsv(relativeMainX * 360, (1 - relativeMainY) * 100, (1 - relativeSelectorPosition) * 100);
            if (_mode == ColorMode.Red) brightPointColor.SetArgb(255, (1 - relativeSelectorPosition) * 255, relativeMainX * 255, (1 - relativeMainY) * 255);
            if (_mode == ColorMode.Green) brightPointColor.SetArgb(255, relativeMainX * 255, relativeSelectorPosition * 255, relativeMainY * 255);
            if (_mode == ColorMode.Blue) brightPointColor.SetArgb(255, relativeMainX * 255, (1 - relativeMainY) * 255, (1 - relativeSelectorPosition) * 255);

            return brightPointColor;
        }
    }

    private void DrawColumnMarker()
    {
        Canvas.SetTop(ColumnMarker, SelectorPosition);
    }

    private void DrawMarker()
    {
        SquareMarker.Stroke = new SolidColorBrush(Color.FromRgb(255, 255, 255));

        if (BrightPointColor is { V: > 50, S: < 75 }) SquareMarker.Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        Canvas.SetLeft(SquareMarker, MainPosition.X - 5);
        Canvas.SetTop(SquareMarker, MainPosition.Y - 5);
    }

    private void ColorModeChanged(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton button)
        {
            ColorMode? newColorMode = button.Name switch
            {
                "RedMode" => ColorMode.Red,
                "GreenMode" => ColorMode.Green,
                "BlueMode" => ColorMode.Blue,
                "HueMode" => ColorMode.Hue,
                "SaturationMode" => ColorMode.Saturation,
                "ValueMode" => ColorMode.Value,
                _ => null
            };

            if (newColorMode != _mode)
            {
                _mode = newColorMode ?? ColorMode.Hue;
                SelectorImageDraw();
                SquareImageDraw();
                SetColorTextBoxes();
            }
        }
    }

    private void SwitchMode()
    {
        switch (_mode)
        {
            case ColorMode.Red: RedMode.IsChecked = true; break;
            case ColorMode.Green: GreenMode.IsChecked = true; break;
            case ColorMode.Blue: BlueMode.IsChecked = true; break;
            case ColorMode.Hue: HueMode.IsChecked = true; break;
            case ColorMode.Saturation: SaturationMode.IsChecked = true; break;
            case ColorMode.Value: ValueMode.IsChecked = true; break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private bool _isMousePressedOnSquareImage;
    private void SquareImageCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        _isMousePressedOnSquareImage = true;
        Mouse.Capture(SquareImageCanvas);
    }

    private void SquareImageCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Released)
        {
            _isMousePressedOnSquareImage = false;
            Mouse.Capture(null);
        }
    }


    private void SquareImage_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isMousePressedOnSquareImage)
        {
            MainPosition = new System.Windows.Point(
                Math.Clamp(e.GetPosition(SquareImageCanvas).X, 0, SquareImage.ActualWidth - 1),
                Math.Clamp(e.GetPosition(SquareImageCanvas).Y, 0, SquareImage.ActualHeight - 1)
            );

            if (_mode != ColorMode.Hue) SelectorImageDraw();
            DrawMarker();
            SetColorTextBoxes();
        }
    }

    private bool _isMousePressedOnColumnImage;
    private void ColumnImageCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            _isMousePressedOnColumnImage = true;
            Mouse.Capture(ColumnImageCanvas);
        }
    }



    private void ColumnImageCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Released)
        {
            _isMousePressedOnColumnImage = false;
            Mouse.Capture(null);
        }
    }
    private void ColumnImageCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isMousePressedOnColumnImage)
        {
            SelectorPosition = (int)(e.GetPosition(ColumnImageCanvas).Y);
            SquareImageDraw();
            DrawMarker();
            DrawColumnMarker();
            SetColorTextBoxes();
        }
    }

    private void tb_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (IsInitialized && sender is TextBox box)
        {
            SetColorTextBoxes(box);
            SelectorImageDraw();
            SquareImageDraw();
            DrawColumnMarker();
            DrawMarker();
        }
    }

    private void AddEventToTextBoxes()
    {
        TextBox[] tbColorTextBoxes = [tbColorRGB, tbColorHSV, tbColorWeb, tbR, tbG, tbB, tbH, tbS, tbV];
        foreach (var tb in tbColorTextBoxes) tb.TextChanged += tb_TextChanged;
    }

    private void RemoveEventFromTextBoxes()
    {
        TextBox[] tbColorTextBoxes = [tbColorRGB, tbColorHSV, tbColorWeb, tbR, tbG, tbB, tbH, tbS, tbV];
        foreach (var tb in tbColorTextBoxes) tb.TextChanged -= tb_TextChanged;
    }

    private void SetColorTextBoxes(TextBox? nowChangingBox = null)
    {
        RemoveEventFromTextBoxes();

        if (nowChangingBox != null)
        {
            EColor color = new(0, 0, 0, 0);
            // Изменяется одно из полей
            if (nowChangingBox == tbColorWeb)
            {
                string colorHex = Regex.Replace((tbColorWeb.Text + "000000").Substring(0, 6), @"[^0-9A-Fa-f]", "0");
                color = new EColor(255, Convert.ToInt32(colorHex.Substring(0, 2), 16), Convert.ToInt32(colorHex.Substring(2, 2), 16), Convert.ToInt32(colorHex.Substring(4, 2), 16));
            }

            if (nowChangingBox != tbR) tbR.Text = color.R.ToString();
            if (nowChangingBox != tbG) tbG.Text = color.G.ToString();
            if (nowChangingBox != tbB) tbB.Text = color.B.ToString();
            if (nowChangingBox != tbH) tbH.Text = ((int)color.H).ToString();
            if (nowChangingBox != tbS) tbS.Text = ((int)color.S).ToString();
            if (nowChangingBox != tbV) tbV.Text = ((int)color.V).ToString();
            if (nowChangingBox != tbColorRGB) tbColorRGB.Text = color.R.ToString() + ", " + color.G.ToString() + ", " + color.B.ToString();
            if (nowChangingBox != tbColorHSV) tbColorHSV.Text = color.H.ToString("0°") + ", " + color.S.ToString("0") + "%, " + color.V.ToString("0") + "%";
            if (nowChangingBox != tbColorWeb) tbColorWeb.Text = $"{color.R:X2}{color.G:X2}{color.B:X2}";
            ColorView.Background = new SolidColorBrush(color.Color);

            var columnHeight = (int)ColumnImageCanvas.ActualHeight;
            var squareWidth = SquareImageCanvas.ActualWidth;
            var squareHeight = SquareImageCanvas.ActualHeight;

            if (_mode == ColorMode.Hue)
            {
                _selectorPosition = columnHeight - int.Parse(tbH.Text) * columnHeight / 360;
                _mainPosition.X = int.Parse(tbS.Text) * squareWidth / 100;
                _mainPosition.Y = squareHeight - int.Parse(tbV.Text) * squareHeight / 100;
            }
            if (_mode == ColorMode.Saturation)
            {
                _selectorPosition = columnHeight - int.Parse(tbS.Text) * columnHeight / 100;
                _mainPosition.X = int.Parse(tbH.Text) * squareWidth / 360;
                _mainPosition.Y = squareHeight - int.Parse(tbV.Text) * squareHeight / 100;
            }
            if (_mode == ColorMode.Value)
            {
                _selectorPosition = columnHeight - int.Parse(tbV.Text) * columnHeight / 100;
                _mainPosition.X = int.Parse(tbH.Text) * squareWidth / 360;
                _mainPosition.Y = squareHeight - int.Parse(tbS.Text) * squareHeight / 100;
            }
            if (_mode == ColorMode.Red)
            {
                _selectorPosition = columnHeight - int.Parse(tbR.Text) * columnHeight / 256;
                _mainPosition.X = int.Parse(tbG.Text) * squareWidth / 256;
                _mainPosition.Y = squareHeight - int.Parse(tbB.Text) * squareHeight / 256;
            }
            if (_mode == ColorMode.Green)
            {
                _selectorPosition = columnHeight - int.Parse(tbG.Text) * columnHeight / 256;
                _mainPosition.X = int.Parse(tbR.Text) * squareWidth / 256;
                _mainPosition.Y = squareHeight - int.Parse(tbB.Text) * squareHeight / 256;
            }
            if (_mode == ColorMode.Blue)
            {
                _selectorPosition = columnHeight - int.Parse(tbB.Text) * columnHeight / 256;
                _mainPosition.X = int.Parse(tbR.Text) * squareWidth / 256;
                _mainPosition.Y = squareHeight - int.Parse(tbG.Text) * squareHeight / 256;
            }
        }
        else
        {
            ColorView.Background = new SolidColorBrush(BrightPointColor.Color);

            int r = BrightPointColor.R;
            int g = BrightPointColor.G;
            int b = BrightPointColor.B;
            double h = BrightPointColor.H;
            double s = BrightPointColor.S;
            double v = BrightPointColor.V;


            tbColorRGB.Text = r.ToString() + ", " + g.ToString() + ", " + b.ToString();
            tbColorHSV.Text = h.ToString("0°") + ", " + s.ToString("0") + "%, " + v.ToString("0") + "%";
            tbColorWeb.Text = $"{r:X2}{g:X2}{b:X2}";

            tbR.Text = r.ToString();
            tbG.Text = g.ToString();
            tbB.Text = b.ToString();

            tbH.Text = h.ToString("0");
            tbS.Text = s.ToString("0");
            tbV.Text = v.ToString("0");
        }

        AddEventToTextBoxes();
    }


}