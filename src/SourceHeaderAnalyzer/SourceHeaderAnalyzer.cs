using System;
using System.Collections.Immutable;
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
        private static readonly DiagnosticDescriptor IncorrectHeaderDiagnostic = new DiagnosticDescriptor(
            "SHA0001",
            "The file does not have the correct header.",
            "The file does not have the correct header.",
            "Codebase",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor MisplacedHeaderDiagnostic = new DiagnosticDescriptor(
            "SHA0002",
            "Nothing must come before the file header.",
            "Nothing must come before the file header.",
            "Codebase",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor OutdatedHeaderDiagnostic = new DiagnosticDescriptor(
            "SHA0003",
            "The header is not current.",
            "{0}",
            "Codebase",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor InvalidHeaderDiagnostic = new DiagnosticDescriptor(
            "SHA0004",
            "The header has invalid information.",
            "{0}",
            "Codebase",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
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

            if (GetHeaderTemplate().TryMatch(text.ToString(), out var result))
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
                var endOfFirstNonWhitespaceLine = text.Lines
                    .FirstOrDefault(_ => !string.IsNullOrWhiteSpace(_.ToString()))
                    .End;

                context.ReportDiagnostic(Diagnostic.Create(IncorrectHeaderDiagnostic, Location.Create(context.Tree,
                    new TextSpan(0, endOfFirstNonWhitespaceLine != 0 ? endOfFirstNonWhitespaceLine : text.Length))));
            }
        }

        private static HeaderTemplate GetHeaderTemplate()
        {
            return new HeaderTemplate(new TemplateSegment[]
            {
                new TextTemplateSegment("// Copyright © "),
                new YearTemplateSegment(DateTime.Now.Year),
                new TextTemplateSegment(" (range: "),
                new YearRangeTemplateSegment(DateTime.Now.Year - 1, DateTime.Now.Year),
                new TextTemplateSegment(")")
            }.ToImmutableArray());
        }
    }
}
