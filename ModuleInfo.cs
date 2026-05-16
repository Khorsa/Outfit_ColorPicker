/*
 * Copyright (C) 2026 Roman Stolyarov <rshome@mail.ru>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */
using OutfitTool.API1;
using OutfitTool.API1.Dto;

namespace ColorPicker;

internal class ModuleInfo : IModuleInfo
{
    public string Name => "color_picker";
    public string AssemblyName => "ColorPicker";
    public string DisplayName => "ColorPicker";
    public string Description => "Подбор цвета";
    public ModuleVersion Version => new (1, 0, "release");
    public string Changes => "Тестовая версия";
    public string Author => "Stolyarov Roman";
    public string AuthorContacts => "rshome@mail.ru";
    public int RequireApiVersion => 1;
}