using System.Collections.Generic;
using System.Linq;

namespace _Core.Editor.ScenarioValidator
{
    public sealed class ValidationReport
    {
        private readonly List<ValidationResult> _results = new List<ValidationResult>();

        public IReadOnlyList<ValidationResult> Results => _results;
        public int ErrorCount => _results.Count(x => x.Severity == ValidationSeverity.Error);
        public int WarningCount => _results.Count(x => x.Severity == ValidationSeverity.Warning);
        public int InfoCount => _results.Count(x => x.Severity == ValidationSeverity.Info);

        public void Add(ValidationResult result)
        {
            if (result != null)
                _results.Add(result);
        }
    }
}