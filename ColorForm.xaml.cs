using ColorPicker.ColorComponents;
using System.Windows;
using System.Windows.Controls;

namespace ColorPicker;

public partial class ColorForm
{
    public ColorForm()
    {
        LocalizationHelper.LanguageChanged += LanguageChanged;
        InitializeComponent();
        AddEventToTextBoxes();

        _gradientImage = new GradientImage();
        _selectorImage = new SelectorImage();
        _zoomImage = new ZoomImage();
    }

    private void LanguageChanged(object? sender, EventArgs e)
    {
        var oldDict = (from d in this.Resources.MergedDictionaries
            where d.Source != null && d.Source.OriginalString.Contains("Languages/lang.")
            select d).First();
        if (oldDict != null)
        {
            var ind = Resources.MergedDictionaries.IndexOf(oldDict);
            Resources.MergedDictionaries.Remove(oldDict);
            Resources.MergedDictionaries.Insert(ind, LocalizationHelper.CurrentDictionary);
        }
        else
        {
            Resources.MergedDictionaries.Add(LocalizationHelper.CurrentDictionary);
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        ColorPickerTabShutdown();
        Hide();
    }

    private void Window_Activated(object sender, EventArgs e)
    {
        SwitchMode();
        SelectorImageDraw();
        SquareImageDraw();
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        DrawMarker();
        DrawColumnMarker();
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        if (ReferenceEquals(sender, bColorWebCopy)) Clipboard.SetText(tbColorWeb.Text);
        if (ReferenceEquals(sender, bColorHSVCopy)) Clipboard.SetText(tbColorHSV.Text);
        if (ReferenceEquals(sender, bColorRGBCopy)) Clipboard.SetText(tbColorRGB.Text);
    }

    private void colorSelectorTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            if ((e.AddedItems[0] as TabItem)?.Name == "colorPickerTab") InitTab(1);
        } 
        if (e.RemovedItems.Count > 0)
        {
            if ((e.RemovedItems[0] as TabItem)?.Name == "colorPickerTab") ColorPickerTabShutdown();
        }
    }

    public void InitTab(int tabNum)
    {
        if (tabNum == 1) ColorPickerTabInit();
    }
}