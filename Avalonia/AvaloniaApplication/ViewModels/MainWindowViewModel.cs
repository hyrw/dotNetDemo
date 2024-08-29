using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AvaloniaApplication.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    readonly DispatcherTimer timer;
    readonly int tempCount = 2;

    [ObservableProperty]
    ObservableCollection<RemarkAndValue> values = [];

    [ObservableProperty]
    bool enableFlash = true;

    public MainWindowViewModel()
    {
        var list = new List<RemarkAndValue>(tempCount * sizeof(double));
        for (int i = 0; i < tempCount; i++)
        {
            double temp = 100;
            byte[] bytes = BitConverter.GetBytes(temp);
            for (int j = 0; j < bytes.Length; j++)
            {
                if (j == 0)
                {
                    list.Add(new RemarkAndValue($"temp{i + 1}", bytes[j]));
                }
                else
                {
                    list.Add(new RemarkAndValue(string.Empty, bytes[j]));
                }
            }
        }
        Values = new(list);
        this.timer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromSeconds(1),
        };
        this.timer.Tick += Timer_Tick;
        this.timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        int index = Random.Shared.Next(0, tempCount);
        index = sizeof(double) * index;
        double temp = Random.Shared.NextDouble() * 100;
        byte[] bytes = BitConverter.GetBytes(temp);
        for (int i = 0; i < bytes.Length; i++)
        {
            Values[index + i].Value = bytes[i];
        }
    }
}

public partial class RemarkAndValue(string Remark, byte Value) : ViewModelBase
{
    [ObservableProperty]
    public string remark = Remark;

    [ObservableProperty]
    public byte value = Value;
}
