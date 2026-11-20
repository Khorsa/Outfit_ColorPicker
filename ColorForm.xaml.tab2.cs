/*
 * Copyright (C) 2026 Roman Stolyarov <rshome@mail.ru>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ColorPicker;

public partial class ColorForm
{
    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    private const int WH_MOUSE_LL = 14;
    private const int WM_MOUSEMOVE = 0x0200;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_MOUSEWHEEL = 0x020A;

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
    private LowLevelMouseProc? _proc;
    
    private IntPtr _hookId = IntPtr.Zero;

    private bool _isMouseCaptured;

    private const double ZOOM_STEP = 0.5;
    private const double MAX_ZOOM = 20;

    private System.Drawing.Color SelectedColor
    {
        set { 
            magnColorView.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, value.R, value.G, value.B));

            EColor ec = new EColor(255, value.R, value.G, value.B);
            tbMagnColorHSV.Text = ec.H.ToString("0°") + ", " + ec.S.ToString("0") + "%, " + ec.V.ToString("0") + "%";
            tbMagnColorRGB.Text = ec.R + ", " + ec.G + ", " + ec.B;
            tbMagnColorWeb.Text = $"{ec.R:X2}{ec.G:X2}{ec.B:X2}";
        }
    }

    private struct MsLlHookStruct(Point pt, int mouseData, uint flags, uint time, IntPtr dwExtraInfo)
    {
        public Point Pt = pt;
        public readonly int MouseData = mouseData;
        public uint Flags = flags;
        public uint Time = time;
        public IntPtr DwExtraInfo = dwExtraInfo;
    }

    private double _zoom = 1;
    private double Zoom
    {
        get => _zoom;
        set
        {
            _zoom = Math.Clamp(value, 1, MAX_ZOOM);
            tbMagnZoom.Text = ((int)(_zoom * 100)) + "%";
        }
    }
    private bool _initialized;
    private readonly DispatcherTimer _timer = new();
    
    private void ColorPickerTabInit()
    {
        if (_initialized) return;
        _timer.Interval = TimeSpan.FromMilliseconds(10);
        _timer.Tick += MagnifyTimerHandler;
        _timer.Start();
        CaptureGlobalMouse();
        _initialized = true;
        
        _proc = HookCallback;
        _hookId = SetHook(_proc);
    }

    private void ColorPickerTabShutdown()
    {
        _timer.Stop();
        _timer.Tick -= MagnifyTimerHandler;
    
        if (_hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }

        _hooked = false;
    
        ReleaseGlobalMouse();
        _overlayWindow?.Close();
        _overlayWindow = null;
    
        // Freeing up image resources
        MagnifyImage.Source = null;
        _initialized = false;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public int X;
        public int Y;
    }

    private static bool _hooked;
    private static IntPtr SetHook(LowLevelMouseProc proc)
    {
        if (_hooked) return 0;
        _hooked = true;

        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule;
        if (curModule == null)
            throw new Exception("Couldn't get current process");

        return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode < 0) return CallNextHookEx(_hookId, nCode, wParam, lParam);
        switch (wParam)
        {
            case WM_MOUSEMOVE:
                break;
            case WM_LBUTTONDOWN:
                break;
            case WM_MOUSEWHEEL:
            {
                var hookStruct = (MsLlHookStruct)(Marshal.PtrToStructure(lParam, typeof(MsLlHookStruct)) ?? throw new InvalidOperationException());
                var wheelDelta = (short)((hookStruct.MouseData >> 16) & 0xFFFF);
    
                if (wheelDelta > 0) Zoom += ZOOM_STEP;  // up
                else Zoom -= ZOOM_STEP;                 // down
                break;
            }
        }
        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private void MagnifyTimerHandler(object? sender, EventArgs e)
    {
        if (!(this.Visibility == Visibility.Visible && (this.WindowState != WindowState.Minimized))) return;

        if (MagnifyImageCanvas.ActualWidth != 0 && MagnifyImageCanvas.ActualHeight != 0)
        {
            System.Windows.Point screenPosition = Mouse.GetPosition(null);
            tbMagnPosition.Text = $"{screenPosition.X}, {screenPosition.Y}";

            try
            {
                MagnifyImage.Source = _zoomImage.CapturePartOfScreen(
                    new System.Drawing.Point((int)screenPosition.X, (int)screenPosition.Y),
                    new System.Drawing.Size((int)MagnifyImageCanvas.ActualWidth, (int)MagnifyImageCanvas.ActualHeight),
                    Zoom
                );
            }
            catch
            {
                // Край экрана/смена монитора: кадр может быть вне виртуального экрана.
                MagnifyImage.Source = null;
            }
        }
    }

    private void ResetZoom_Click(object sender, RoutedEventArgs e)
    {
        Zoom = 1;
    }

    private void ReleaseGlobalMouse()
    {
        if (!_isMouseCaptured) return;
        try
        {
            Mouse.Capture(null);
            Mouse.OverrideCursor = null;
            _isMouseCaptured = false;
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private void OverlayWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        Owner = null;
        _isMouseCaptured = false;
    }

    private bool _mousePressed;
    private Window? _overlayWindow;
    private void SetupOverlayWindow()
    {
        if (_overlayWindow != null) _overlayWindow.Closing += OverlayWindow_Closing;

        // Creating a panel for intercepting events
        var capturePanel = new Grid
        {
            Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(1, 0, 0, 0))
        };

        capturePanel.MouseDown += (_, e) =>
        {
            if (e.ChangedButton == MouseButton.Left) _mousePressed = true;
        };


        capturePanel.MouseUp += (_, e) =>
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _overlayWindow?.Hide();
                if (_overlayWindow != null)
                {
                    var screenPosition = _overlayWindow.PointToScreen(e.GetPosition(capturePanel));
                    var bmp = new Bitmap(1, 1);
                    using var g = Graphics.FromImage(bmp);
                    g.CopyFromScreen((int)screenPosition.X, (int)screenPosition.Y, 0, 0, new System.Drawing.Size(1, 1));
                    SelectedColor = bmp.GetPixel(0, 0);
                }
                _overlayWindow?.Show();
                _mousePressed = false;
            }
        };


        capturePanel.MouseMove += (_, e) =>
        {
            if (_overlayWindow == null) return;
            var screenPosition = _overlayWindow.PointToScreen(e.GetPosition(capturePanel));
            if (!_mousePressed) return;
            var bmp = new Bitmap(1, 1);
            using var g = Graphics.FromImage(bmp);
            g.CopyFromScreen((int)screenPosition.X, (int)screenPosition.Y, 0, 0, new System.Drawing.Size(1, 1));
            SelectedColor = bmp.GetPixel(0, 0);
        };

        if (_overlayWindow != null)
        {
            _overlayWindow.Content = capturePanel;
            _overlayWindow.Focusable = true;

            _overlayWindow.KeyDown += (_, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    _overlayWindow.Close();
                }
            };

            _overlayWindow.Show();
            this.Owner = _overlayWindow;
        }

        this.Focus();
        _isMouseCaptured = true;
    }

    private void CaptureGlobalMouse()
    {
        if (_isMouseCaptured) return;

        try
        {
            // We use the system parameters for the virtual screen
            _overlayWindow = new Window
            {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(1, 0, 0, 0)),
                Topmost = true,
                ShowInTaskbar = false,
                Cursor = Cursors.Cross,

                // We stretch it over the entire virtual screen
                Left = SystemParameters.VirtualScreenLeft,
                Top = SystemParameters.VirtualScreenTop,
                Width = SystemParameters.VirtualScreenWidth,
                Height = SystemParameters.VirtualScreenHeight,
                WindowStartupLocation = WindowStartupLocation.Manual
            };

            SetupOverlayWindow();
        }
        catch (Exception)
        {
            // ignored
        }
    }
}