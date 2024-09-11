using System;
using CommunityToolkit.Mvvm.ComponentModel;

using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using PackageDecoder.Communication.Modbus;

namespace AvaloniaApplication.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Loopback, 502);
    
    [ObservableProperty]
    ObservableCollection<RemarkAndValue> values = [];

    [ObservableProperty]
    bool enableFlash = true;

    [ObservableProperty]
    private int interval = 1000;

    private readonly ModbusCommunication modbus;

    public MainWindowViewModel()
    {
        this.modbus = new ModbusCommunication(ipEndPoint, ModbusType.TcpIp);
    }

    [RelayCommand]
    private async Task Start()
    {
        var readCoilRegister = ReadCoilRegister.Create(0, 1, ModbusFunctionCode.ReadCoil, 0x00, 10);
        while (true)
        {
            if (!this.modbus.Connected)
            {
                await this.modbus.ConnectAsync();
            }
            var result= await modbus.ReadCoilRegisterAsync(readCoilRegister);
            var coils = result.Coils;
            Dispatcher.UIThread.Post(()=>
            {
                Values.Clear();
                foreach (var t in coils)
                {
                    Values.Add(new RemarkAndValue(string.Empty, t));
                }
            });
            await Task.Delay(TimeSpan.FromMilliseconds(Interval));
        }
    }

}

public partial class RemarkAndValue(string remark, byte value) : ViewModelBase
{
    [ObservableProperty]
    private string remark = remark;

    [ObservableProperty]
    private byte value = value;
}
