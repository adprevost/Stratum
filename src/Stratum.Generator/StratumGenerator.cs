// Stratum.Generator/StratumGenerator.cs
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Stratum.Generator;

[Generator]
public class StratumGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var stratumFiles = context.AdditionalTextsProvider
            .Where(f => f.Path.EndsWith(".stratum", StringComparison.OrdinalIgnoreCase));

        context.RegisterSourceOutput(stratumFiles, (spc, file) =>
        {
            var text = file.GetText()?.ToString();
            if (text == null) return;

            var parser = new StratumParser(file.Path);
            var page = parser.Parse(text);

            foreach (var diag in parser.Diagnostics)
                spc.ReportDiagnostic(diag);

            if (page == null) return;

            var source = GenerateSource(page);
            var hintName = System.IO.Path.GetFileName(file.Path) + ".g.cs";
            spc.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
        });
    }

    private static string GenerateSource(StratumPage page)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// AUTO-GENERATED — DO NOT EDIT");
        sb.AppendLine($"// Source: {page.Name}.stratum");
        sb.AppendLine();
        sb.AppendLine("using Stratum.Core;");
        sb.AppendLine("using Stratum.Controls;");
        sb.AppendLine();
        sb.AppendLine($"public partial class {page.Name} : Page");
        sb.AppendLine("{");

        // Field declarations
        foreach (var ctrl in page.Controls)
        {
            if (string.IsNullOrEmpty(ctrl.Name)) continue;
            sb.AppendLine($"    protected {ctrl.Type} {ctrl.Name} = null!;");
        }
        sb.AppendLine();

        // InitializeControls override
        sb.AppendLine("    protected override void InitializeControls()");
        sb.AppendLine("    {");

        foreach (var ctrl in page.Controls)
        {
            var (x, y, w, h) = PropertyMapper.GetPositionAndSize(ctrl);
            var initProps = PropertyMapper.MapProperties(ctrl);
            var finalProps = initProps;

            string varName = string.IsNullOrEmpty(ctrl.Name) ? "_unnamed" : ctrl.Name;

            string ctorArgs;
            switch (ctrl.Type)
            {
                case "Label":
                {
                    string text = ctrl.Properties.TryGetValue("text", out var tv) ? tv : "\"\"";
                    if (!text.StartsWith("\"")) text = $"\"{text}\"";
                    ctorArgs = $"{text}, {x}, {y}, {w}, {h}";
                    // Remove text from initProps since it's in ctor
                    finalProps.RemoveAll(p => p.StartsWith("Text ="));
                    break;
                }
                case "Button":
                {
                    string text = ctrl.Properties.TryGetValue("text", out var tv) ? tv : "\"\"";
                    if (!text.StartsWith("\"")) text = $"\"{text}\"";
                    ctorArgs = $"{text}, {x}, {y}, {w}, {h}";
                    finalProps.RemoveAll(p => p.StartsWith("Text ="));
                    break;
                }
                case "TextBox":
                    ctorArgs = $"{x}, {y}, {w}, {h}";
                    break;
                case "CheckBox":
                {
                    string text = ctrl.Properties.TryGetValue("text", out var tv) ? tv : "\"\"";
                    if (!text.StartsWith("\"")) text = $"\"{text}\"";
                    ctorArgs = $"{text}, {x}, {y}, {w}, {h}";
                    finalProps.RemoveAll(p => p.StartsWith("Text ="));
                    break;
                }
                case "DataGrid":
                case "Panel":
                case "ProgressBar":
                case "Tabs":
                case "SidebarNav":
                    ctorArgs = $"{x}, {y}, {w}, {h}";
                    break;
                case "Modal":
                    ctorArgs = "";
                    break;
                default:
                    ctorArgs = $"{x}, {y}, {w}, {h}";
                    break;
            }

            // Remove style from finalProps — it's a full assignment already
            // and MapButtonStyle returns the full "Style = ButtonStyle.X" string
            // (no duplicate removal needed; just make sure it's not duplicated)

            string assignment;
            if (string.IsNullOrEmpty(ctrl.Name))
                assignment = $"        new {ctrl.Type}({ctorArgs})";
            else
                assignment = $"        {ctrl.Name} = new {ctrl.Type}({ctorArgs})";

            if (finalProps.Count > 0)
            {
                sb.AppendLine($"{assignment}");
                sb.AppendLine("        {");
                foreach (var prop in finalProps)
                    sb.AppendLine($"            {prop},");
                sb.AppendLine("        };");
            }
            else
            {
                sb.AppendLine($"{assignment};");
            }

            // DataGrid columns
            if (ctrl.Type == "DataGrid" && ctrl.Columns.Count > 0 && !string.IsNullOrEmpty(ctrl.Name))
            {
                foreach (var col in ctrl.Columns)
                    sb.AppendLine($"        {ctrl.Name}.Columns.Add(new DataGridColumn {{ Header = \"{col.Header}\", Width = {col.Width} }});");
            }

            // Tabs entries
            if (ctrl.Type == "Tabs" && ctrl.Tabs.Count > 0 && !string.IsNullOrEmpty(ctrl.Name))
            {
                foreach (var t in ctrl.Tabs)
                    sb.AppendLine($"        {ctrl.Name}.TabList.Add(\"{t}\");");
                sb.AppendLine($"        {ctrl.Name}.EnsureActive();");
            }

            // SidebarNav entries
            if (ctrl.Type == "SidebarNav" && ctrl.NavEntries.Count > 0 && !string.IsNullOrEmpty(ctrl.Name))
            {
                foreach (var e in ctrl.NavEntries)
                {
                    string kind = e.IsGroup ? "Group" : "Item";
                    sb.AppendLine($"        {ctrl.Name}.Items.Add(new SidebarNavEntry {{ Kind = NavEntryKind.{kind}, Label = \"{e.Label}\" }});");
                }
                // Default ActiveItem to first NavItem
                foreach (var e in ctrl.NavEntries)
                {
                    if (!e.IsGroup)
                    {
                        sb.AppendLine($"        {ctrl.Name}.ActiveItem = \"{e.Label}\";");
                        break;
                    }
                }
            }

            sb.AppendLine();
        }

        // Add() / Register calls
        foreach (var ctrl in page.Controls)
        {
            if (string.IsNullOrEmpty(ctrl.Name)) continue;
            if (ctrl.Type == "Modal")
                sb.AppendLine($"        RegisterModal({ctrl.Name});");
            else
                sb.AppendLine($"        Add({ctrl.Name});");
        }

        // Always register a ToastManager so any Page can show toasts.
        sb.AppendLine("        if (Stratum.Core.Application.Current!.ToastHost == null)");
        sb.AppendLine("            Stratum.Core.Application.Current.RegisterToastHost(new ToastManager());");

        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }
}
