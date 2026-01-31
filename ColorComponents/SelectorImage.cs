using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace ColorPicker.ColorComponents;

class SelectorImage
{
    private static readonly Bitmap Bmp = new(20, 256);

    private readonly ColorMode? _oldColorMode = null;
    private readonly EColor? _oldBrightPointColor = null;
    public BitmapImage Draw(ColorMode mode, EColor brightPointColor)
    {
        if (_oldColorMode != mode
            || _oldBrightPointColor?.H != brightPointColor.H
            || _oldBrightPointColor?.S != brightPointColor.S
            || _oldBrightPointColor?.V != brightPointColor.V
            || _oldBrightPointColor?.R != brightPointColor.R
            || _oldBrightPointColor?.G != brightPointColor.G
            || _oldBrightPointColor?.B != brightPointColor.B
           )
        {
            CFastPixel pix = new CFastPixel(Bmp);
            pix.Lock();
            EColor ec = new EColor(0, 0, 0, 0);
            for (var i = 0; i < 256; i++)
            {
                if (mode == ColorMode.Hue) ec.SetHsv(i * 360 / 256, 100, 100);
                else if (mode == ColorMode.Saturation) ec.SetHsv(brightPointColor.H, i * 100 / 256, brightPointColor.V);
                else if (mode == ColorMode.Value) ec.SetHsv(brightPointColor.H, brightPointColor.S, i * 100 / 256);
                else if (mode == ColorMode.Red) ec.SetArgb(255, i, brightPointColor.G, brightPointColor.B);
                else if (mode == ColorMode.Green) ec.SetArgb(255, brightPointColor.R, i, brightPointColor.B);
                else if (mode == ColorMode.Blue) ec.SetArgb(255, brightPointColor.R, brightPointColor.G, i);

                for (var j = 0; j < 20; j++) pix.SetPixel(j, 255 - i, Color.FromArgb(ec.Color.R, ec.Color.G, ec.Color.B));
            }
            pix.UnLock();
        }

        BitmapImage bitmapImage = new BitmapImage();
        using (MemoryStream memoryStream = new MemoryStream())
        {
            Bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
            memoryStream.Position = 0;
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
        }
        return bitmapImage;
    }
}