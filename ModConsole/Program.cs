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


[ApplicationInfo(Help = "Example: ModScanner.exe -c com8")]
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
                _serialPort = new SerialPort();

				Options opt = CliParser.Parse<Options>(args);
//				Console.WriteLine("Device-ID: "+opt.Address);
//				Console.WriteLine("Function: "+opt.Mfunction);
//				Console.WriteLine("Start DP: "+opt.Saddress);
//				Console.WriteLine("Qty Of DP: "+opt.Qtyaddresses);
//                Console.WriteLine("DP-Value: " + opt.DPValue);
//                Console.WriteLine("IEEE754: "+opt.Ieee754);
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

                //                bool[] scanUnits;
                //                for(int i = 1; i < 32; i++)
                //                {
                //                    scanUnits = master.ReadCoils(slaveAddress, Convert.ToUInt16(i), Convert.ToUInt16(1013));
                //                    Console.WriteLine(scanUnits);
                //                }
                int next = 0;
                string[] unitTyp = new string[33];
                string[] controllerTyp = new string[33];
                string[] controllerswversion = new string[33];
                ushort[] results;
                for(int i = 1; i < 32; i++)
                {// controller typ
                    try
                    {
                        results = master.ReadHoldingRegisters(Convert.ToByte(i), Convert.ToUInt16(0), Convert.ToUInt16(1));
                        var wert = FromHexString(Convert.ToString((int)results[0])); 
                        Console.WriteLine("Wert: " + wert);
                        if (wert == 0) controllerTyp[i]  = "unknown";
                        if (wert == 1) controllerTyp[i]  = "C4000";
                        if (wert == 2) controllerTyp[i]  = "C1001";
                        if (wert == 3) controllerTyp[i]  = "C1002";
                        if (wert == 4) controllerTyp[i]  = "C5000";
                        if (wert == 5) controllerTyp[i]  = "C6000";
                        if (wert == 6) controllerTyp[i]  = "C1010";
                        if (wert == 7) controllerTyp[i]  = "C7000IOC";
                        if (wert == 8) controllerTyp[i]  = "C7000AT";
                        if (wert == 9) controllerTyp[i]  = "C7000PT";
                        if (wert == 10) controllerTyp[i] = "C5MSC";
                        if (wert == 11) controllerTyp[i] = "C7000PT2";
                        if (wert == 12) controllerTyp[i] = "C2020";
                        if (wert == 13) controllerTyp[i] = "C100";
                        if (wert == 14) controllerTyp[i] = "C102"; 
                        if (wert == 15) controllerTyp[i] = "C103"; 
                        if (wert == 16) controllerTyp[i] = "C7000TP";
                        Console.WriteLine("controllerTyp :" + controllerTyp);
                        Console.WriteLine("results :" + results);
                        Thread.Sleep(50);
                    }
                    catch (Exception e)
                    {
                        // unknown options etc...
                        Console.Error.WriteLine("Fatal Error: " + e.Message);
                        return 1;
                    }
                    if (i == 32) next = 1;
                }
                // unit typ
                if(next == 1)
                {
                    for (int i = 1; i < 32; i++)
                    {// controller typ
                        try
                        {
                            results = master.ReadHoldingRegisters(Convert.ToByte(i), Convert.ToUInt16(2), Convert.ToUInt16(1));
                            var wert = FromHexString(Convert.ToString((int)results[0])); 
                            Console.WriteLine("Wert: " + wert);
                            if (wert == 0)  unitTyp[i] = "MC";
                            if (wert == 1)  unitTyp[i] = "DX";
                            if (wert == 2)  unitTyp[i] = "CW";
                            if (wert == 3)  unitTyp[i] = "CH";
                            if (wert == 4)  unitTyp[i] = "ECO-COOL";
                            if (wert == 5)  unitTyp[i] = "MSC";
                            if (wert == 6)  unitTyp[i] = "GE1";
                            if (wert == 7)  unitTyp[i] = "GE2";
                            if (wert == 8)  unitTyp[i] = "Dualfluid";
                            if (wert == 9)  unitTyp[i] = "CW2";
                            if (wert == 10) unitTyp[i] = "CMD";
                            if (wert == 11) unitTyp[i] = "CHP";
                            if (wert == 12) unitTyp[i] = "FAU";
                            if (wert == 13) unitTyp[i] = "CPP";
                            if (wert == 14) unitTyp[i] = "Predator";
                            if (wert == 15) unitTyp[i] = "Prodigy";
                            if (wert == 16) unitTyp[i] = "ENS";
                            if (wert == 17) unitTyp[i] = "CyberRow A";
                            if (wert == 18) unitTyp[i] = "CyberRow CW";
                            if (wert == 19) unitTyp[i] = "CyberRow G";
                            if (wert == 20) unitTyp[i] = "EC-Tower";
                            if (wert == 21) unitTyp[i] = "Explorer";
                            if (wert == 255) unitTyp[i] = "unknown";

                            Console.WriteLine("Unittyp :" + unitTyp);
                            Console.WriteLine("results :" + results);
                            Thread.Sleep(50);
                        }
                            catch (Exception e)
                        {
                            // unknown options etc...
                            Console.Error.WriteLine("Fatal Error: " + e.Message);
                            return 1;
                        }
                        if (i == 32) next = 2;
                    }
                }
                // sw version
                if (next == 2)
                {
                    for (int i = 1; i < 32; i++)
                    {// controller typ
                        try
                        {
                            results = master.ReadHoldingRegisters(Convert.ToByte(i), Convert.ToUInt16(6), Convert.ToUInt16(1));
                            var wert = FromHexString(Convert.ToString((int)results[0])); 
                            Console.WriteLine("Wert: " + wert);
                            controllerswversion[i] = wert.ToString();
                            Console.WriteLine("SWVersion :" + controllerswversion);
                            Console.WriteLine("results :" + results);
                            Thread.Sleep(50);
                        }
                        catch (Exception e)
                        {
                            // unknown options etc...
                            Console.Error.WriteLine("Fatal Error: " + e.Message);
                            return 1;
                        }
                        if (i == 32) next = 3;
                    }
                }
                // bus id
                string[] busid = new string[33];
                if (next == 3)
                {
                    for (int i = 1; i < 32; i++)
                    {// busid
                        try
                        {
                            results = master.ReadHoldingRegisters(Convert.ToByte(i), Convert.ToUInt16(10), Convert.ToUInt16(1));
                            var wert = FromHexString(Convert.ToString((int)results[0])); 
                            Console.WriteLine("Wert: " + wert);
                            busid[i] = wert.ToString();
                            Console.WriteLine("BusID :" + busid);
                            Console.WriteLine("results :" + results);
                            Thread.Sleep(50);
                        }
                        catch (Exception e)
                        {
                            // unknown options etc...
                            Console.Error.WriteLine("Fatal Error: " + e.Message);
                            return 1;
                        }
                        if (i == 32) next = 4;
                    }
                }
                // glob id
                string[] globid = new string[33];
                if (next == 4)
                {
                    for (int i = 1; i < 32; i++)
                    {// busid
                        try
                        {
                            results = master.ReadHoldingRegisters(Convert.ToByte(i), Convert.ToUInt16(12), Convert.ToUInt16(1));
                            var wert = FromHexString(Convert.ToString((int)results[0])); 
                            Console.WriteLine("Wert: " + wert);
                            globid[i] = wert.ToString();
                            Console.WriteLine("GlobID :" + globid);
                            Console.WriteLine("results :" + results);
                            Thread.Sleep(50);
                        }
                        catch (Exception e)
                        {
                            // unknown options etc...
                            Console.Error.WriteLine("Fatal Error: " + e.Message);
                            return 1;
                        }
                        if (i == 32) next = 5;
                    }
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