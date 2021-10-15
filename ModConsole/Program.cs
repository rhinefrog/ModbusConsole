using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
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


[ApplicationInfo(Help = "This program does something useful.")]
public class Options
{
	//	[Flag('s', "silent", Help = "Produce no output.")]
	//	public bool Silent;

	//	[Option('n', "name", "NAME", Help = "Name of user.")]
	//	public string Name
	//	{
	//		get { return _name; }
	//		set
	//		{
	//			if (String.IsNullOrWhiteSpace(value))
	//				throw new InvalidOptionValueException("Name must not be blank");
	//			_name = value;
	//		}
	//	}
	//	private string _name;

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
			if (String.IsNullOrWhiteSpace(value))
				throw new InvalidOptionValueException("Comport must not be blank");
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
	public string Saddress
	{
		get { return _saddress; }
		set
		{
			if (String.IsNullOrWhiteSpace(value))
				throw new InvalidOptionValueException("Comport must not be blank");
			_saddress = value;
		}
	}
	private string _saddress;

	[Option('q', "qaddresses", "QUANTYADDRESSES", Help = "Quantity of addresses")]
	public string Qtyaddresses
	{
		get { return _qtyaddresses; }
		set
		{
			if (String.IsNullOrWhiteSpace(value))
				throw new InvalidOptionValueException("Comport must not be blank");
			_qtyaddresses = value;
		}
	}
	private string _qtyaddresses;

	[Flag('i', "ieee754", Help = "If IEEE754 required -i")]
	public bool Ieee754;

    [Option('w', "write", "WRITE", Help = "Value write")]
    public string DPValue
    {
        get { return _dpvalue; }
        set
        {
            if (String.IsNullOrWhiteSpace(value))
                throw new InvalidOptionValueException("Comport must not be blank");
            _dpvalue = value;
        }
    }
    private string _dpvalue;
}

namespace ModConsole
{
	class Program
	{
		static int Main(string[] args)
		{
            try
			{
                SerialPort _serialPort;
                IModbusSerialMaster master;
                byte slaveAddress = 1;
                _serialPort = new SerialPort();

				Options opt = CliParser.Parse<Options>(args);
				Console.WriteLine("Device-ID: "+opt.Address);
				Console.WriteLine("Function: "+opt.Mfunction);
				Console.WriteLine("Start DP: "+opt.Saddress);
				Console.WriteLine("Qty Of DP: "+opt.Qtyaddresses);
                Console.WriteLine("DP-Value: " + opt.DPValue);
                Console.WriteLine("IEEE754: "+opt.Ieee754);
				//				Console.WriteLine(opt.OutputFile);
				Console.WriteLine("Comport: "+opt.Comport);

                // Allow the user to set the appropriate properties.
                _serialPort.PortName = opt.Comport;
				_serialPort.BaudRate = 9600;
				_serialPort.Parity = Parity.None;
				_serialPort.DataBits = 8;
                _serialPort.StopBits = StopBits.One;
				_serialPort.Handshake = Handshake.None;
				// Set the read/write timeouts
				_serialPort.ReadTimeout = 500;
				_serialPort.WriteTimeout = 500;
//				_serialPort.Open();

                if (!_serialPort.IsOpen) _serialPort.Open();
                master = ModbusSerialMaster.CreateRtu(_serialPort);
                master.Transport.ReadTimeout = 1000;
                master.Transport.WriteTimeout = 1000;

                slaveAddress = Convert.ToByte(opt.Address);

                if (opt.Mfunction == "1")
                {
                    bool[] results = master.ReadCoils(slaveAddress, Convert.ToUInt16(opt.Saddress), Convert.ToUInt16(opt.Qtyaddresses));
                    Console.WriteLine(results);
                }

                if (opt.Mfunction == "2") {  
                    bool[] results = master.ReadInputs(slaveAddress, Convert.ToUInt16(opt.Saddress), Convert.ToUInt16(opt.Qtyaddresses));
                    Console.WriteLine(results);
                }

                if(opt.Mfunction == "3")
                {
                    ushort[] results = master.ReadHoldingRegisters(slaveAddress, Convert.ToUInt16(opt.Saddress), Convert.ToUInt16(opt.Qtyaddresses));
                    decimal temp = results[0];
                    temp /= 10;
                    Console.WriteLine(results);
                }

                if (opt.Mfunction == "4")
                {
                    ushort[] results = master.ReadInputRegisters(slaveAddress, Convert.ToUInt16(opt.Saddress), Convert.ToUInt16(opt.Qtyaddresses));
                    decimal temp = results[0];
                    temp /= 10;
                    Console.WriteLine(results);
                }

                if (opt.Mfunction == "5")
                {
                    master.WriteSingleCoil(slaveAddress, Convert.ToUInt16(opt.Saddress), Convert.ToBoolean(opt.DPValue));
                }

                if (opt.Mfunction == "6")
                {
                    master.WriteSingleRegister(slaveAddress, Convert.ToUInt16(opt.Saddress), Convert.ToUInt16(opt.DPValue));
                }

                if (opt.Mfunction == "16")
                {
					var tempo = ToHexString(float.Parse(opt.DPValue));
					ushort[] registers = new ushort[] { 1, 2 };
                    registers[0] = Convert.ToUInt16(tempo.Substring(2, 4), 16);
                    registers[1] = Convert.ToUInt16(tempo.Substring(6, 4), 16);
                    Console.WriteLine(registers);
                    master.WriteMultipleRegisters(slaveAddress, Convert.ToUInt16(opt.Saddress), registers);
                }

                _serialPort.Close();
                _serialPort.Dispose();
                master.Dispose();

                // IEEE754 it works
 //               var hexString = ToHexString(-10.5F);
 //               var f = FromHexString(hexString);
 //               Console.WriteLine(hexString.ToString());
 //               Console.WriteLine(f.ToString());

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
/*
// Use this code inside a project created with the Visual C# > Windows Desktop > Console Application template.
// Replace the code in Program.cs with this code.

using System;
using System.IO.Ports;
using System.Threading;

public class PortChat
{
    static bool _continue;
    static SerialPort _serialPort;

    public static void Main()
    {
        string name;
        string message;
        StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
        Thread readThread = new Thread(Read);

        // Create a new SerialPort object with default settings.
        _serialPort = new SerialPort();

        // Allow the user to set the appropriate properties.
        _serialPort.PortName = SetPortName(_serialPort.PortName);
        _serialPort.BaudRate = SetPortBaudRate(_serialPort.BaudRate);
        _serialPort.Parity = SetPortParity(_serialPort.Parity);
        _serialPort.DataBits = SetPortDataBits(_serialPort.DataBits);
        _serialPort.StopBits = SetPortStopBits(_serialPort.StopBits);
        _serialPort.Handshake = SetPortHandshake(_serialPort.Handshake);

        // Set the read/write timeouts
        _serialPort.ReadTimeout = 500;
        _serialPort.WriteTimeout = 500;

        _serialPort.Open();
        _continue = true;
        readThread.Start();

        Console.Write("Name: ");
        name = Console.ReadLine();

        Console.WriteLine("Type QUIT to exit");

        while (_continue)
        {
            message = Console.ReadLine();

            if (stringComparer.Equals("quit", message))
            {
                _continue = false;
            }
            else
            {
                _serialPort.WriteLine(
                    String.Format("<{0}>: {1}", name, message));
            }
        }

        readThread.Join();
        _serialPort.Close();
    }

    public static void Read()
    {
        while (_continue)
        {
            try
            {
                string message = _serialPort.ReadLine();
                Console.WriteLine(message);
            }
            catch (TimeoutException) { }
        }
    }

    // Display Port values and prompt user to enter a port.
    public static string SetPortName(string defaultPortName)
    {
        string portName;

        Console.WriteLine("Available Ports:");
        foreach (string s in SerialPort.GetPortNames())
        {
            Console.WriteLine("   {0}", s);
        }

        Console.Write("Enter COM port value (Default: {0}): ", defaultPortName);
        portName = Console.ReadLine();

        if (portName == "" || !(portName.ToLower()).StartsWith("com"))
        {
            portName = defaultPortName;
        }
        return portName;
    }
    // Display BaudRate values and prompt user to enter a value.
    public static int SetPortBaudRate(int defaultPortBaudRate)
    {
        string baudRate;

        Console.Write("Baud Rate(default:{0}): ", defaultPortBaudRate);
        baudRate = Console.ReadLine();

        if (baudRate == "")
        {
            baudRate = defaultPortBaudRate.ToString();
        }

        return int.Parse(baudRate);
    }

    // Display PortParity values and prompt user to enter a value.
    public static Parity SetPortParity(Parity defaultPortParity)
    {
        string parity;

        Console.WriteLine("Available Parity options:");
        foreach (string s in Enum.GetNames(typeof(Parity)))
        {
            Console.WriteLine("   {0}", s);
        }

        Console.Write("Enter Parity value (Default: {0}):", defaultPortParity.ToString(), true);
        parity = Console.ReadLine();

        if (parity == "")
        {
            parity = defaultPortParity.ToString();
        }

        return (Parity)Enum.Parse(typeof(Parity), parity, true);
    }
    // Display DataBits values and prompt user to enter a value.
    public static int SetPortDataBits(int defaultPortDataBits)
    {
        string dataBits;

        Console.Write("Enter DataBits value (Default: {0}): ", defaultPortDataBits);
        dataBits = Console.ReadLine();

        if (dataBits == "")
        {
            dataBits = defaultPortDataBits.ToString();
        }

        return int.Parse(dataBits.ToUpperInvariant());
    }

    // Display StopBits values and prompt user to enter a value.
    public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
    {
        string stopBits;

        Console.WriteLine("Available StopBits options:");
        foreach (string s in Enum.GetNames(typeof(StopBits)))
        {
            Console.WriteLine("   {0}", s);
        }

        Console.Write("Enter StopBits value (None is not supported and \n" +
         "raises an ArgumentOutOfRangeException. \n (Default: {0}):", defaultPortStopBits.ToString());
        stopBits = Console.ReadLine();

        if (stopBits == "" )
        {
            stopBits = defaultPortStopBits.ToString();
        }

        return (StopBits)Enum.Parse(typeof(StopBits), stopBits, true);
    }
    public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
    {
        string handshake;

        Console.WriteLine("Available Handshake options:");
        foreach (string s in Enum.GetNames(typeof(Handshake)))
        {
            Console.WriteLine("   {0}", s);
        }

        Console.Write("Enter Handshake value (Default: {0}):", defaultPortHandshake.ToString());
        handshake = Console.ReadLine();

        if (handshake == "")
        {
            handshake = defaultPortHandshake.ToString();
        }

        return (Handshake)Enum.Parse(typeof(Handshake), handshake, true);
    }
}
*/