using System;
using System.Collections.Generic;

namespace LifeSim.Engine.Controls;

public class Style
{
    // Dictionary of (string, object). The method Apply(object) uses reflection to set the value of the property with the same name as the key.
    private static readonly Dictionary<Type, Style> _defaultStyles = new Dictionary<Type, Style>();

    public static Style Empty { get; } = new Style();

    public static void RegisterDefaultStyle<T>(Style style)
    {
        _defaultStyles[typeof(T)] = style;
    }

    public static Style? GetDefaultStyle<T>()
    {
        return GetDefaultStyle(typeof(T));
    }

    public static Style? GetDefaultStyle(Type type)
    {
        // Use the whole hierarchy of T to register the default style, from more specific to more general.
        Type? currentType = type;
        while (currentType != null)
        {
            if (_defaultStyles.TryGetValue(currentType, out var style))
            {
                return style;
            }

            currentType = currentType.BaseType;
        }

        return Empty;
    }

    private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

    public Style? BasedOn { get; set; }

    public Type? TargetType { get; }

    public Style()
    {

    }

    public Style(Type targetType)
    {
        this.TargetType = targetType;
    }

    public Style(Type targetType, Style? basedOn)
    {
        this.TargetType = targetType;
        this.BasedOn = basedOn;
    }


    public object this[string key]
    {
        get
        {
            if (this._properties.TryGetValue(key, out object? value))
            {
                return value;
            }

            if (this.BasedOn != null)
            {
                return this.BasedOn[key];
            }

            throw new KeyNotFoundException($"Property '{key}' not found in the style.");
        }
        set => this._properties[key] = value;
    }

    public void Apply(object target)
    {
        this.BasedOn?.Apply(target);

        foreach (var property in this._properties)
        {
            var targetProperty = target.GetType().GetProperty(property.Key);
            if (targetProperty != null)
            {
                targetProperty.SetValue(target, property.Value);
            }
        }
    }



    public void Copy(Style destination)
    {
        if (this.BasedOn != null)
        {
            this.BasedOn.Copy(destination);
        }

        foreach (var property in this._properties)
        {
            destination[property.Key] = property.Value;
        }
    }

    public Style Clone()
    {
        var clone = new Style();
        this.Copy(clone);
        return clone;
    }

    public static Style Merge(Style style1, Style style2)
    {
        var merged = new Style();
        style1.Copy(merged);
        style2.Copy(merged);
        return merged;
    }
}