using System.Text;
using LifeSim.Imago.Generators.Parsing;

namespace LifeSim.Imago.Generators.Emitting;

/// <summary>
/// Writes indented C# source code with automatic <c>#line</c> directive management.
/// Only emits <c>#line</c> when the source span changes, and <c>#line default</c>
/// when transitioning from mapped to unmapped code.
/// </summary>
internal sealed class SourceWriter
{
    private readonly StringBuilder _sb = new StringBuilder();
    private readonly string _filePath;
    private readonly bool _useEnhancedLine;
    private SourceSpan? _activeSpan;

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceWriter"/> class.
    /// </summary>
    /// <param name="filePath">The template file path used in <c>#line</c> directives.</param>
    /// <param name="useEnhancedLine">Whether to use C# 10+ enhanced <c>#line</c> directives with column info.</param>
    public SourceWriter(string filePath, bool useEnhancedLine = false)
    {
        this._filePath = filePath;
        this._useEnhancedLine = useEnhancedLine;
    }

    /// <summary>
    /// Gets or sets the current indentation level. Each level equals 4 spaces.
    /// </summary>
    public int Indentation { get; set; }

    /// <summary>
    /// Writes an indented line mapped to a source location in the template file.
    /// </summary>
    /// <param name="span">The source span to map to, or <c>null</c> for unmapped code.</param>
    /// <param name="text">The code text to write.</param>
    public void WriteLine(SourceSpan? span, string text)
    {
        if (span != null && span.Value.Line > 0)
        {
            if (this._activeSpan == null ||
                this._activeSpan.Value.Line != span.Value.Line ||
                this._activeSpan.Value.Column != span.Value.Column)
            {
                EmitLineDirective(span.Value);
                this._activeSpan = span;
            }
        }
        else if (this._activeSpan != null)
        {
            this._sb.AppendLine("#line default");
            this._activeSpan = null;
        }

        WriteIndented(text);
    }

    /// <summary>
    /// Writes an indented line with no source mapping.
    /// </summary>
    /// <param name="text">The code text to write.</param>
    public void WriteLine(string text)
    {
        this.WriteLine(null, text);
    }

    /// <summary>
    /// Writes a blank line with no source mapping.
    /// </summary>
    public void WriteLine()
    {
        if (this._activeSpan != null)
        {
            this._sb.AppendLine("#line default");
            this._activeSpan = null;
        }

        this._sb.AppendLine();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return this._sb.ToString();
    }

    private void EmitLineDirective(SourceSpan span)
    {
        if (this._useEnhancedLine)
        {
            var charOffset = this.Indentation * 4;
            this._sb.AppendLine(
                $"#line ({span.Line},{span.Column}) - ({span.EndLine},{span.EndColumn}) {charOffset} \"{this._filePath}\""
            );
        }
        else
        {
            this._sb.AppendLine($"#line {span.Line} \"{this._filePath}\"");
        }
    }

    private void WriteIndented(string text)
    {
        for (var i = 0; i < this.Indentation; i++)
        {
            this._sb.Append("    ");
        }

        this._sb.AppendLine(text);
    }
}
