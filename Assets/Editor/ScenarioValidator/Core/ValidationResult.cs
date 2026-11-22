using UnityEngine;

namespace _Core.Editor.ScenarioValidator
{
    public sealed class ValidationResult
    {
        public string Code { get; }
        public string Message { get; }
        public ValidationSeverity Severity { get; }
        public Object Target { get; }

        public ValidationResult(string code, string message, ValidationSeverity severity, Object target)
        {
            Code = code;
            Message = message;
            Severity = severity;
            Target = target;
        }
    }
}