/*
 * Copyright (C) 2026 Roman Stolyarov <rshome@mail.ru>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */
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

        try
        {
            var color = ReadColor(nowChangingBox);

            tbR.Text = color.R.ToString();
            tbG.Text = color.G.ToString();
            tbB.Text = color.B.ToString();
            tbH.Text = ((int)Math.Round(color.H)).ToString();
            tbS.Text = ((int)Math.Round(color.S)).ToString();
            tbV.Text = ((int)Math.Round(color.V)).ToString();
            tbColorRGB.Text = color.R + ", " + color.G + ", " + color.B;
            tbColorHSV.Text = color.H.ToString("0°") + ", " + color.S.ToString("0") + "%, " + color.V.ToString("0") + "%";
            tbColorWeb.Text = $"{color.R:X2}{color.G:X2}{color.B:X2}";
            ColorView.Background = new SolidColorBrush(color.Color);

            var columnHeight = Math.Max(1, (int)ColumnImageCanvas.ActualHeight);
            var squareWidth = Math.Max(1, SquareImageCanvas.ActualWidth);
            var squareHeight = Math.Max(1, SquareImageCanvas.ActualHeight);

            switch (_mode)
            {
                case ColorMode.Hue:
                    _selectorPosition = columnHeight - (int)(color.H * columnHeight / 360);
                    _mainPosition.X = color.S * squareWidth / 100;
                    _mainPosition.Y = squareHeight - color.V * squareHeight / 100;
                    break;
                case ColorMode.Saturation:
                    _selectorPosition = columnHeight - (int)(color.S * columnHeight / 100);
                    _mainPosition.X = color.H * squareWidth / 360;
                    _mainPosition.Y = squareHeight - color.V * squareHeight / 100;
                    break;
                case ColorMode.Value:
                    _selectorPosition = columnHeight - (int)(color.V * columnHeight / 100);
                    _mainPosition.X = color.H * squareWidth / 360;
                    _mainPosition.Y = squareHeight - color.S * squareHeight / 100;
                    break;
                case ColorMode.Red:
                    _selectorPosition = columnHeight - color.R * columnHeight / 256;
                    _mainPosition.X = color.G * squareWidth / 256;
                    _mainPosition.Y = squareHeight - color.B * squareHeight / 256;
                    break;
                case ColorMode.Green:
                    _selectorPosition = columnHeight - color.G * columnHeight / 256;
                    _mainPosition.X = color.R * squareWidth / 256;
                    _mainPosition.Y = squareHeight - color.B * squareHeight / 256;
                    break;
                case ColorMode.Blue:
                    _selectorPosition = columnHeight - color.B * columnHeight / 256;
                    _mainPosition.X = color.R * squareWidth / 256;
                    _mainPosition.Y = squareHeight - color.G * squareHeight / 256;
                    break;
            }

            _selectorPosition = Math.Clamp(_selectorPosition, 0, columnHeight - 1);
            _mainPosition.X = Math.Clamp(_mainPosition.X, 0, Math.Max(0, squareWidth - 1));
            _mainPosition.Y = Math.Clamp(_mainPosition.Y, 0, Math.Max(0, squareHeight - 1));
        }
        finally
        {
            AddEventToTextBoxes();
        }
    }

    private EColor ReadColor(TextBox? nowChangingBox)
    {
        var current = BrightPointColor;

        if (nowChangingBox == null) return current;

        if (nowChangingBox == tbColorWeb)
        {
            var colorHex = Regex.Replace((tbColorWeb.Text + "000000").Substring(0, 6), @"[^0-9A-Fa-f]", "0");
            return new EColor(255,
                Convert.ToInt32(colorHex.Substring(0, 2), 16),
                Convert.ToInt32(colorHex.Substring(2, 2), 16),
                Convert.ToInt32(colorHex.Substring(4, 2), 16));
        }

        if (nowChangingBox == tbColorRGB)
        {
            var parts = tbColorRGB.Text.Split(',')
                .Select(part => part.Trim())
                .ToArray();

            if (parts.Length >= 3
                && int.TryParse(parts[0], out var r)
                && int.TryParse(parts[1], out var g)
                && int.TryParse(parts[2], out var b))
            {
                return new EColor(255, Math.Clamp(r, 0, 255), Math.Clamp(g, 0, 255), Math.Clamp(b, 0, 255));
            }

            return current;
        }

        if (nowChangingBox == tbColorHSV)
        {
            var numbers = Regex.Matches(tbColorHSV.Text, @"\d+(?:[.,]\d+)?")
                .Select(match => double.TryParse(match.Value.Replace(',', '.'), out var value) ? value : double.NaN)
                .Where(value => !double.IsNaN(value))
                .ToArray();

            if (numbers.Length >= 3)
            {
                return new EColor(
                    Math.Clamp(numbers[0], 0, 360),
                    Math.Clamp(numbers[1], 0, 100),
                    Math.Clamp(numbers[2], 0, 100));
            }

            return current;
        }

        if (_mode is ColorMode.Red or ColorMode.Green or ColorMode.Blue)
        {
            return new EColor(255,
                Math.Clamp(ReadInt(tbR.Text, current.R), 0, 255),
                Math.Clamp(ReadInt(tbG.Text, current.G), 0, 255),
                Math.Clamp(ReadInt(tbB.Text, current.B), 0, 255));
        }

        return new EColor(
            Math.Clamp(ReadDouble(tbH.Text, current.H), 0, 360),
            Math.Clamp(ReadDouble(tbS.Text, current.S), 0, 100),
            Math.Clamp(ReadDouble(tbV.Text, current.V), 0, 100));
    }

    private static int ReadInt(string text, int fallback)
    {
        return int.TryParse(text, out var value) ? value : fallback;
    }

    private static double ReadDouble(string text, double fallback)
    {
        return double.TryParse(text.Replace(',', '.'),
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out var value)
            ? value
            : fallback;
    }


}