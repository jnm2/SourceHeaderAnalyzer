using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace SourceHeaderAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class ReplaceHeaderFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
            SourceHeaderAnalyzer.IncorrectHeaderDiagnostic.Id,
            SourceHeaderAnalyzer.OutdatedHeaderDiagnostic.Id,
            SourceHeaderAnalyzer.InvalidHeaderDiagnostic.Id);

        public const string IsInsertOnlyProperty = "IsInsertOnly";

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var updateLocation =
                context.Diagnostics
                    .Where(_ => !_.Properties.ContainsKey(IsInsertOnlyProperty))
                    .Select(_ => (TextSpan?)_.Location.SourceSpan)
                    .FirstOrDefault();

            var title = updateLocation == null ? "Insert header" : "Update header";

            context.RegisterCodeFix(CodeAction.Create(title, c => InsertOrUpdateHeader(context.Document, updateLocation, c)), context.Diagnostics);

            return Task.CompletedTask;
        }

        private static async Task<Document> InsertOrUpdateHeader(Document document, TextSpan? updateLocation, CancellationToken cancellationToken)
        {
            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var templateResult = await AnalyzerAndFixAbstraction.GetHeaderTemplate(
                document.Project.AdditionalDocuments,
                _ => _.FilePath,
                _ => _.GetTextAsync(cancellationToken),
                () => null)
                .ConfigureAwait(false);

            if (templateResult.TryGetItem2(out _))
                return document;

            var template = templateResult.Item1;

            var updatedTextBuilder = new StringBuilder();
            template.Evaluate(AnalyzerAndFixAbstraction.GetCurrentValues(), updatedTextBuilder, text.ToString());

            return document.WithText(text.WithChanges(new TextChange(updateLocation ?? new TextSpan(0, 0), updatedTextBuilder.ToString())));
        }
    }
}
