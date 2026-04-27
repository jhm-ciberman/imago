namespace Imago.Generators;

/// <summary>
/// Fully qualified names of Imago types referenced by the source generator,
/// either for Roslyn symbol lookup or for emission into generated C# code.
/// </summary>
internal static class KnownSymbols
{
    /// <summary>The <c>[ItemsProperty]</c> attribute that marks a multi-child collection slot.</summary>
    public const string ItemsPropertyAttribute = "Imago.Controls.ItemsPropertyAttribute";

    /// <summary>The <c>[ItemsMethod]</c> attribute that marks an additive method for children.</summary>
    public const string ItemsMethodAttribute = "Imago.Controls.ItemsMethodAttribute";

    /// <summary>The <c>[ContentProperty]</c> attribute that marks a single-child slot.</summary>
    public const string ContentPropertyAttribute = "Imago.Controls.ContentPropertyAttribute";

    /// <summary>The <c>[TextProperty]</c> attribute that marks the property receiving inline text.</summary>
    public const string TextPropertyAttribute = "Imago.Controls.TextPropertyAttribute";

    /// <summary>The <c>[FactoryTemplate]</c> attribute that marks a factory-template type like <c>DataTemplate&lt;T&gt;</c>.</summary>
    public const string FactoryTemplateAttribute = "Imago.Controls.FactoryTemplateAttribute";

    /// <summary>The <c>IDataTemplate</c> interface implemented by every data template, including the polymorphic composite.</summary>
    public const string DataTemplateInterface = "Imago.Controls.IDataTemplate";

    /// <summary>The <c>IMountable</c> interface required by templates that declare bindings.</summary>
    public const string MountableInterface = "Imago.SceneGraph.IMountable";

    /// <summary>The <c>DataTemplates</c> composite type emitted when a template slot has multiple <c>DataTemplate</c> branches.</summary>
    public const string DataTemplatesType = "Imago.Controls.DataTemplates";

    /// <summary>The disposable wrapper emitted for template-binding teardown.</summary>
    public const string TemplateBindingDisposableType = "Imago.Controls.TemplateBindingDisposable";
}
