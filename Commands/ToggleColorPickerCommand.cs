/*
 * Copyright (C) 2026 Roman Stolyarov <rshome@mail.ru>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Windows;
using OutfitTool.API1.Dto;
using OutfitTool.API1;

namespace ColorPicker.Commands;

class ToggleColorPickerCommand : ICommand
{
    public string Name => "ShowHideColorPicker";
    public string Description => LocalizationHelper.GetString("ColorPicker");
    public CommandContextMenu? ContextMenu => GetContextMenu();

    public CommandContextMenu? GetContextMenu()
    {
        string resourceName = "monitor_brush.png";
        return new CommandContextMenu(
            LocalizationHelper.GetString("ColorPicker"),
            new BitmapImage(
                new Uri("pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/Resources/" + resourceName)
            )
        );
    }

    public void Run()
    {
        if (ModuleController.ColorForm == null)
        {
            return;
        }

        if (ModuleController.ColorForm.Visibility == Visibility.Visible)
        {
            if (ModuleController.ColorForm.colorSelectorTabs.SelectedIndex == 0)
            {
                ModuleController.ColorForm.colorSelectorTabs.SelectedIndex = 1;
                ModuleController.ColorForm.InitTab(1);
            }
            else
            {
                ModuleController.ColorForm.Hide();
                ModuleController.ColorForm.Visibility = Visibility.Hidden;
            }
        }
        else
        {
            ModuleController.ColorForm.Topmost = true;
            ModuleController.ColorForm.Show();
            ModuleController.ColorForm.Activate();
            ModuleController.ColorForm.colorSelectorTabs.SelectedIndex = 1;
            ModuleController.ColorForm.InitTab(1);
            ModuleController.ColorForm.Focus();
        }
    }
}