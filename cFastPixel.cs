using System.Drawing;
using System.Drawing.Imaging;

namespace ColorPicker;

class CFastPixel
{
    private BitmapData? _bmpData;
    private IntPtr _bmpPtr = IntPtr.Zero;
    private byte[] _rgbValues = [];
    private readonly bool _isAlpha;
    private readonly int _width;
    private readonly int _height;
    private bool _locked;
    private readonly Bitmap _bitmap;

    public CFastPixel(Bitmap bmp)
    {
        _bitmap = bmp;
        if (bmp.PixelFormat == (bmp.PixelFormat | PixelFormat.Indexed))
            throw new Exception("Can't lock indexed bitmap");

        _isAlpha = bmp.PixelFormat == (bmp.PixelFormat | PixelFormat.Alpha);
        _width = bmp.Width;
        _height = bmp.Height;
    }

    public void Lock()
    {
        if (_locked) throw new Exception("Bitmap already locked");

        _bmpData = _bitmap.LockBits(new Rectangle(0,0,_width, _height), ImageLockMode.ReadWrite, _bitmap.PixelFormat);
        _rgbValues = new byte[_width * _height * (_isAlpha?4:3)];
        _bmpPtr = _bmpData.Scan0;
        System.Runtime.InteropServices.Marshal.Copy(_bmpPtr, _rgbValues, 0, _rgbValues.Length);
        _locked = true;
    }

    public void UnLock()
    {
        if (!_locked) throw new Exception("Bitmap not locked");
        System.Runtime.InteropServices.Marshal.Copy(_rgbValues, 0, _bmpPtr, _rgbValues.Length);
        if (_bmpData != null) _bitmap.UnlockBits(_bmpData);
        _locked = false;
    }

    public void SetPixel(int x, int y, Color c)
    {
        if (!_locked) throw new Exception("Bitmap not locked");
        var index = ((y * _width + x) * (_isAlpha ? 4 : 3));
        _rgbValues[index] = c.B;
        _rgbValues[index + 1] = c.G;
        _rgbValues[index + 2] = c.R;
        if (_isAlpha) _rgbValues[index + 3] = c.A;

    }
}