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
using OutfitTool.API1;
using OutfitTool.API1.Dto;

namespace ColorPicker.Commands;

class ToggleColorSelectorCommand : ICommand
{
    public string Name => "ShowHideColorSelector";
    public string Description => LocalizationHelper.GetString("ColorSelector");
    public CommandContextMenu? ContextMenu => GetContextMenu();

    public CommandContextMenu? GetContextMenu()
    {
        string resourceName = "palette2.png";
        return new CommandContextMenu(
            LocalizationHelper.GetString("ColorSelector"),
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
            if (ModuleController.ColorForm.colorSelectorTabs.SelectedIndex == 1)
            {
                ModuleController.ColorForm.colorSelectorTabs.SelectedIndex = 0;
                ModuleController.ColorForm.InitTab(0);
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
            ModuleController.ColorForm.colorSelectorTabs.SelectedIndex = 0;
            ModuleController.ColorForm.InitTab(0);
            ModuleController.ColorForm.Focus();
        }
    }
}