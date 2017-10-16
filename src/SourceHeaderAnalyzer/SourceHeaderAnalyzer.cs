using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

            var templateResult =
                AnalyzerAndFixAbstraction.GetHeaderTemplate(
                    context.Options.AdditionalFiles,
                    _ => _.Path,
                    _ => Task.FromResult(_.GetText(context.CancellationToken)),
                    () => CreateTopLineLocation(context.Tree, text))
                .AssertCompletedSynchronously();

            if (templateResult.TryGetItem1(out var headerTemplate))
            {
                if (headerTemplate != null)
                {
                    var currentValues = AnalyzerAndFixAbstraction.GetCurrentValues();

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
                        context.ReportDiagnostic(Diagnostic.Create(
                            IncorrectHeaderDiagnostic,
                            CreateTopLineLocation(context.Tree, text),
                            new Dictionary<string, string> { [ReplaceHeaderFixProvider.IsInsertOnlyProperty] = "True" }.ToImmutableDictionary()));
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
    }
}
