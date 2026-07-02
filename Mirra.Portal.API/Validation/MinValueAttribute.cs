using System.ComponentModel.DataAnnotations;

namespace Mirra_Portal_API.Validation
{
    public class MinValueAttribute : ValidationAttribute
    {
        private readonly int _minimum;

        public MinValueAttribute(int minimum)
        {
            _minimum = minimum;
        }

        public override bool IsValid(object value)
        {
            if (value is null) return true;
            return value is int number && number >= _minimum;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The field {name} must be greater than or equal to {_minimum}.";
        }
    }
}
