/*
 * Copyright (C) 2026 Roman Stolyarov <rshome@mail.ru>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */
using ColorPicker.Commands;
using OutfitTool.API1;
using OutfitTool.API1.Dto;
using OutfitTool.API1.Logger;
using System.Globalization;
using System.Windows.Media.Imaging;

namespace ColorPicker;

internal class ModuleController : IModuleController
{
    public static ColorForm ColorForm = new();

    public List<ICommand> GetCommandList()
    {
        return
        [
            new ToggleColorPickerCommand(),
            new ToggleColorSelectorCommand()
        ];
    }

    public BitmapImage? GetTaskbarIcon()
    {
        return null;
    }

    public string? GetTaskbarIconText()
    {
        return null;
    }

    public void OpenSettings()
    {
    }

    public Notification? PopNotification()
    {
        return null;
    }

    public void SetLanguage(string language)
    {
        LocalizationHelper.SetLanguage(new CultureInfo(language));
    }

    public void Shutdown()
    {
        ColorForm.Close();
    }

    public void Init(ILogger logger)
    {
        ColorForm = new ColorForm();
    }
}