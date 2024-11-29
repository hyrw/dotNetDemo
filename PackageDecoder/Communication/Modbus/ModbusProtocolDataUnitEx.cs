using System.Buffers.Binary;
using System.Collections;
using System.ComponentModel;
using System.Net.Sockets;

namespace PackageDecoder.Communication.Modbus;

public static class ModbusProtocolDataUnitEx
{
    /// <summary>
    /// 0x01 读线圈状态（Read Coils）
    /// </summary>
    /// <param name="pdu"></param>
    /// <param name="quantity">数量</param>
    /// <returns></returns>
    public static ReadOnlySpan<byte> ReadCoilRegister(this ModbusProtocolDataUnit pdu, int quantity)
    {
        if (pdu.HasError) { return []; };

        var bitArray = new BitArray(pdu.Data.ToArray());
        var coils = new byte[quantity];
        for (var i = 0; i < coils.Length; i++)
        {
            coils[i] = (byte) (bitArray[i] ? 1 : 0);
        }
        return coils;
    }

    /// <summary>
    /// 0x02 读离散输入状态（Read Discrete Inputs ）
    /// </summary>
    /// <returns></returns>
    public static ReadOnlySpan<byte> ReadDiscreteInputs(this ModbusProtocolDataUnit pdu, int quantity)
    {
        return pdu.ReadDiscreteInputs(quantity);
    }

    /// <summary>
    /// 0x03 读保持寄存器（Read Holding Registers ）
    /// </summary>
    /// <param name="pdu"></param>
    /// <param name="quantity"></param>
    /// <returns></returns>
    public static ReadOnlySpan<ushort> ReadHoldingRegisters(this ModbusProtocolDataUnit pdu)
    {
        byte byteCount = pdu.Data[..1].Span[0];
        ushort[] span = new ushort[byteCount / 2];
        for (var i = 0; i< span.Length ; i++)
        {
            span[i] = BinaryPrimitives.ReadUInt16BigEndian(pdu.Data.Span.Slice(i, 2));
        }
        return span;
    }

    /// <summary>
    /// 0x04 读输入寄存器（Read Input Registers）
    /// </summary>
    /// <param name="pdu"></param>
    /// <param name="quantity"></param>
    /// <returns></returns>
    public static ReadOnlySpan<ushort> ReadInputRegisters(this ModbusProtocolDataUnit pdu)
    {
        return pdu.ReadHoldingRegisters();
    }

    /// <summary>
    /// 0x05 写单线圈（Write Single Coil ）
    /// </summary>
    /// <param name="pdu"></param>
    /// <returns></returns>
    public static ushort WriteSingleCoil(this ModbusProtocolDataUnit pdu)
    {
        return BinaryPrimitives.ReadUInt16BigEndian(pdu.Data.Span[..2]);
    }

    /// <summary>
    /// 0x06 写单寄存器（Write Single Register）
    /// </summary>
    /// <param name="pdu"></param>
    /// <returns></returns>
    public static (ushort Address, ushort Value) WriteSingleRegister(this ModbusProtocolDataUnit pdu)
    {
        (ushort address, ushort value) = pdu.GetAddressAndData();
        return (Address: address, Value: value);
    }

    /// <summary>
    /// 0x0F 写多个线圈（Write Multiple Coils）
    /// </summary>
    /// <param name="pdu"></param>
    /// <returns></returns>
    public static (ushort Address, ushort Quantity) WriteMultipleCoils(this ModbusProtocolDataUnit pdu)
    {
        (ushort address, ushort quantity) = pdu.GetAddressAndData();
        return (Address: address, Quantity: quantity);
    }
    /// <summary>
    /// 0x10 写多个寄存器（Write Multiple registers）
    /// </summary>
    /// <param name="pdu"></param>
    /// <returns></returns>
    public static (ushort Address, ushort Quantity) WriteMultipleRegisters(this ModbusProtocolDataUnit pdu)
    {
        (ushort address, ushort quantity) = pdu.GetAddressAndData();
        return (Address: address, Quantity: quantity);
    }

    private static (ushort Address, ushort Data) GetAddressAndData(this ModbusProtocolDataUnit pdu)
    {
        ushort address = BinaryPrimitives.ReadUInt16BigEndian(pdu.Data.Span[..2]);
        ushort data = BinaryPrimitives.ReadUInt16BigEndian(pdu.Data.Span.Slice(2, 2));
        return (Address: address, Data: data);
    }
}
