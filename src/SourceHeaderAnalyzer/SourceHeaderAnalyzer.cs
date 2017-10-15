using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SourceHeaderAnalyzer.Templating;

namespace SourceHeaderAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SourceHeaderAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor MissingHeaderTemplateDiagnostic = new DiagnosticDescriptor(
            "SHA0000",
            "A header *.cs.template file must be added to this project.",
            "A header *.cs.template file must be added to this project. Create a file at the highest-level folder where the header applies and reference it in each project like this:\r\n" +
            "\r\n" +
            "  <ItemGroup>\r\n" +
            "    <AdditionalFiles Include=\"..\\..\\Header.cs.template\" />\r\n" +
            "  </ItemGroup>\r\n",
            "Codebase",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MisconfiguredHeaderTemplateDiagnostic = new DiagnosticDescriptor(
            "SHA0001",
            "Header *.cs.template file configuration is invalid.",
            "{0}",
            "Codebase",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor IncorrectHeaderDiagnostic = new DiagnosticDescriptor(
            "SHA0002",
            "The file does not have the correct header.",
            "The file does not have the correct header.",
            "Codebase",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MisplacedHeaderDiagnostic = new DiagnosticDescriptor(
            "SHA0003",
            "Nothing must come before the file header.",
            "Nothing must come before the file header.",
            "Codebase",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor OutdatedHeaderDiagnostic = new DiagnosticDescriptor(
            "SHA0004",
            "The header is not current.",
            "{0}",
            "Codebase",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor InvalidHeaderDiagnostic = new DiagnosticDescriptor(
            "SHA0005",
            "The header has invalid information.",
            "{0}",
            "Codebase",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            MissingHeaderTemplateDiagnostic,
            MisconfiguredHeaderTemplateDiagnostic,
            IncorrectHeaderDiagnostic,
            MisplacedHeaderDiagnostic,
            OutdatedHeaderDiagnostic,
            InvalidHeaderDiagnostic);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxTreeAction(OnSyntaxTreeParsed);
        }

        private static void OnSyntaxTreeParsed(SyntaxTreeAnalysisContext context)
        {
            var text = context.Tree.GetText(context.CancellationToken);

            var templateResult = GetHeaderTemplate(context, text);

            if (templateResult.TryGetItem1(out var headerTemplate))
            {
                if (headerTemplate != null)
                {
                    var currentValues = new DynamicTemplateValues(DateTime.Now.Year);

                    if (headerTemplate.TryMatch(text.ToString(), currentValues, out var result))
                    {
                        if (result.Start != 0)
                            context.ReportDiagnostic(Diagnostic.Create(MisplacedHeaderDiagnostic, Location.Create(context.Tree, new TextSpan(0, result.Start))));

                        if (result.IsInexact)
                            context.ReportDiagnostic(Diagnostic.Create(IncorrectHeaderDiagnostic, Location.Create(context.Tree, new TextSpan(result.Start, result.Length))));

                        foreach (var errorMessage in result.ErrorMessages)
                            context.ReportDiagnostic(Diagnostic.Create(InvalidHeaderDiagnostic, Location.Create(context.Tree, new TextSpan(result.Start, result.Length)), errorMessage));

                        foreach (var updateMessage in result.UpdateMessages)
                            context.ReportDiagnostic(Diagnostic.Create(OutdatedHeaderDiagnostic, Location.Create(context.Tree, new TextSpan(result.Start, result.Length)), updateMessage));
                    }
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(IncorrectHeaderDiagnostic, CreateTopLineLocation(context.Tree, text)));
                    }
                }
            }
            else if (templateResult.TryGetItem2(out var diagnostic))
            {
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static Location CreateTopLineLocation(SyntaxTree tree, SourceText text)
        {
            var endOfFirstNonWhitespaceLine = text.Lines
                .FirstOrDefault(_ => !string.IsNullOrWhiteSpace(_.ToString()))
                .End;

            return Location.Create(tree, new TextSpan(0, endOfFirstNonWhitespaceLine != 0 ? endOfFirstNonWhitespaceLine : text.Length));
        }

        private static OneOf<HeaderTemplate, Diagnostic> GetHeaderTemplate(SyntaxTreeAnalysisContext context, SourceText text)
        {
            var file = context.Options.AdditionalFiles.Where(_ => _.Path.EndsWith(".cs.template", StringComparison.Ordinal)).Take(2).ToList();
            switch (file.Count)
            {
                case 1:
                    break;

                case 0:
                    return Diagnostic.Create(
                        MissingHeaderTemplateDiagnostic,
                        CreateTopLineLocation(context.Tree, text));

                default:
                    return Diagnostic.Create(
                        MisconfiguredHeaderTemplateDiagnostic,
                        CreateTopLineLocation(context.Tree, text),
                        "More than one header *.cs.template file has been added to this project. Remove all but one of them from the project’s <AdditionalFiles> items.");
            }

            var fileText = file[0].GetText(context.CancellationToken);
            if (fileText.Length == 0)
            {
                if (!File.Exists(file[0].Path))
                {
                    return Diagnostic.Create(
                        MisconfiguredHeaderTemplateDiagnostic,
                        CreateTopLineLocation(context.Tree, text),
                        $"{file[0].Path} does not exist.");
                }

                return (HeaderTemplate)null;
            }

            return TemplateParser.Parse(new SourceTextReader(fileText)).Select(
                template => template,
                errorMessage => Diagnostic.Create(
                    MisconfiguredHeaderTemplateDiagnostic,
                    CreateTopLineLocation(context.Tree, text),
                    $"Error in {file[0].Path}: {errorMessage}"));
        }
    }
}
