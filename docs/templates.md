# Imago Templates

- [Introduction](#introduction)
- [Getting Started](#getting-started)
    - [Imports](#imports)
    - [Type Aliases](#type-aliases)
    - [The Root Element](#the-root-element)
- [Setting Properties](#setting-properties)
    - [Bare Values](#bare-values)
    - [Expressions](#expressions)
- [Nesting Controls](#nesting-controls)
    - [Multiple Children](#multiple-children)
    - [Single Child](#single-child)
    - [Property Elements](#property-elements)
- [Displaying Text](#displaying-text)
- [Named Elements](#named-elements)
- [Constructor Arguments](#constructor-arguments)
- [Events](#events)
- [Bindings](#bindings)
    - [Supported Expressions](#supported-expressions)
    - [Method Targets](#method-targets)
- [Advanced Bindings](#advanced-bindings)
- [Data Templates](#data-templates)
- [Nested Classes](#nested-classes)
- [The Code-Behind](#the-code-behind)
    - [Dynamic Content](#dynamic-content)
- [Custom Controls](#custom-controls)


<a name="introduction"></a>
## Introduction

Building GUI layouts entirely in C# can get verbose and hard to visualize, especially when nesting panels, buttons, and text blocks several levels deep. Imago templates let you express that same layout in XML, which is compiled into C# at build time by a Roslyn source generator. You get the readability of declarative markup with full type checking and zero runtime overhead.

Each `.template.xml` file is paired with a code-behind `.cs` file by name: `MyControl.template.xml` generates into the `MyControl` partial class. The template declares structure (what controls exist, how they're arranged, and what their properties are) while the code-behind adds behavior (event handlers, commands, and anything dynamic).


<a name="getting-started"></a>
## Getting Started

Let's walk through a simple pause menu. The two files sit side by side:

**PauseMenu.template.xml**
```xml
<?using Imago.Controls ?>

<Canvas
    xmlns="urn:imago:controls"
    xmlns:x="urn:imago:directives"
    HorizontalAlignment="Stretch"
    VerticalAlignment="Stretch"
>
    <TextButton
        Text="Resume"
        Command="{this._vm.ResumeCommand}"
        Style="{AppStyles.PrimaryButton}"
    />
</Canvas>
```

**PauseMenu.cs**
```csharp
public partial class PauseMenu : Canvas
{
    private readonly PauseMenuViewModel _vm;

    public PauseMenu(PauseMenuViewModel vm)
    {
        this._vm = vm;
        this.LoadTemplate();
    }
}
```

Calling `this.LoadTemplate()` creates the `TextButton`, sets its properties, and adds it to the canvas. For simple cases like this, the code-behind just stores dependencies and calls `LoadTemplate()`.

There are a few things to notice in this example. The compiler pairs the two files by name: `PauseMenu.template.xml` generates into the `PauseMenu` partial class. The root element tag must match the base class (here, `Canvas`). The `<?using ?>` lines import namespaces, just like `using` in C#. And the `Command` attribute uses an `{expression}` to reference the view model, which works because `_vm` is assigned before `LoadTemplate()` runs.

> [!NOTE]
> The class must be declared as `partial` so the generated `LoadTemplate()` method can be added to it.

Every template needs two XML namespace declarations on the root element. `xmlns="urn:imago:controls"` tells the compiler how to resolve element names to control types, and `xmlns:x="urn:imago:directives"` enables directives like `x:Name` and `x:Arguments`. If you plan to use [bindings](#bindings), you'll add a third: `xmlns:bind="urn:imago:bindings"`.


<a name="imports"></a>
### Imports

Import every namespace that contains a control you use as an element, or a type you reference inside an expression:

```xml
<?using Imago.Controls ?>
<?using Imago.Support.Numerics ?>
<?using MyGame.Gui ?>
```

For example, if you use `new Thickness(bottom: 6)` in an expression, the namespace containing `Thickness` must be imported. The class's own namespace is always included automatically, so you don't need to import it.


<a name="type-aliases"></a>
### Type Aliases

If two imported namespaces contain a type with the same name, you can resolve the ambiguity with an alias:

```xml
<?using StatusVM = MyGame.ViewModels.StatusPanelViewModel ?>
```

The alias becomes the element name:

```xml
<StatusVM Name="MyViewModel" />
```

Aliases are also handy for giving shorter names to deeply nested types, even when there's no ambiguity. Nested types use the `+` separator, same as in C# reflection:

```xml
<?using PresetVM = MyGame.ViewModels.PresetsViewModel+PresetItemViewModel ?>
```


<a name="the-root-element"></a>
### The Root Element

The root element represents the control class you're defining. Properties set on it apply to `this`, and its tag name must match the base class:

```xml
<Canvas
    xmlns="urn:imago:controls"
    xmlns:x="urn:imago:directives"
    HorizontalAlignment="Stretch"
    VerticalAlignment="Stretch"
>
```

```csharp
public partial class MainMenu : Canvas { ... }
```

You can set properties, subscribe to events, and even use `bind:` on the root element. The only thing you cannot do is use `x:Arguments` on it, since the root represents `this` and is never instantiated by the template.


<a name="setting-properties"></a>
## Setting Properties

Every attribute you write in a template becomes a property assignment in the generated C# code. Most of the time, you just write the value and the compiler figures out the rest:

```xml
<TextBlock Text="Hello World" FontSize="14" IsHitTestVisible="false" />
<StackPanel Margin="bottom: 6" Dock="Right" />
```

<a name="bare-values"></a>

### Bare Values

The template compiler looks at the property's type and converts the bare value accordingly:

**Strings** are wrapped in quotes:

```xml
<TextBlock Text="Hello" />            <!-- "Hello" -->
```

**Floats** get the `f` suffix when the value is numeric. Static members like `NaN`, `MaxValue`, and `Infinity` are also recognized:

```xml
<Panel Opacity="0.5" />               <!-- 0.5f -->
<Panel Width="NaN" />                 <!-- float.NaN -->
<Panel Width="MaxValue" />            <!-- float.MaxValue -->
```

**Enums** are qualified with their type when the value matches a member name:

```xml
<Panel Dock="Right" />                <!-- DockPosition.Right -->
```

**Parsable types** that implement `IParsable<T>` are parsed via their `Parse` method. The bare value is passed as a string argument:

```xml
<TextBlock Foreground="#ff0000" />     <!-- Color.Parse("#ff0000", CultureInfo.InvariantCulture) -->
```

**Other structs and classes** use constructor sugar. The bare value becomes the constructor arguments:

```xml
<StackPanel Margin="bottom: 6" />     <!-- new Thickness(bottom: 6) -->
<Panel Size="32, 32" />               <!-- new Vector2(32, 32) -->
```

Constructor arguments can be any valid C#, including method calls and named arguments:

```xml
<StackPanel Margin="bottom: 6, right: GetPad(true)" />
```

**Everything else** (int, bool, double) passes through verbatim:

```xml
<Panel Width="300" />                 <!-- 300 -->
<Panel IsHitTestVisible="false" />    <!-- false -->
```

The C# compiler catches any type mismatches at build time.

| Type | XML | Generated C# |
|------|-----|---------------|
| `string` | `Text="Hello"` | `"Hello"` |
| `float` | `Opacity="0.5"` | `0.5f` |
| `float` | `Width="NaN"` | `float.NaN` |
| `enum` | `Dock="Right"` | `DockPosition.Right` |
| `IParsable` | `Foreground="#ff0000"` | `Color.Parse("#ff0000", ...)` |
| struct/class | `Margin="bottom: 6"` | `new Thickness(bottom: 6)` |
| `int` | `Width="300"` | `300` |
| `bool` | `IsHitTestVisible="false"` | `false` |

<a name="braces"></a>

### Expressions

For anything that isn't a simple literal, wrap it in `{braces}` to write a raw C# expression. The content is emitted verbatim into the generated code:

```xml
<TextBlock Style="{AppStyles.HeaderText}" />
<TextButton Command="{this._vm.ResumeCommand}" />
<Panel Background="{new ColorBackground(TailwindColors.Gray900.WithAlpha(0.75f))}" />
<Panel Width="{float.MaxValue}" />
<Panel Mode="{MyEnum.A | MyEnum.B}" />
```

This makes braces the escape hatch from auto-conversion when you need to reference a field or property instead of a literal:

```xml
<Panel Width="{this._vm.PanelWidth}" />
<Panel Visible="{this._vm.IsActive}" />
<TextBlock Text="{this._vm.PlayerName}" />
```

To output a literal brace in a string property, double it: `Text="{{not an expression}}"`.

> [!NOTE]
> Expressions are evaluated once when `LoadTemplate()` runs. If you need a value to update over time, use [bindings](#bindings) instead.


<a name="nesting-controls"></a>
## Nesting Controls

<a name="multiple-children"></a>
### Multiple Children

Panels like `StackPanel`, `Canvas`, and `DockPanel` accept any number of children. Just nest them:

```xml
<StackPanel Orientation="Horizontal" Gap="8">
    <TextBlock Text="First" />
    <TextBlock Text="Second" />
    <TextBlock Text="Third" />
</StackPanel>
```

Whether a control accepts children (and how many) is determined by attributes on the control class. Panels use `[ItemsProperty]` to declare which property holds the child collection, while single-child containers use `[ContentProperty]`. See [Custom Controls](#custom-controls) for details.


<a name="single-child"></a>
### Single Child

Controls like `ContentControl` and `ScrollViewer` accept exactly one child:

```xml
<ContentControl>
    <StackPanel>
        <TextBlock Text="Inside content" />
    </StackPanel>
</ContentControl>
```

If you try to add more than one, the compiler will let you know.


<a name="property-elements"></a>
### Property Elements

Sometimes you need to assign a control to a specific property rather than adding it as a regular child. You can do this with `<TypeName.PropertyName>` syntax:

```xml
<Button Appearance="{AppStyles.ZoomInAppearance}">
    <Button.Tooltip>
        <Tooltip Text="Zoom In" Placement="Right" />
    </Button.Tooltip>
</Button>
```

Here, `<Tooltip>` is assigned to the button's `Tooltip` property rather than added as a child. You can use property elements for any property, not just controls. For string properties, place the text directly inside the property element:

```xml
<TextBlock>
    <TextBlock.Text>
        Multiline text here
        with preserved line breaks
    </TextBlock.Text>
</TextBlock>
```


<a name="displaying-text"></a>
## Displaying Text

For short text, an attribute works fine:

```xml
<TextBlock Text="Hello World" />
```

But for longer or multiline text, you can place the content directly inside the element:

```xml
<TextBlock>
    &#x2022; First bullet point with some explanatory text
    &#x2022; Second bullet point describing another feature
    &#x2022; Third bullet point with additional context
</TextBlock>
```

Multiline text is automatically dedented: leading and trailing blank lines are stripped, and the common leading whitespace is removed. This means you can indent naturally inside your XML without extra whitespace ending up in the output.

This syntax works on any control with a `[TextProperty]` attribute (like `TextBlock`). Standard XML entities like `&#x2022;` are resolved during parsing.


<a name="named-elements"></a>
## Named Elements

Use the `x:Name` directive to create a code-behind field for a control:

```xml
<TextButton x:Name="SaveButton" Text="Save" />
<TextBlock x:Name="StatusLabel" Text="Ready" />
```

The compiler generates a private field for each, converting PascalCase to _camelCase. So `SaveButton` becomes `_saveButton` and `StatusLabel` becomes `_statusLabel`. These fields are available as soon as `LoadTemplate()` returns:

```csharp
public MyGui()
{
    this.LoadTemplate();

    // _saveButton and _statusLabel are now available
    this._saveButton.IsEnabled = false;
}
```

The `x:Name` value is also assigned to the control's `Name` property, making it findable via `Find()` and `FindOrFail()`.

If you only need to set `Name` for runtime lookup without generating a field, use the plain `Name` attribute instead:

```xml
<TextBlock Name="MyLabel" Text="Hello" />
```

> [!NOTE]
> Only add `x:Name` to elements you actually need from C#. Unnamed elements are still created and added to the layout. Names must be unique within each class scope (the main template and each [nested class](#nested-classes) have independent namespaces).


<a name="constructor-arguments"></a>
## Constructor Arguments

If a child control requires constructor parameters, pass them with `x:Arguments`:

```xml
<ToolbarControls x:Arguments="{this._vm}" Margin="4" />
<SideBar x:Arguments="{this._vm, this}" Dock="Right" />
```

Like expressions, the content inside the braces is raw C#. You can pass multiple arguments separated by commas or construct objects inline:

```xml
<StatusPanel x:Arguments="{new StatusPanelViewModel(this._vm.Character)}" />
```

Properties set on the element are assigned after construction, so you can freely combine `x:Arguments` with regular attributes on the same element.

> [!NOTE]
> `x:Arguments` is only valid on child elements. The root element represents `this` and is never instantiated by the template.


<a name="events"></a>
## Events

To subscribe to an event, set it as an attribute with the handler method name:

```xml
<TextButton Click="OkButton_Click" Style="{AppStyles.PrimaryButton}" />
```

The compiler recognizes `Click` as an event on `TextButton` and wires it up automatically. A bare method name refers to a method on the current class:

```csharp
private void OkButton_Click(object? sender, EventArgs e)
{
    PopupService.Instance.HidePopup();
}
```

If the handler lives outside the current class, wrap it in `{braces}` like any other expression:

```xml
<TextButton Click="{ClipboardHelpers.CopyToClipboard}" />
```

Events work on the root element too, subscribing directly on `this`.


<a name="bindings"></a>
## Bindings

Regular attributes are evaluated once when `LoadTemplate()` runs. If you need a value to stay in sync with a source that changes over time, use a binding. Bindings watch a source property and update the target automatically whenever the source raises `PropertyChanged`.

To declare a binding, add the `bind` namespace to your root element and use the `bind:` prefix on any attribute you want to keep reactive:

```xml
<DockPanel
    xmlns="urn:imago:controls"
    xmlns:x="urn:imago:directives"
    xmlns:bind="urn:imago:bindings"
    bind:IsVisible="this._vm.IsPanelVisible"
>
    <TextBlock x:Name="Title" bind:Text="this._vm.PlayerName" Dock="Top" />
    <ProgressBar x:Name="HealthBar" bind:Value="this._vm.Health" />
    <ProgressBar x:Name="EnergyBar" bind:Value="this._vm.Energy" />
</DockPanel>
```

The target is set once immediately and updated each time the source property changes. You may use `bind:` on the root element, on named children, or on any element in the tree. Bindings are activated when the control is mounted (added to its parent scene graph) and disposed automatically when unmounted.

> [!WARNING]
> Bindings are not allowed inside [data template](#data-templates) bodies, since each factory invocation would create duplicate subscriptions with no clear ownership.


<a name="supported-expressions"></a>
### Supported Expressions

A binding expression is a one-level member access: a source object followed by a property name. The source must implement `INotifyPropertyChanged`. The leading `this.` is optional:

```xml
bind:Value="this._vm.Health"
bind:Value="_vm.Health"               <!-- implicit this. -->
bind:Value="this._otherField.Prop"
```

The target (the part after `bind:`) is resolved against the element's type. If the compiler finds a settable property, it generates an assignment. You cannot combine `bind:` and a regular attribute on the same property.

For anything more complex (nested paths, conditionals, string interpolation), see [Advanced Bindings](#advanced-bindings).


<a name="method-targets"></a>
### Method Targets

If the target name resolves to a method with one parameter instead of a settable property, the compiler generates a method call instead of an assignment. This is useful when a property change needs to drive side effects beyond setting a single value:

```xml
<Canvas
    bind:IsVisible="this._vm.IsPanelVisible"
    bind:SetInfoPanelOpen="this._vm.IsInfoPanelOpen"
/>
```

```csharp
private void SetInfoPanelOpen(bool value)
{
    this._infoPanel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
}
```

Both property and method targets follow the same lifecycle: called once immediately, then again on each change. The syntax is identical either way.


<a name="advanced-bindings"></a>
## Advanced Bindings

Template bindings are limited to a single `source.Property` path. When you need more expressive bindings, override `CreateBindings()` in the code-behind and use [EBind](https://github.com/SIDOVSKY/EBind). EBind supports nested paths, conditionals, method calls, and string interpolation.

Like template bindings, `CreateBindings()` is called when the control is added to a layer and disposed automatically when removed:

```csharp
protected override IDisposable? CreateBindings()
{
    return new EBinding
    {
        // Property binding with expression
        () => this._title.Text == $"{this._vm.FirstName} {this._vm.LastName}",

        // Conditional binding
        () => this._panel.Opacity == (this._vm.IsActive ? 1f : 0.5f),

        // Action binding: called whenever CurrentTab changes
        () => this.UpdateTabContent(this._vm.CurrentTab),

        // Event binding
        (this._vm.Character, nameof(Character.ItemInHandChanged), this.UpdateItemInHand),
    };
}
```

**Property bindings** use `==` to declare a one-way sync from right to left. Any property or field referenced on the right side triggers an update when it changes.

**Action bindings** call a method whenever any referenced property changes. Useful for side effects that don't map to a single property assignment.

**Event bindings** take a source object, an event name, and a handler. Unlike the other two, they don't fire on construction.


<a name="data-templates"></a>
## Data Templates

When a panel uses `ItemsSource` to render a dynamic collection, each item needs a `DataTemplate` that defines how to turn it into a control. You can declare this directly in the template using `x:TypeArguments` to specify the item type:

```xml
<StackPanel ItemsSource="{this._vm.Presets}">
    <StackPanel.ItemTemplate>
        <DataTemplate x:TypeArguments="PresetVM">
            <TextButton
                x:Arguments="{item.Name}"
                Appearance="{AppStyles.PrimaryButtonAppearance}"
                Command="{this._vm.ApplyPresetCommand}"
                CommandParameter="{item.Preset}"
            />
        </DataTemplate>
    </StackPanel.ItemTemplate>
</StackPanel>
```

Inside the `<DataTemplate>` body, `item` refers to the current data item, typed as `PresetVM` in this example. You can use it in any `{expression}`, including `x:Arguments`. The compiler generates a private factory method that receives `item` as a parameter, so `{item.Name}`, `{item}`, and `{this._vm.SomeCommand}` all work.

The `DataTemplate` must contain exactly one child element, which becomes the root of the generated control tree. The type passed to `x:TypeArguments` is resolved through the same namespace imports and aliases as element names:

```xml
<?using MyGame.ViewModels ?>
<?using PresetVM = MyGame.ViewModels.PresetsViewModel+PresetItemViewModel ?>
```

There are a few restrictions inside a data template body:

- **No `x:Name`.** The factory is called once per item, so a single class field would be overwritten each time.
- **No `bind:` bindings.** Each invocation would create duplicate subscriptions with no clear ownership.
- **No `x:Arguments` on the factory root.** Children within the factory can still use `x:Arguments`.

For cases where the factory logic requires conditionals or multi-step computation, keep the `ItemTemplate` assignment in the code-behind instead:

```csharp
this.LoadTemplate();
this._itemsPanel.ItemTemplate = new DataTemplate<PresetItem>(this.ButtonFactory);
```


<a name="nested-classes"></a>
## Nested Classes

Sometimes a control has private nested classes that are implementation details of the parent. Instead of giving each one its own template file, you can define them inline using `x:Class` on a direct child of the root element:

```xml
<DockPanel
    xmlns="urn:imago:controls"
    xmlns:x="urn:imago:directives"
>
    <!-- Regular children of the main template -->
    <WrapPanel x:Name="ItemsPanel" />

    <!-- Nested class definition: not added as a child -->
    <DetailsCard x:Class="ItemDetailsContent" Width="400">
        <ContentControl x:Name="Preview" Dock="Left" Width="128" Height="128" />
        <TextBlock x:Name="Name" Dock="Top" Style="{AppStyles.HeaderText}" />
    </DetailsCard>
</DockPanel>
```

The element tag (`DetailsCard`) becomes the base class, and `x:Class` names the nested class. This element is not added to the parent's children. Instead, the compiler generates a separate `LoadTemplate()` for it:

```csharp
public partial class ItemsTab : DockPanel
{
    private readonly ItemDetailsContent _details = new();

    public ItemsTab()
    {
        this.LoadTemplate();
    }

    private partial class ItemDetailsContent : DetailsCard
    {
        public ItemDetailsContent()
        {
            this.LoadTemplate(); // generated from the x:Class block
        }
    }
}
```

Everything works the same as a regular template root: properties on the element apply to `this`, children are added to the control tree, and named elements become private fields on the nested class.

You can define multiple `x:Class` blocks in one file. Namespace imports (`<?using ?>`) are shared across all of them.


<a name="the-code-behind"></a>
## The Code-Behind

Many templates need very little code-behind. But for anything the template can't express declaratively, the code-behind is where you handle it.

The key rule is ordering. Assign any fields that your template expressions reference _before_ calling `this.LoadTemplate()`, and don't access named elements _until after_ it returns:

```csharp
public MyGui(MyViewModel vm)
{
    // 1. Store dependencies (referenced by {this._vm.X} expressions)
    this._vm = vm;

    // 2. Build the template (creates controls, sets properties, adds children)
    this.LoadTemplate();

    // 3. Now named elements like _saveButton are available
    this._saveButton.IsEnabled = false;
}
```

> [!WARNING]
> Accessing a named element before `LoadTemplate()` will give you `null`. Referencing an unassigned field inside a template expression will throw at runtime.


<a name="dynamic-content"></a>
### Dynamic Content

When part of the UI needs to be built at runtime, use a named `ContentControl` as a slot in the template and fill it from code:

**SettingsPopup.template.xml**
```xml
<StackPanel>
    <TextBlock Text="Graphics Backend" Style="{AppStyles.HeaderText}" />
    <ContentControl x:Name="RadioPanelSlot" />
    <TextBlock Text="Changing the backend requires a restart." />
</StackPanel>
```

**SettingsPopup.cs**
```csharp
public SettingsPopup(SettingsViewModel vm)
{
    this.LoadTemplate();
    this._radioPanelSlot.Content = this.CreateBackendRadioPanel(vm);
}
```

This keeps the static layout in the template while the code-behind builds the dynamic parts.


<a name="custom-controls"></a>
## Custom Controls

The built-in controls work in templates out of the box. Panels accept children, content controls accept a single child, and `TextBlock` accepts text. If you're building your own controls, you can teach the compiler how they accept content using three attributes.

`[ItemsProperty]` tells the compiler which property holds multiple children:

```csharp
[ItemsProperty(nameof(Children))]
public class MyPanel : Control
{
    public List<Control> Children { get; } = new();
}
```

`[ItemsMethod]` is similar, but tells the compiler to call a method for each child instead of adding to a collection property:

```csharp
[ItemsMethod(nameof(AddChild))]
public class MyContainer : Control
{
    public void AddChild(Control child) { ... }
}
```

This is useful when adding a child involves side effects beyond appending to a list (e.g., `Node3D.AddChild` and `Screen.AddLayer`).

`[ContentProperty]` declares a single-child slot:

```csharp
[ContentProperty(nameof(Body))]
public class Card : Control
{
    public Control Body { get; set; }
}
```

And `[TextProperty]` tells it where text content goes:

```csharp
[TextProperty(nameof(Label))]
public class Badge : Control
{
    public string Label { get; set; }
}
```

With these in place, your controls work naturally in templates:

```xml
<MyPanel>
    <TextBlock Text="First" />
    <TextBlock Text="Second" />
</MyPanel>

<Card>
    <TextBlock Text="Card body" />
</Card>

<Badge>Gold Member</Badge>
```

All four attributes are inherited, so subclasses get them for free. If a control lacks all four attributes, nesting children inside it will produce a compile error.

For a deeper look at building framework-level controls (layout, drawing, input handling), see the [custom controls](custom-controls.md) documentation.
