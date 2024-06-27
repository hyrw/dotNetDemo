using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ControlDemoApp.ViewModels;

internal partial class HPBarViewModel : ObservableObject
{
    readonly Random random = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RandomLossOfLifeCommand))]
    double hp = 500;

    public double MaxHP => 1000;

    [ObservableProperty]
    double lossLife;

    [RelayCommand(CanExecute = nameof(CanLossLife))]
    private void RandomLossOfLife()
    {
        LossLife = random.Next(0, 100);
        Hp -= LossLife > Hp ? Hp : LossLife;
    }
    bool CanLossLife => Hp > 0;

    [RelayCommand]
    private void ResetLife()
    {
        Hp = MaxHP;
        LossLife = 0;
    }

}
