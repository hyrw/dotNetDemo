using System.Diagnostics;
using System.Globalization;
using System.Windows.Controls;

namespace Validation.ValidationRules;

public class RangeRule : ValidationRule
{
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        Debug.WriteLine($"{DateTime.Now} 开始校验");
        if (!double.TryParse(value.ToString(), out double num))
        {
            return new ValidationResult(false, string.Empty);
        }
        if (num < MinValue || num > MaxValue)
        {
            return new ValidationResult(false, $"年龄必须在{MinValue} - {MaxValue}之间");
        }

        return ValidationResult.ValidResult;
    }
}
