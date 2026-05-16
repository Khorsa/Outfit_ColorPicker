/*
 * Copyright (C) 2026 Roman Stolyarov <rshome@mail.ru>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */
using System;
using System.Windows.Media;

namespace ColorPicker;

class EColor
{
    private double dR = 0;
    private double dG = 0;
    private double dB = 0;

    private double min = 0;
    private double max = 0;

    public int R
    {
        set
        {
            if ((value < 0) || (value > 255)) throw new Exception("eColor: Incorrect value");
            this.dR = value;
        }
        get{ return (int)this.dR; }
    }

    public int G
    {
        set
        {
            if ((value < 0) || (value > 255)) throw new Exception("eColor: Incorrect value");
            this.dG = value;
        }
        get { return (int)this.dG; }
    }

    public int B
    {
        set
        {
            if ((value < 0) || (value > 255)) throw new Exception("eColor: Incorrect value");
            this.dB = value;
        }
        get { return (int)this.dB; }
    }


    public double H
    {
        set
        {
            if ((value < 0) || (value > 360)) throw new Exception("eColor: Incorrect value");
            int r = 0;
            int g = 0;
            int b = 0;
            HsvToRgb(value, S / 100, V / 100, out r, out g, out b);
            dR = r;
            dG = g;
            dB = b;
        }
        get
        {
            if (max == min) return 0;
            if ((max == dR) && (dG >= dB)) return 60 * (dG - dB) / (max - min);
            if ((max == dR) && (dG < dB)) return 60 * (dG - dB) / (max - min) + 360;
            if (max == dG) return 60 * (dB - dR) / (max - min) + 120;
            if (max == dB) return 60 * (dR - dG) / (max - min) + 240;
            return 0;
        }
    }


    public double S
    {
        set
        {
            if ((value < 0) || (value > 100)) throw new Exception("eColor: Incorrect value");
            int r = 0;
            int g = 0;
            int b = 0;
            HsvToRgb(H, value / 100, V / 100, out r, out g, out b);
            dR = r;
            dG = g;
            dB = b;
        }
        get
        {
            if (max == 0) return 0;
            return (100 - 100*min / max);
        }
    }

    public double V
    {
        set
        {
            if ((value < 0) || (value > 100)) throw new Exception("eColor: Incorrect value");

            int r = 0;
            int g = 0;
            int b = 0;
            HsvToRgb(H, S / 100, value / 100, out r, out g, out b);
            dR = r;
            dG = g;
            dB = b;
        }
        get
        {
            return max/2.55;
        }
    }

    public EColor(int A, int R, int G, int B)
    {
        if ((R < 0) || (R > 255)) throw new Exception("eColor: Incorrect value");
        if ((G < 0) || (G > 255)) throw new Exception("eColor: Incorrect value");
        if ((B < 0) || (B > 255)) throw new Exception("eColor: Incorrect value");

        this.dR = R;
        this.dG = G;
        this.dB = B;
        this.min = Math.Min(dR, Math.Min(dG, dB));
        this.max = Math.Max(dR, Math.Max(dG, dB));
    }

    public EColor(Color c)
    {
        this.dR = c.R;
        this.dG = c.G;
        this.dB = c.B;
        this.min = Math.Min(dR, Math.Min(dG, dB));
        this.max = Math.Max(dR, Math.Max(dG, dB));
    }

    public EColor(double H, double S, double V)
    {
        if ((H < 0) || (H > 360)) throw new Exception("eColor: Incorrect value");
        if ((S < 0) || (S > 100)) throw new Exception("eColor: Incorrect value");
        if ((V < 0) || (V > 100)) throw new Exception("eColor: Incorrect value");

        int r = 0;
        int g = 0;
        int b = 0;
        HsvToRgb(H, S / 100, V / 100, out r, out g, out b);
        dR = r;
        dG = g;
        dB = b;
        this.min = Math.Min(dR, Math.Min(dG, dB));
        this.max = Math.Max(dR, Math.Max(dG, dB));
    }

    public Color Color
    {
        get
        {
            return Color.FromRgb((byte)dR, (byte)dG, (byte)dB);
        }

    }


    private void HsvToRgb(double h, double S, double V, out int r, out int g, out int b)
    {
        double H = h;
        while (H < 0) { H += 360; };
        while (H >= 360) { H -= 360; };
        double R, G, B;
        if (V <= 0)
        { R = G = B = 0; }
        else if (S <= 0)
        { R = G = B = V; }
        else
        {
            double hf = H / 60.0;
            int i = (int)Math.Floor(hf);
            double f = hf - i;
            double pv = V * (1 - S);
            double qv = V * (1 - S * f);
            double tv = V * (1 - S * (1 - f));
            switch (i)
            {
                case 0:
                    R = V;
                    G = tv;
                    B = pv;
                    break;
                case 1:
                    R = qv;
                    G = V;
                    B = pv;
                    break;
                case 2:
                    R = pv;
                    G = V;
                    B = tv;
                    break;
                case 3:
                    R = pv;
                    G = qv;
                    B = V;
                    break;
                case 4:
                    R = tv;
                    G = pv;
                    B = V;
                    break;
                case 5:
                    R = V;
                    G = pv;
                    B = qv;
                    break;
                case 6:
                    R = V;
                    G = tv;
                    B = pv;
                    break;
                case -1:
                    R = V;
                    G = pv;
                    B = qv;
                    break;
                default:
                    R = G = B = V;
                    break;
            }
        }
        r = Clamp((int)(R * 255.0));
        g = Clamp((int)(G * 255.0));
        b = Clamp((int)(B * 255.0));
    }

    private int Clamp(int i)
    {
        if (i < 0) return 0;
        if (i > 255) return 255;
        return i;
    }
        
    public void SetArgb(int A, int R, int G, int B)
    {
        this.dR = R;
        this.dG = G;
        this.dB = B;
        this.min = Math.Min(dR, Math.Min(dG, dB));
        this.max = Math.Max(dR, Math.Max(dG, dB));
    }
        
        
    public void SetArgb(double A, double R, double G, double B)
    {
        this.dR = (int)R;
        this.dG = (int)G;
        this.dB = (int)B;
        this.min = (int)Math.Min(dR, Math.Min(dG, dB));
        this.max = (int)Math.Max(dR, Math.Max(dG, dB));
    }
        
    public void SetHsv(double H, double S, double V)
    {
        H = Math.Clamp(H, 0, 360);
        S = Math.Clamp(S, 0, 100);
        V = Math.Clamp(V, 0, 100);

        int r = 0;
        int g = 0;
        int b = 0;
        HsvToRgb(H, S / 100, V / 100, out r, out g, out b);
        dR = r;
        dG = g;
        dB = b;
        this.min = Math.Min(dR, Math.Min(dG, dB));
        this.max = Math.Max(dR, Math.Max(dG, dB));
    }

}