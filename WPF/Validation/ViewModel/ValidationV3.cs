using System.Diagnostics;

namespace Validation.ViewModel;

/// <summary>
/// CustomerValidationRule
/// 先校验，再更新 Source 的值
/// </summary>
public class ValidationV3 : ViewModelBase
{
    private int _age;

    public int Age
    {
        get => _age;
        set
        {
            Debug.WriteLine($"{DateTime.Now} 设置值");
            //SetProperty(ref _age, value);
            _age = value;
        }
    }
}

