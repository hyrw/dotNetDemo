using System.Diagnostics;

namespace Validation.ViewModel;

/// <summary>
/// ExceptionValidation
/// 先更新 Source
/// </summary>
public class ValidationV1 : ViewModelBase
{
    private int _age;

    public int Age
    {
        get => _age;
        set
        {
            Debug.WriteLine($"{DateTime.Now} 设置值");
            if (value < 0 || value > 100)
                throw new ArgumentException($"年龄必须在0 - 100之间");
            //SetProperty(ref _age, value);
            _age = value;
        }
    }

}
