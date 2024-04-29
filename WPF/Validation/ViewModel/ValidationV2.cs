using System.ComponentModel;
using System.Diagnostics;

namespace Validation.ViewModel;

/// <summary>
/// IDataErrorInfo
/// 先设置 Source 的值，再校验某个属性是否有错误
/// </summary>
public class ValidationV2 : ViewModelBase, IDataErrorInfo
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

    #region IDataErrorInfo

    public string Error => string.Empty;

    public string this[string columnName]
    {
        get
        {
            Debug.WriteLine($"{DateTime.Now} 检查 {columnName} Error");
            return columnName switch
            {
                nameof(Age) when Age < 0 || Age > 100 => $"年龄必须在0 - 100之间",
                _ => string.Empty
            };

        }
    }
    #endregion
}
