using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Imago.Generators.Parsing;

/// <summary>
/// Represents a source region in a template XML file.
/// </summary>
internal readonly struct SourceSpan : IEquatable<SourceSpan>
{
    /// <summary>
    /// Gets the file path.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets the 1-based start line number.
    /// </summary>
    public int Line { get; }

    /// <summary>
    /// Gets the 1-based start column number.
    /// </summary>
    public int Column { get; }

    /// <summary>
    /// Gets the 1-based end line number.
    /// </summary>
    public int EndLine { get; }

    /// <summary>
    /// Gets the 1-based exclusive end column number.
    /// </summary>
    public int EndColumn { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceSpan"/> struct with a zero-length span.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="line">The 1-based line number.</param>
    /// <param name="column">The 1-based column number.</param>
    public SourceSpan(string filePath, int line, int column)
        : this(filePath, line, column, line, column)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceSpan"/> struct.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="line">The 1-based start line number.</param>
    /// <param name="column">The 1-based start column number.</param>
    /// <param name="endLine">The 1-based end line number.</param>
    /// <param name="endColumn">The 1-based exclusive end column number.</param>
    public SourceSpan(string filePath, int line, int column, int endLine, int endColumn)
    {
        this.FilePath = filePath;
        this.Line = line;
        this.Column = column;
        this.EndLine = endLine;
        this.EndColumn = endColumn;
    }

    /// <summary>
    /// Converts this span to a Roslyn <see cref="Location"/> pointing into the template file.
    /// </summary>
    /// <returns>A Roslyn location.</returns>
    public Location ToLocation()
    {
        var start = new LinePosition(this.Line - 1, this.Column - 1);
        var end = new LinePosition(this.EndLine - 1, this.EndColumn - 1);
        var span = new LinePositionSpan(start, end);
        return Location.Create(this.FilePath, new TextSpan(0, 0), span);
    }

    /// <inheritdoc />
    public bool Equals(SourceSpan other)
    {
        return this.Line == other.Line &&
               this.Column == other.Column &&
               this.EndLine == other.EndLine &&
               this.EndColumn == other.EndColumn &&
               this.FilePath == other.FilePath;
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        return obj is SourceSpan other && this.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + this.Line;
            hash = hash * 31 + this.Column;
            hash = hash * 31 + this.EndLine;
            hash = hash * 31 + this.EndColumn;
            hash = hash * 31 + (this.FilePath?.GetHashCode() ?? 0);
            return hash;
        }
    }

    /// <summary>
    /// Determines whether two <see cref="SourceSpan"/> instances are equal.
    /// </summary>
    public static bool operator ==(SourceSpan left, SourceSpan right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="SourceSpan"/> instances are not equal.
    /// </summary>
    public static bool operator !=(SourceSpan left, SourceSpan right) => !left.Equals(right);
}
