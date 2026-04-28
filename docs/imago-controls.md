# Imago Controls Reference

- [Introduction](#introduction)
- [Available Controls](#available-controls)
    - [Display](#display)
    - [Interactive](#interactive)
    - [Layout Panels](#layout-panels)
    - [Containers](#containers)
- [Common Properties](#common-properties)
- [Styling](#styling)
    - [Styles](#styles)
    - [Button Appearances](#button-appearances)
    - [Backgrounds](#backgrounds)
    - [Text Effects](#text-effects)
    - [Colors](#colors)
- [Text](#text)
- [Tooltips](#tooltips)
- [Data-Bound Collections](#data-bound-collections)


## Introduction

Imago includes a set of UI controls inspired by WPF. You can use them directly in C# or, more commonly, in [templates](templates.md). This page covers what controls are available, how to lay them out, and how to style them.

All controls inherit from `Control`, which provides the shared properties for sizing, spacing, alignment, and input. Panels arrange children using a two-phase layout system (measure, then arrange), and drawing is handled automatically unless you're [building a custom control](custom-controls.md).


## Available Controls

### Display

| Control | Purpose |
|---------|---------|
| `TextBlock` | Display text. Supports word wrap, alignment, inline emoji, and text effects. |
| `TextureBlock` | Display an image or texture. |
| `Viewport3D` | Embed a 3D viewport in the UI. |

### Interactive

| Control | Purpose |
|---------|---------|
| `Button` | Clickable area wrapping any content. Supports `Command` for MVVM binding and `Click` for event handling. |
| `TextButton` | A button with a text label. The most common button type. |
| `ToggleButton` | A button that maintains checked/unchecked state with distinct appearances. Set `AllowUncheck = false` to prevent unchecking. |
| `TextBox` | Editable text input with a blinking caret. Handles emoji, surrogate pairs, and cluster-aware editing. |
| `RadioGroup` | Non-visual coordinator that enforces mutual exclusivity among `ToggleButton` instances. Useful when toggles live in different containers. |
| `RadioStackPanel` | A `StackPanel` that auto-manages its `ToggleButton` children as a radio group. Preferred when all toggles are siblings. |

### Layout Panels

| Panel | Use when you need... |
|-------|---------------------|
| `StackPanel` | Items in a row or column. Set `Orientation` and `Gap`. |
| `WrapPanel` | Items that wrap to the next row when space runs out. |
| `DockPanel` | A header/sidebar/footer with a fill area. Children set `Dock`; the last child fills the center. |
| `Canvas` | Absolute pixel positioning. Children set `Left`, `Top`, `Right`, `Bottom`. |
| `LayeredPanel` | Overlapping layers (e.g. a background behind a foreground). |

### Containers

| Control | Purpose |
|---------|---------|
| `ContentControl` | Wraps a single child. Acts as a [slot](templates.md#dynamic-content) for dynamic content, or as a [data-driven host](#data-bound-collections) when combined with `ContentSource` and `ContentTemplate`. |
| `ScrollViewer` | Wraps content that may overflow. Set `ScrollDirection` to `Horizontal` or `Vertical`. |
| `ItemsControl` | Renders a collection of items using a data template. See [Data-Bound Collections](#data-bound-collections). |


## Common Properties

All controls inherit these from `Control`:

| Property | Type | Notes |
|----------|------|-------|
| `Width`, `Height` | `float` | `float.NaN` means auto-sized (the default). |
| `Margin` | `Thickness` | Space outside the control. |
| `HorizontalAlignment` | enum | `Left`, `Center`, `Right`, `Stretch` |
| `VerticalAlignment` | enum | `Top`, `Center`, `Bottom`, `Stretch` |
| `Visibility` | enum | `Visible`, `Hidden` (reserves space), `Collapsed` (no space) |
| `Background` | `IBackground?` | See [Backgrounds](#backgrounds). |
| `Style` | `IStyle?` | See [Styles](#styles). |
| `Tooltip` | `Tooltip?` | See [Tooltips](#tooltips). |
| `IsHitTestVisible` | `bool` | Whether the control receives mouse input. |
| `ZIndex` | `int` | Stacking order within the parent. Higher renders on top. Ties keep insertion order. Default `0`. |

In a template, you set these as attributes:

```xml
<StackPanel
    Margin="10"
    HorizontalAlignment="Stretch"
    Visibility="Collapsed"
    Background="{new ColorBackground(TailwindColors.Gray900)}"
/>
```


## Styling

### Styles

A style is a reusable bag of property values. Apply one with the `Style` property. Imago doesn't ship a style registry, so the common pattern is to declare your own static styles and reference them:

```xml
<TextBlock Style="{AppStyles.HeaderText}" />
<TextButton Style="{AppStyles.PrimaryButton}" />
```

To create a new style:

```csharp
using Imago.Controls;

public static class AppStyles
{
    public static IStyle HeaderText { get; } = new Style<TextBlock>(t =>
    {
        t.Foreground = TailwindColors.Slate300;
        t.FontSize = 14;
    });
}
```

### Button Appearances

Buttons have four visual states: idle, hover, pressed, and disabled. A `ButtonAppearance` bundles the visuals for all four:

```xml
<Button Appearance="{AppStyles.PrimaryButtonAppearance}" />
```

You can create appearances from sprites (4-frame strip) or solid colors:

```csharp
using Imago.Controls;

ButtonAppearance.FromSprite(sprite)
ButtonAppearance.FromColor(color)
ButtonAppearance.FromColors(idle, hover, pressed, disabled)
```

### Backgrounds

Any control can have a background. The available types are:

```csharp
using Imago.Controls.Drawing;
using Imago.Support.Drawing;

new ColorBackground(TailwindColors.Stone900)    // Solid color fill
new TextureBackground(texture)                  // Image fill
new SpriteBackground(sprite)                    // 9-slice scalable sprite
```

### Text Effects

`TextBlock` and `TextButton` support text effects via the `TextEffect` property:

```xml
<TextBlock TextEffect="{new ShadowTextEffect(2, 2, Color.Black)}" />
<TextBlock TextEffect="{new StrokeTextEffect(1, Color.Black)}" />
```

You can combine multiple effects with `MultiTextEffect`.

### Colors

The `TailwindColors` static class provides the full Tailwind CSS v3 palette. Colors are named as `TailwindColors.{Color}{Shade}`:

```csharp
using Imago.Support.Drawing;

TailwindColors.Slate500
TailwindColors.Rose300
TailwindColors.Green600
```

Shades range from 50 to 950. For custom colors, use `new Color("#hex")`.


## Text

`TextBlock` is the main control for displaying text. The most common properties:

| Property | Purpose |
|----------|---------|
| `Text` | The displayed string. |
| `Foreground` | Text color. |
| `FontSize` | Size in pixels. |
| `LineHeight` | Line spacing. `float.NaN` uses the font default. |
| `TextWrap` | Set to `TextWrap.Wrap` to enable word wrapping. |
| `TextHorizontalAlignment` | `Left`, `Center`, or `Right`. |
| `TextEffect` | Shadow, stroke, or combined effects. |

Unicode emoji are rendered automatically as inline sprites. The system uses `IInlineContentParser` to detect emoji and substitute PNG images from the asset pipeline.


## Tooltips

Any control can display a tooltip on hover. Set it using a [property element](templates.md#property-elements) in a template:

```xml
<Button Appearance="{AppStyles.PrimaryButtonAppearance}">
    <Button.Tooltip>
        <Tooltip Text="Save your progress" Placement="Right" />
    </Button.Tooltip>
</Button>
```

`Placement` controls where the tooltip appears relative to the control: `Top`, `Bottom`, `Left`, or `Right`.


## Data-Bound Collections

`ItemsControl` renders a list of items using a data template. Set `ItemsSource` to your data and `ItemTemplate` to a `DataTemplate` that builds a control for each item:

```csharp
using Imago.Controls;

var list = new ItemsControl
{
    ItemsSource = vm.AvailableItems,
    ItemTemplate = new DataTemplate<ItemDefinition>(item =>
    {
        return new TextButton(item.Name)
        {
            Style = AppStyles.PrimaryButton,
        };
    }),
};
```

`RadioStackPanel` combines this with radio-group behavior, automatically treating `ToggleButton` children as mutually exclusive. It exposes `SelectedIndex` and `SelectionChanged` for tracking the selection.

`ContentControl` mirrors the same pattern for a single content area. Set `ContentSource` to your data item and `ContentTemplate` to a template that turns it into a control:

```csharp
this._messageView.ContentSource = vm.SelectedMessage;
this._messageView.ContentTemplate = new DataTemplate<MessageVM>(m => new MessageView(m));
```

When the source can be different runtime types, list one template per type in a `DataTemplates`. Items are paired with the first template whose type fits:

```csharp
this._messageView.ContentTemplate = new DataTemplates
{
    new DataTemplate<TextMessageVM>(m => new TextMessageView(m)),
    new DataTemplate<ImageMessageVM>(m => new ImageMessageView(m)),
};
```

Whenever `ContentSource` changes, the previous content is disposed and a new control is built from the matching template. Setting `ContentSource` to `null` clears the content. The same shape works on `ItemsControl.ItemTemplate` for heterogeneous collections, and the markup form is documented under [Polymorphic Templates](templates.md#polymorphic-templates).
