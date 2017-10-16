using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SourceHeaderAnalyzer.Templating;

namespace SourceHeaderAnalyzer
{
    public static class AnalyzerAndFixAbstraction
    {
        public static DynamicTemplateValues GetCurrentValues() => new DynamicTemplateValues(DateTime.Now.Year);

        public static async Task<OneOf<HeaderTemplate, Diagnostic>> GetHeaderTemplate<T>(
            IEnumerable<T> files,
            Func<T, string> filePathSelector,
            Func<T, Task<SourceText>> textSelector,
            Func<Location> diagnosticLocationFactory)
        {
            if (files == null) throw new ArgumentNullException(nameof(files));
            if (filePathSelector == null) throw new ArgumentNullException(nameof(filePathSelector));
            if (textSelector == null) throw new ArgumentNullException(nameof(textSelector));
            if (diagnosticLocationFactory == null) throw new ArgumentNullException(nameof(diagnosticLocationFactory));

            var templateFiles = files.Where(file => filePathSelector.Invoke(file)?.EndsWith(".cs.template", StringComparison.Ordinal) == true).Take(2).ToList();
            switch (templateFiles.Count)
            {
                case 1:
                    break;

                case 0:
                    return Diagnostic.Create(
                        SourceHeaderAnalyzer.MissingHeaderTemplateDiagnostic,
                        diagnosticLocationFactory.Invoke());

                default:
                    return Diagnostic.Create(
                        SourceHeaderAnalyzer.MisconfiguredHeaderTemplateDiagnostic,
                        diagnosticLocationFactory.Invoke(),
                        "More than one header *.cs.template file has been added to this project. Remove all but one of them from the project’s <AdditionalFiles> items.");
            }

            var templateFile = templateFiles[0];

            var fileText = await textSelector.Invoke(templateFile);
            if (fileText.Length == 0)
            {
                var filePath = filePathSelector.Invoke(templateFile);
                if (!File.Exists(filePath))
                {
                    return Diagnostic.Create(
                        SourceHeaderAnalyzer.MisconfiguredHeaderTemplateDiagnostic,
                        diagnosticLocationFactory.Invoke(),
                        $"{filePath} does not exist.");
                }

                return (HeaderTemplate)null;
            }

            return TemplateParser.Parse(new SourceTextReader(fileText)).Select(
                template => template,
                errorMessage => Diagnostic.Create(
                    SourceHeaderAnalyzer.MisconfiguredHeaderTemplateDiagnostic,
                    diagnosticLocationFactory.Invoke(),
                    $"Error in {filePathSelector.Invoke(templateFile)}: {errorMessage}"));
        }
    }
}
