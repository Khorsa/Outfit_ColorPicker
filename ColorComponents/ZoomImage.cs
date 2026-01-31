using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace ColorPicker.ColorComponents;

internal class ZoomImage
{
    private BitmapImage? _lastBitmap;

    public BitmapImage CapturePartOfScreen(Point position, Size size, double zoom)
    {
        var zoomedSize = new Size((int)(size.Width / zoom), (int)(size.Height / zoom));
        var zoomedPosition = new Point(position.X - (int)(size.Width / zoom / 2), position.Y - (int)(size.Height / zoom / 2));

        using var screenBitmap = new Bitmap(size.Width, size.Height);
        Bitmap zoomedBitmap = new Bitmap(zoomedSize.Width, zoomedSize.Height);

        using (var g = Graphics.FromImage(zoomedBitmap))
        {
            g.CopyFromScreen(zoomedPosition, Point.Empty, zoomedSize);
        }

        using (var g = Graphics.FromImage(screenBitmap))
        {
            // Увеличиваем изображение
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;

            g.DrawImage(
                zoomedBitmap, 
                new Rectangle(0, 0, screenBitmap.Width, screenBitmap.Height), 
                new Rectangle(0, 0, zoomedBitmap.Width, zoomedBitmap.Height), 
                GraphicsUnit.Pixel
            );
        }

        using (MemoryStream memoryStream = new MemoryStream())
        {
            screenBitmap.Save(memoryStream, ImageFormat.Bmp);
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