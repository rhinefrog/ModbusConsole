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
using FluentModbus;
using OptionAttribute = coptions.OptionAttribute;


[ApplicationInfo(Help = "Example: ModConsole.exe -c com8 -a 1 -f 3 -s 2340 -q 4 -i ")]
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

    [Option('w', "write", "WRITE", Help = "Value write")]
    public string DPValue
    {
        get { return _dpvalue; }
        set
        {
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
				Options opt = CliParser.Parse<Options>(args);
                var client = new ModbusRtuClient()
                {
                    BaudRate = 9600,
                    Handshake = Handshake.None,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };
                var clientPort = opt.Comport;
                    
                Span<byte> data;

                var sleepTime = TimeSpan.FromMilliseconds(100);
                if (!client.IsConnected)                client.Connect(clientPort);
                try
                {

                    // ReadHoldingRegisters = 0x03,        // FC03
                    //                        data = client.ReadHoldingRegisters<byte>(unitIdentifier, startingAddress, opt.Qtyaddresses);
                    //                        logger.LogInformation("FC03 - ReadHoldingRegisters: Done");
                    //                        Thread.Sleep(sleepTime);

                    // WriteMultipleRegisters = 0x10,      // FC16
                    //                        client.WriteMultipleRegisters(unitIdentifier, startingAddress, new byte[] { 10, 00, 20, 00, 30, 00, 255, 00, 255, 01 });
                    //                        logger.LogInformation("FC16 - WriteMultipleRegisters: Done");
                    //                        Thread.Sleep(sleepTime);

                    // ReadCoils = 0x01,                   // FC01
                    //                        data = client.ReadCoils(unitIdentifier, startingAddress, opt.Qtyaddresses);
                    //                        logger.LogInformation("FC01 - ReadCoils: Done");
                    //                        Thread.Sleep(sleepTime);

                    // ReadDiscreteInputs = 0x02,          // FC02
                    //                        data = client.ReadDiscreteInputs(unitIdentifier, startingAddress, 10);
                    //                        logger.LogInformation("FC02 - ReadDiscreteInputs: Done");
                    //                        Thread.Sleep(sleepTime);

                    // ReadInputRegisters = 0x04,          // FC04
                    //                        data = client.ReadInputRegisters<byte>(unitIdentifier, startingAddress, 10);
                    //                        logger.LogInformation("FC04 - ReadInputRegisters: Done");
                    //                        Thread.Sleep(sleepTime);

                    // WriteSingleCoil = 0x05,             // FC05
                    //                        client.WriteSingleCoil(unitIdentifier, registerAddress, true);
                    //                        logger.LogInformation("FC05 - WriteSingleCoil: Done");
                    //                        Thread.Sleep(sleepTime);

                    // WriteSingleRegister = 0x06,         // FC06
                    //                        client.WriteSingleRegister(unitIdentifier, registerAddress, 127);
                    //                        logger.LogInformation("FC06 - WriteSingleRegister: Done");
                    //                    }
                    if (opt.Mfunction == "1")
                    {
                        // ReadCoils = 0x01,                   // FC01
                        data = client.ReadCoils(opt.Address, opt.Saddress, opt.Qtyaddresses);
                        var newData = data.ToArray();
                        string hilf = FromHexString(ByteArrayToString(newData)).ToString();
                        if (hilf == "0")
                        {
                            Console.WriteLine("false");
                        }
                        else
                        {
                            Console.WriteLine("true");
                        }
                        Console.WriteLine("FC01 - ReadCoils: Done");
                        //                        Thread.Sleep(sleepTime);
                    }

                    if (opt.Mfunction == "2") {  
                        // ReadDiscreteInputs = 0x02,          // FC02
                        data = client.ReadDiscreteInputs(opt.Address, opt.Saddress, opt.Qtyaddresses);
                        var newData = data.ToArray();
                        string hilf = FromHexString(ByteArrayToString(newData)).ToString();
                        if (hilf == "0")
                        {
                            Console.WriteLine("false");
                        }
                        else
                        {
                            Console.WriteLine("true");
                        }
                        Console.WriteLine("FC01 - ReadCoils: Done");
                        //                        Thread.Sleep(sleepTime);
                    }

                    if (opt.Mfunction == "3")
                {
                        // ReadHoldingRegisters = 0x03,        // FC03
                        data = client.ReadHoldingRegisters<byte>(opt.Address, opt.Saddress, opt.Qtyaddresses);
                        var newData = data.ToArray();
                        Console.WriteLine(ByteArrayToString(newData));
                        Console.WriteLine(FromHexString(ByteArrayToString(newData)));
                        Console.WriteLine("FC03 - ReadHoldingRegisters: Done");
                }

                if (opt.Mfunction == "4")
                {
                        // ReadInputRegisters = 0x04,          // FC04
                        data = client.ReadInputRegisters<byte>(opt.Address, opt.Saddress, opt.Qtyaddresses);
                        //                        Thread.Sleep(sleepTime);
                        var newData = data.ToArray();
                        if (opt.Ieee754 == true)
                        {
                            Console.WriteLine(FromHexString(ByteArrayToString(newData)));
                        }else
                        {
                            Console.WriteLine(ByteArrayToString(newData));
                        }
                        Console.WriteLine("FC03 - ReadHoldingRegisters: Done");
                }

                if (opt.Mfunction == "5")
                {
                     // WriteSingleCoil = 0x05,             // FC0
                     client.WriteSingleCoil(opt.Address, opt.Saddress, Convert.ToBoolean(opt.DPValue));
                        //                        logger.LogInformation("FC05 - WriteSingleCoil: Done");
                        //                        Thread.Sleep(sleepTime);
                        Console.WriteLine("FC05 - WriteSingleCoil: Done");
                    }

                    if (opt.Mfunction == "16")
                    {
                        // WriteMultipleRegisters = 0x10,      // FC16
                        float fhilf = Convert.ToInt16(opt.DPValue) / 10;
                        var tempo = ToHexString(fhilf);
                        Console.WriteLine(tempo);
                        var registers = tempo.ToArray();
                        //byte[] registers = new byte[] { 0x41, 0xc0, 0x00, 0x00 };
                     /*   registers[0] = Convert.ToByte(tempo.Substring(2, 2));
                        registers[1] = Convert.ToByte(tempo.Substring(4, 2));
                        registers[2] = Convert.ToByte(tempo.Substring(6, 2));
                        registers[3] = Convert.ToByte(tempo.Substring(8, 2));
                       */ client.WriteMultipleRegisters(opt.Address, opt.Saddress, registers); 
                        //                        Thread.Sleep(sleepTime);
                        //					var tempo = ToHexString(float.Parse(opt.DPValue));
                        //					ushort[] registers = new ushort[] { 1, 2 };
                        //                    registers[0] = Convert.ToUInt16(tempo.Substring(2, 4), 16);
                        //                    registers[1] = Convert.ToUInt16(tempo.Substring(6, 4), 16);
                        //                    Console.WriteLine(registers);
                        //                    master.WriteMultipleRegisters(slaveAddress, Convert.ToUInt16(opt.Saddress), registers);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                client.Close();

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
            int GetBitRange(int data, int offset, int count)
            {
                return data << offset >> (32 - count);
            }
            string ConvertLinearToString(ushort data)
            {
                var n = GetBitRange(data, 16, 5);
                var y = GetBitRange(data, 21, 11);
                var value = y * Math.Pow(2, n);
                return value.ToString();
            }
            float FromHexString(string s)
            {
                var i = Convert.ToInt32(s, 16);
                var bytes = BitConverter.GetBytes(i);
                return BitConverter.ToSingle(bytes, 0);
            }
            string ByteArrayToString(byte[] ba)
            {
                return BitConverter.ToString(ba).Replace("-", "");
            }
            byte[] StringToByteArray(String hex)
            {
                int NumberChars = hex.Length;
                byte[] bytes = new byte[NumberChars / 2];
                for (int i = 0; i < NumberChars; i += 2)
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                return bytes;
            }
            string ConvertHex(string hexString)
            {
                try
                {
                    string ascii = string.Empty;

                    for (int i = 0; i < hexString.Length; i += 2)
                    {
                        string hs = string.Empty;

                        hs = hexString.Substring(i, 2);
                        ulong decval = Convert.ToUInt64(hs, 16);
                        long deccc = Convert.ToInt64(hs, 16);
                        char character = Convert.ToChar(deccc);
                        ascii += character;

                    }

                    return ascii;
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }

                return string.Empty;
            }
        }
    }
/*    public static class EncodingExtensions
    {
        public static string GetString(this Encoding encoding, Span<byte> source)
        {
            //naive way using ToArray, but possible to improve when needed
            return encoding.GetString(source.ToArray());
        }
    }
*/
}
