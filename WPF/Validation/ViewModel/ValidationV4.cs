using System.Collections;
using System.ComponentModel;
using System.Diagnostics;

namespace Validation.ViewModel;

/// <summary>
/// INotifyDataErrorInfo
/// 先校验，再更新 Source 的值
/// </summary>
public class ValidationV4 : ViewModelBase, INotifyDataErrorInfo
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

            string key = nameof(Age);
            _propertyErrors.Remove(key);
            if (value < 0 || value > 100)
            {
                List<string> errorList =
                [
                    $"年龄必须在0 - 100之间"
                ];
                _propertyErrors.TryAdd(key, errorList);
                //ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(key));
                //RaisePropertyChanged(nameof(HasErrors));
            }
        }
    }

    #region INotifyDataErrorInfo

    readonly Dictionary<string, List<string>> _propertyErrors = [];

    public bool HasErrors
    {
        get
        {
            Debug.WriteLine($"{DateTime.Now} 检查是否有 Error");
            return _propertyErrors.Count != 0;
        }
    }

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        Debug.WriteLine($"{DateTime.Now} 检查 {propertyName} Error");
        return _propertyErrors.GetValueOrDefault(propertyName ?? string.Empty, []);
    }

    #endregion
}

