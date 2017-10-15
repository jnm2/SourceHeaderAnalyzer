using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using SourceHeaderAnalyzer.UI;

namespace SourceHeaderAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class MissingHeaderTemplateFixProvider : CodeFixProvider
    {
        private static readonly Lazy<UserInterface> UserInterfaceProvider = new Lazy<UserInterface>(UserInterface.TryGet);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            if (context.Document.Project.FilePath != null)
            {
                var userInterface = UserInterfaceProvider.Value;
                if (userInterface != null)
                    context.RegisterCodeFix(new AddHeaderTemplateAction(context.Document.Project, userInterface), context.Diagnostics);
            }

            return Task.CompletedTask;
        }

        private sealed class AddHeaderTemplateAction : CodeAction
        {
            private readonly Project project;
            private readonly UserInterface userInterface;

            public AddHeaderTemplateAction(Project project, UserInterface userInterface)
            {
                this.project = project;
                this.userInterface = userInterface;
            }

            public override string Title => "Add a Header.cs.template file";

            protected override Task<IEnumerable<CodeActionOperation>> ComputePreviewOperationsAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult((IEnumerable<CodeActionOperation>)null);
            }

            protected override Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
            {
                var result = userInterface.SaveFile(new FileDialogOptions
                {
                    WindowTitle = "Find or create a header template file for the repository",
                    InitialDirectory = Path.GetDirectoryName(Path.GetDirectoryName(project.Solution.FilePath ?? project.FilePath)),
                    InitialFileName = "Header.cs.template",
                    Filters = new[]
                    {
                        new FileDialogFilter("C# header templates (*.cs.template)", "*.cs.template"),
                        new FileDialogFilter("All Files (*.*)", "*.*")
                    }
                });

                if (result == null) return Task.FromResult(project.Solution);

                var additionalDocument = project.AddAdditionalDocument(Path.GetFileName(result.FileName), string.Empty, null, result.FileName);

                return Task.FromResult(additionalDocument.Project.Solution);
            }
        }

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
            SourceHeaderAnalyzer.MissingHeaderTemplateDiagnostic.Id);
    }
}
