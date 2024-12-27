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
    private readonly IPEndPoint ipEndPoint = new(IPAddress.Loopback, 502);

    [ObservableProperty]
    ObservableCollection<RemarkAndValue> values = [];

    [ObservableProperty]
    bool enableFlash = true;

    private TimeSpan interval = TimeSpan.FromSeconds(1);

    private ModbusCommunication? modbus;
    private bool flag;

    [RelayCommand]
    private async Task Start()
    {
        flag = true;
        modbus ??= new ModbusCommunication(ipEndPoint, ModbusType.TcpIp);
        var readCoilRegister = ReadCoilRegister.Create(0, 1, ModbusFunctionCode.ReadCoil, 0x00, 10);
        while (flag)
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
            await Task.Delay(interval);
        }
    }

    [RelayCommand]
    private async Task StopMonitor()
    {
        flag = false;
        if (modbus is not null) await modbus.DisconnectAsync();
        this.modbus = null;
    }
}

public partial class RemarkAndValue(string remark, byte value) : ViewModelBase
{
    [ObservableProperty]
    private string remark = remark;

    [ObservableProperty]
    private byte value = value;
}
