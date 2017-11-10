using System.Collections.Generic;
using System.Linq;

namespace StockMarket.Service.Validation
{
    public class ValidationResponse
    {
        public ValidationResponse(IEnumerable<ValidationResult> validationResults)
        {
            ValidationResults = validationResults ?? Enumerable.Empty<ValidationResult>();
        }
        
        public IEnumerable<ValidationResult> ValidationResults { get; }

        public bool IsValid => !ValidationResults.Any();
    }

    public class ValidationResponse<TResult> : ValidationResponse
    {
        public ValidationResponse(TResult result, IEnumerable<ValidationResult> validationResults)
            : base(validationResults)
        {
            Result = result;
        }

        public TResult Result { get; }
    }
}