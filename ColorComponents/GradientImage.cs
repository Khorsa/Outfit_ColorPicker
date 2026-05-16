/*
 * Copyright (C) 2026 Roman Stolyarov <rshome@mail.ru>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace ColorPicker.ColorComponents;

class GradientImage
{
    private static readonly Bitmap MainBitmap = new(256, 256);

    private ColorMode? _lastColorMode;
    private double? _lastSelectorPositionInPercent;
    private BitmapImage? _lastBitmap;

    public BitmapImage Draw(ColorMode mode, double selectorPositionInPercent)
    {
        var roundedSelectorPos = Math.Round(selectorPositionInPercent, 4);
        if (_lastColorMode == mode && _lastSelectorPositionInPercent == roundedSelectorPos && _lastBitmap != null)
            return _lastBitmap;

        _lastColorMode = mode;
        _lastSelectorPositionInPercent = roundedSelectorPos;

        // Получаем плоский массив пикселей
        Rectangle rect = new Rectangle(0, 0, MainBitmap.Width, MainBitmap.Height);
        BitmapData bmpData = MainBitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

        int bytes = Math.Abs(bmpData.Stride) * MainBitmap.Height;
        byte[] rgbValues = new byte[bytes];
        EColor ec = new EColor(0, 0, 0, 0);
        int invertedSelectorPosition360 = Math.Clamp((int)((1 - selectorPositionInPercent) * 360), 0, 359);
        int invertedSelectorPosition100 = Math.Clamp((int)((1 - selectorPositionInPercent) * 100), 0, 99);
        int invertedSelectorPosition256 = Math.Clamp((int)((1 - selectorPositionInPercent) * 256), 0, 255);

        // Вычисляем позиции для каждого пикселя
        for (int i = 0; i < 256; i++)
        {
            double hueFactor = mode == ColorMode.Hue ? invertedSelectorPosition360 : 0;
            double saturationFactor = mode == ColorMode.Saturation ? invertedSelectorPosition100 : 0;
            double valueFactor = mode == ColorMode.Value ? invertedSelectorPosition100 : 0;

            for (int j = 0; j < 256; j++)
            {
                int x = i;
                int y = 255 - j;

                if (mode == ColorMode.Hue) ec.SetHsv(hueFactor, i * 100 / 256, j * 100 / 256);
                else if (mode == ColorMode.Saturation) ec.SetHsv(i * 360 / 256, saturationFactor, j * 100 / 256);
                else if (mode == ColorMode.Value) ec.SetHsv(i * 360 / 256, j * 100 / 256, valueFactor);
                else if (mode == ColorMode.Red) ec.SetArgb(0, invertedSelectorPosition256, i, j);
                else if (mode == ColorMode.Green) ec.SetArgb(0, i, invertedSelectorPosition256, j);
                else if (mode == ColorMode.Blue) ec.SetArgb(0, i, j, invertedSelectorPosition256);

                int index = (y * bmpData.Stride) + (x * 3); // поскольку Format24bppRgb: каждый пиксель занимает 3 байта
                rgbValues[index] = ec.Color.B;
                rgbValues[index + 1] = ec.Color.G;
                rgbValues[index + 2] = ec.Color.R;
            }
        }

        System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, bmpData.Scan0, bytes);
        MainBitmap.UnlockBits(bmpData);

        using (MemoryStream memoryStream = new MemoryStream())
        {
            MainBitmap.Save(memoryStream, ImageFormat.Bmp);
            _lastBitmap = new BitmapImage();
            memoryStream.Position = 0;
            _lastBitmap.BeginInit();
            _lastBitmap.StreamSource = memoryStream;
            _lastBitmap.CacheOption = BitmapCacheOption.OnLoad;
            _lastBitmap.EndInit();
        }
        return _lastBitmap;
    }
}