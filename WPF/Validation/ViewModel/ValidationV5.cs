using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Windows.Controls;

namespace Validation.ViewModel;

/// <summary>
/// DataAnnotation and Validator
/// 先校验，再更新 Source 的值
/// </summary>
public class ValidationV5 : ViewModelBase
{
    private int _age;

    [DisplayName("年龄")]
    [Range(0, 100, ErrorMessage = "{0}必须在0 - 100之间")]
    public int Age
    {
        get => _age;
        set
        {
            Debug.WriteLine($"{DateTime.Now} 设置值");
            Validator.ValidateProperty(value, new ValidationContext(this)
            {
                MemberName = nameof(Age),
            });
            //SetProperty(ref _age, value);
            _age = value;
        }
    }
}
