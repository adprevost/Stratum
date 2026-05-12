// Stratum.Generator/DiagnosticDescriptors.cs
using Microsoft.CodeAnalysis;

namespace Stratum.Generator;

internal static class DiagnosticDescriptors
{
    private const string Category = "Stratum";

    public static readonly DiagnosticDescriptor STRATUM002_UnknownControlType = new DiagnosticDescriptor(
        "STRATUM002", "Unknown control type",
        "Unknown control type '{0}'{1}", Category,
        DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor STRATUM003_UnknownProperty = new DiagnosticDescriptor(
        "STRATUM003", "Unknown property",
        "Unknown property '{0}' on {1}{2}", Category,
        DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor STRATUM004_TabCharacter = new DiagnosticDescriptor(
        "STRATUM004", "Tab character in indentation",
        "Tab character detected — use spaces for indentation", Category,
        DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor STRATUM005_NoPageDeclaration = new DiagnosticDescriptor(
        "STRATUM005", "No page declaration",
        "No page declaration found. File must begin with 'page PageName'", Category,
        DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor STRATUM006_MultiplePageDeclarations = new DiagnosticDescriptor(
        "STRATUM006", "Multiple page declarations",
        "Multiple page declarations found in one file", Category,
        DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor STRATUM007_DuplicateControlName = new DiagnosticDescriptor(
        "STRATUM007", "Duplicate control name",
        "Duplicate control name '{0}' within page", Category,
        DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor STRATUM008_InvalidValue = new DiagnosticDescriptor(
        "STRATUM008", "Invalid value format",
        "Invalid value '{0}' for property '{1}'", Category,
        DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor STRATUM009_BadIndent = new DiagnosticDescriptor(
        "STRATUM009", "Bad indentation",
        "Control declaration at wrong indentation level (odd number of leading spaces)", Category,
        DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor STRATUM101_MissingAt = new DiagnosticDescriptor(
        "STRATUM101", "Missing 'at' property",
        "Control '{0}' has no 'at' property. Position defaults to 0,0", Category,
        DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor STRATUM102_UnnamedControl = new DiagnosticDescriptor(
        "STRATUM102", "Unnamed control",
        "Unnamed control of type '{0}' generates no field and cannot be referenced in code-behind", Category,
        DiagnosticSeverity.Warning, isEnabledByDefault: true);
}
