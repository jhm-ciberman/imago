# Custom Controls

- [Introduction](#introduction)
- [Types of Controls](#types-of-controls)
- [The Layout Lifecycle](#the-layout-lifecycle)
    - [Measuring](#measuring)
    - [Arranging](#arranging)
    - [Drawing](#drawing)
- [Defining Properties](#defining-properties)
- [Input Handling](#input-handling)
- [The DrawingContext](#the-drawingcontext)
- [Reference Code](#reference-code)


## Introduction

The [built-in controls](imago-controls.md) cover most UI needs. But when you need something custom, whether it's a leaf control that draws something unique, a container with a custom layout algorithm, or a game-level composite that bundles existing controls together, you can build your own by extending `Control`.

This page covers the framework-level API for authoring controls. If you're just using existing controls in templates, see the [controls](imago-controls.md) and [templates](templates.md) docs instead.


## Types of Controls

There are three kinds of custom controls, depending on what you're building:

**Leaf controls** inherit from `Control` and render content directly through `DrawCore`. They have no children. `TextBlock` and `TextureBlock` are examples.

**Containers** manage the layout of child controls. They inherit from `ContentControl` (one child) or `ItemsControl` (many children) and override `MeasureOverride` and `ArrangeOverride` to position their children. `StackPanel` and `DockPanel` are examples.

**Composites** combine existing controls into a reusable unit. They're the most common type. You typically inherit from a panel, add children in the constructor (or use a [template](templates.md)), and wire up behavior. Most game UI is built this way.

Framework-level leaf controls and containers live under `src/Imago/Controls/`. Composites go in your game or application project.


## The Layout Lifecycle

The layout system runs in two phases: measure, then arrange. After layout, controls are drawn.

### Measuring

Override `MeasureOverride` to report how much space your control wants. If you have children, call `child.Measure()` on each one:

```csharp
protected override Vector2 MeasureOverride(Vector2 availableSize)
{
    var childSize = Vector2.Zero;
    foreach (var child in this.Items)
    {
        child.Measure(availableSize);
        childSize.X += child.DesiredSize.X;
        childSize.Y = MathF.Max(childSize.Y, child.DesiredSize.Y);
    }

    return childSize;
}
```

### Arranging

Override `ArrangeOverride` to position each child within the final rect. Call `child.Arrange()` on each one:

```csharp
protected override Rect ArrangeOverride(Rect finalRect)
{
    var x = finalRect.X;
    foreach (var child in this.Items)
    {
        child.Arrange(new Rect(x, finalRect.Y, child.DesiredSize.X, finalRect.Height));
        x += child.DesiredSize.X;
    }

    return finalRect;
}
```

### Drawing

Override `DrawCore` to render your control. Always call `base.DrawCore(ctx)` first (it draws the background), then draw your content using the [DrawingContext](#the-drawingcontext):

```csharp
protected override void DrawCore(DrawingContext ctx)
{
    base.DrawCore(ctx);
    ctx.DrawRectangle(this.Position, this.ActualSize, Color.Red);
}
```

Use `this.Position` and `this.ActualSize` for your control's bounds. These are set by the layout pass before `DrawCore` is called.

> [!NOTE]
> Never override `MeasureCore` or `ArrangeCore`. Those handle margin and alignment boilerplate and delegate to your `MeasureOverride`/`ArrangeOverride`.


## Defining Properties

When defining properties on a custom control, use the right setter helper based on what the property affects:

| What changes | Setter to use |
|-------------|---------------|
| Size (e.g. Text, FontSize, Padding) | `SetPropertyAndInvalidateMeasure(ref field, value)` |
| Position only (e.g. Dock, Alignment) | `SetPropertyAndInvalidateArrange(ref field, value)` |
| Visuals only (e.g. Foreground color) | Direct assignment or `SetProperty()` |

```csharp
private Thickness _padding;

public Thickness Padding
{
    get => this._padding;
    set => this.SetPropertyAndInvalidateMeasure(ref this._padding, value);
}
```

Using the right invalidation level avoids unnecessary layout recalculations. A visual-only change doesn't need to re-measure.


## Input Handling

Controls receive input through overridable methods:

| Method | When it's called |
|--------|-----------------|
| `HandleMousePressed(MouseButtonEventArgs)` | Mouse button down on this control. |
| `HandleMouseReleased(MouseButtonEventArgs)` | Mouse button up. |
| `HandleMouseWheel(MouseWheelEventArgs)` | Scroll wheel. |
| `OnMouseEnter()` | Mouse enters the control's bounds. |
| `OnMouseLeave()` | Mouse leaves the control's bounds. |

Hit testing is automatic: `Control.HitTest` walks `VisualChildren` (front-to-back stacking order) and returns the topmost hit. To exclude a control from hit testing without hiding it, set `IsHitTestVisible = false`.

Controls also expose events for external subscribers: `MouseDown`, `MouseUp`, `MouseWheel`, `MouseEnter`, `MouseLeave`.


## The DrawingContext

These are the drawing primitives available inside `DrawCore`:

| Method | What it draws |
|--------|--------------|
| `ctx.DrawRectangle(position, size, color)` | Solid color rectangle. |
| `ctx.DrawTexture(texture, position, size)` | Texture. Multiple overloads for UVs and tint. |
| `ctx.DrawNinePatch(texture, position, size, margin, color, scale)` | 9-slice scalable texture. |
| `ctx.DrawText(font, text, position, color)` | Text string. |
| `ctx.PushOpacity(float)` / `PopOpacity()` | Multiply opacity for nested drawing. |
| `ctx.PushScissorRectangle(rect)` / `PopScissorRectangle()` | Clip drawing to a rectangle. |
