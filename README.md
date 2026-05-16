GNU GPL v3 license — you can do anything you want, but you must give me credit and distribute your changes under the same terms.

ColorPicker is a WPF-based color selection tool implemented as a module for the OutfitTool ecosystem. The application provides two main interfaces: a color picker for screen sampling and a color selector for manual color adjustment.

Key features:

- **Dual-mode interface**: Separate tabs for color picking (screen capture) and color selection (manual adjustment)
- **Color picker**: Global mouse hook for screen color sampling with magnification, zoom control (1-20x), and mouse wheel zoom adjustment
- **Color selector**: Interactive HSV/RGB color space manipulation with gradient and selector images for intuitive color picking
- **Color representation**: Simultaneous display of RGB, HSV, and hexadecimal color values with copy functionality
- **Crosshair cursor**: Global overlay window with crosshair cursor for precise pixel selection
- **Real-time updates**: Immediate color preview with coordinate display during screen sampling

The application is implemented as an OutfitTool API1 module with ModuleController managing lifecycle and two commands: ToggleColorPickerCommand and ToggleColorSelectorCommand. The color engine uses EColor class for HSV/RGB conversions and ColorComponents (GradientImage, SelectorImage, ZoomImage) for rendering. Low-level mouse hooks (WH_MOUSE_LL) enable global screen interaction, while cFastPixel provides optimized bitmap manipulation for performance-critical operations.

---

Лицензия GNU GPL v3 — вы можете делать с этим что угодно, но должны указывать моё авторство и распространять ваши изменения на тех же условиях.

ColorPicker — это инструмент выбора цвета на базе WPF, реализованный как модуль экосистемы OutfitTool. Приложение предоставляет два основных интерфейса: пипетку для выбора цвета с экрана и селектор для ручной настройки цвета.

Ключевые особенности:

- **Двухрежимный интерфейс**: Отдельные вкладки для выбора цвета (захват с экрана) и подбора цвета (ручная настройка)
- **Пипетка**: Глобальный хук мыши для выбора цвета с экрана с увеличением, масштабированием (1-20x) и регулировкой масштаба колесиком мыши
- **Селектор цвета**: Интерактивная работа с цветовым пространством HSV/RGB с градиентами и изображениями-селекторами для интуитивного подбора цвета
- **Представление цвета**: Одновременное отображение значений цвета в форматах RGB, HSV и шестнадцатеричном с возможностью копирования
- **Перекрестие курсора**: Глобальное поверхностное окно с курсором-перекрестием для точного выбора пикселя
- **Обновление в реальном времени**: Немедленный просмотр цвета с отображением координат во время выборки с экрана

Приложение реализовано как модуль OutfitTool API1 с ModuleController, управляющим жизненным циклом, и двумя командами: ToggleColorPickerCommand и ToggleColorSelectorCommand. Цветовой движок использует класс EColor для преобразований HSV/RGB и компоненты ColorComponents (GradientImage, SelectorImage, ZoomImage) для отрисовки. Низкоуровневые хуки мыши (WH_MOUSE_LL) обеспечивают глобальное взаимодействие с экраном, а cFastPixel предоставляет оптимизированную манипуляцию битовыми картами для операций, критичных к производительности.