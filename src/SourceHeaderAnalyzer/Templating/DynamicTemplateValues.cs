using System;

namespace SourceHeaderAnalyzer.Templating
{
    public struct DynamicTemplateValues
    {
        public DynamicTemplateValues(int currentYear)
        {
            if (currentYear < 1000 || currentYear > 9999) throw new ArgumentOutOfRangeException(nameof(currentYear), currentYear, "Current year must be between 1000 and 9999, inclusive.");
            CurrentYear = currentYear;
        }

        public int CurrentYear { get; }
    }
}
