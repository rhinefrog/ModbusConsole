using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using coptions;
using Modbus.Device;
using OptionAttribute = coptions.OptionAttribute;


[ApplicationInfo(Help = "Example: ModConsole.exe -c com8 -a 1 -f 3 -s 2340 -q 4 -i ")]
public class Options
{
	[Option('c', "comport", "COMPORT", Help = "Comport Windows \"COM1\" or Linux \"/dev/ttySx\" ")]
	public string Comport
	{
		get { return _comport; }
		set
		{
			if (String.IsNullOrWhiteSpace(value))
				throw new InvalidOptionValueException("Comport must not be blank");
			_comport = value;
		}
	}
	private string _comport;

	[Option('f', "function", "FUNCTION", Help = "Choose a function ")]
	public string Mfunction
	{
		get { return _mfunction; }
		set
		{
			_mfunction = value;
		}
	}
	private string _mfunction;

	[Option('a', "address", "DEVICE_ID", Help = "Choose a device address ")]
    public int Address
    {
        get { return _address; }
        set
        {
            _address = value;
        }
    }
    private int _address;

    [Option('s', "saddress", "STARTADDRESS", Help = "Choose a start address ")]
	public int Saddress
	{
		get { return _saddress; }
		set
		{
			_saddress = value;
		}
	}
	private int _saddress;

	[Option('q', "qaddresses", "QUANTYADDRESSES", Help = "Quantity of addresses")]
	public int Qtyaddresses
	{
		get { return _qtyaddresses; }
		set
		{
			_qtyaddresses = value;
		}
	}
	private int _qtyaddresses;

	[Flag('i', "ieee754", Help = "If IEEE754 required -i")]
	public bool Ieee754;

    [Flag('d', "debug", Help = "If Debug required -d")]
    public bool DPdebug;

    [Option('w', "write", "WRITE", Help = "Value write")]
    public int DPValue
    {
        get { return _dpvalue; }
        set
        {
            _dpvalue = value;
        }
    }
    private int _dpvalue;
}

namespace ModConsole
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                Options opt = CliParser.Parse<Options>(args);
                ModbusSerialMaster _modbusMaster;
                SerialPort _serialPort;
                _serialPort = new SerialPort(opt.Comport, 9600, Parity.None, 8, StopBits.One);
                _serialPort.Open();

                _modbusMaster = ModbusSerialMaster.CreateRtu(_serialPort);
                _modbusMaster.Transport.ReadTimeout = 500;
                _modbusMaster.Transport.WriteTimeout = 500;
                _modbusMaster.Transport.Retries = 0;
                try
                {

                    if (opt.Mfunction == "1")
                    {
                        // ReadCoils = 0x01,                   // FC01
                        try
                        {
                            bool[] result = _modbusMaster.ReadCoils(Convert.ToByte(opt.Address), (ushort)opt.Saddress, (ushort)opt.Qtyaddresses);
                            if (result[0])
                            {
                                Console.WriteLine("true");
                            }
                            else
                            {
                                Console.WriteLine("false");
                            }
                            if (opt.DPdebug == true) Console.WriteLine("Debug: " + result);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                    if (opt.Mfunction == "2")
                    {
                        // ReadDiscreteInputs = 0x02,          // FC02
                        try
                        {
                            bool[] result = _modbusMaster.ReadInputs(Convert.ToByte(opt.Address), (ushort)opt.Saddress, (ushort)opt.Qtyaddresses);
                            if (result[0])
                            {
                                Console.WriteLine("true");
                            }
                            else
                            {
                                Console.WriteLine("false");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                    if (opt.Mfunction == "3")
                    {
                        // ReadHoldingRegisters = 0x03,        // FC03
                        try
                        {
                            ushort[] result = _modbusMaster.ReadHoldingRegisters(Convert.ToByte(opt.Address), (ushort)opt.Saddress, (ushort)opt.Qtyaddresses);
                            var f = FromHexString(result[0].ToString("X4") + result[1].ToString("X4"));
                            if (opt.Ieee754 == true) Console.WriteLine(f.ToString());
                            if (opt.DPdebug == true) Console.WriteLine(result[0].ToString("X4") + result[1].ToString("X4"));
                            if (!opt.Ieee754) Console.WriteLine(result[0].ToString() + result[1].ToString());
                        }
                        catch (Exception ex)
                        {
                            //The server return error code.You can get the function code and exception code.
                            if (ex.Message.Equals("The operation has timed out."))
                            {
                                Console.WriteLine("Address:"+opt.Address+" existiert nöd");

                            }
                            else
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }

                    }

                    if (opt.Mfunction == "4")
                    {
                        // ReadInputRegisters = 0x04,          // FC04
                        try
                        {
                            ushort[] result = _modbusMaster.ReadInputRegisters(Convert.ToByte(opt.Address), (ushort)opt.Saddress, (ushort)opt.Qtyaddresses);
                            var f = FromHexString(result[0].ToString("X4") + result[1].ToString("X4"));
                            if (opt.Ieee754 == true) Console.WriteLine(f);
                            if (opt.DPdebug == true) Console.WriteLine(result[0].ToString("X4") + result[1].ToString("X4"));
                            if (!opt.Ieee754) Console.WriteLine(result[0].ToString() + result[1].ToString());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                    if (opt.Mfunction == "5")
                    {
                        // WriteSingleCoil = 0x05,             // FC0
                        try
                        {
                            bool wert = false;
                            if (opt.DPValue == 1) wert = true;
                            if (opt.DPValue == 0) wert = false;
                            _modbusMaster.WriteSingleCoil(Convert.ToByte(opt.Address), (ushort)opt.Saddress, wert);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                    if (opt.Mfunction == "16")
                    {
                        // WriteMultipleRegisters = 0x10,      // FC16
                        int h = opt.DPValue;
                        float hilf = (float)h;
                        hilf = hilf / 10;
                        var tempo = ToHexString(hilf);
                        //Console.WriteLine(hilf);
                        //Console.WriteLine(tempo.ToString());
                        if (opt.DPdebug == true) Console.WriteLine(tempo.Substring(2, 8));
                        ushort[] registers = new ushort[2] { 1, 2 };
                        registers[0] = (ushort)Convert.ToInt16(tempo.Substring(2, 4), 16);
                        registers[1] = (ushort)Convert.ToInt16(tempo.Substring(2 + 4, 4), 16);
                        _modbusMaster.WriteMultipleRegisters(Convert.ToByte(opt.Address), (ushort)opt.Saddress, registers);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                //                client.Close();
                _modbusMaster.Dispose();
                _modbusMaster = null;

                _serialPort.Close();
                _serialPort.Dispose();
                _serialPort = null;

                return 0;
            }
            catch (CliParserExit)
            {
                // --help
                return 0;

            }
            catch (Exception e)
            {
                // unknown options etc...
                Console.Error.WriteLine("Fatal Error: " + e.Message);
                return 1;
            }
            string ToHexString(float f)
            {
                var bytes = BitConverter.GetBytes(f);
                var i = BitConverter.ToInt32(bytes, 0);
                return "0x" + i.ToString("X8");
            }
            float FromHexString(string s)
            {
                var i = Convert.ToInt32(s, 16);
                var bytes = BitConverter.GetBytes(i);
                return BitConverter.ToSingle(bytes, 0);
            }
        }
    }
}