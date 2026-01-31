using OutfitTool.API1;
using OutfitTool.API1.Dto;

namespace ColorPicker;

internal class ModuleInfo : IModuleInfo
{
    public string Name => "color_picker";
    public string AssemblyName => "ColorPicker";
    public string DisplayName => "ColorPicker";
    public string Description => "Подбор цвета";
    public ModuleVersion Version => new (0, 1, "alpha");
    public string Changes => "Тестовая версия";
    public string Author => "Stolyarov Roman";
    public string AuthorContacts => "rshome@mail.ru";
    public int RequireApiVersion => 1;
}