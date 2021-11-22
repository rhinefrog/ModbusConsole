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


[ApplicationInfo(Help = "Example: ModbusScan.exe -c com8 -s 0 -e 4 -r 0 -d")]
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

    [Option('s', "scanstartaddr", null, Help = "Start scanning address")]
    public int Startscan
    {
        get { return _startscan; }
        set
        {
            _startscan = value;
        }
    }
    private int _startscan;

    [Option('e', "scanendaddr", null, Help = "End scanning address")]
    public int Endscan
    {
        get { return _endscan; }
        set
        {
            _endscan = value;
        }
    }
    private int _endscan;

    [Option('r', "versuche", null, Help = "Retries / Versuche")]
    public int Versuche
    {
        get { return _versuche; }
        set
        {
            _versuche = value;
        }
    }
    private int _versuche;

    [Flag('d', "debug", Help = "If Debug required -d")]
    public bool DPdebug;
}

namespace ModbusScan
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                Options opt = CliParser.Parse<Options>(args);
                int datapoint = 0;
                int weiter = 0;

                int[] unitExist = new int[32]; // 248
                int[] busID = new int[32]; // 248
                int[] globID = new int[32]; // 248
                string[] controllerType = new string[32];
                string[] geraeteType = new string[32];
                float[] geraeteSW = new float[32];

                ModbusSerialMaster _modbusMaster;
                SerialPort _serialPort;
                _serialPort = new SerialPort(opt.Comport, 9600, Parity.None, 8, StopBits.One);
                _serialPort.Open();

                _modbusMaster = ModbusSerialMaster.CreateRtu(_serialPort);
                _modbusMaster.Transport.ReadTimeout = 1000;
                _modbusMaster.Transport.WriteTimeout = 1000;
                _modbusMaster.Transport.Retries = opt.Versuche; // 3 ;

                if(weiter == 0)     // unit exist
                {
                    Console.WriteLine("=================================");
                    for (int i = opt.Startscan; i < opt.Endscan + 1; i++) //248 bei modbus
                    {
                        try
                        {
                            // ReadHoldingRegisters = 0x03,        // FC03
                            ushort[] result = _modbusMaster.ReadHoldingRegisters(Convert.ToByte(i), (ushort)datapoint, 2);
                            var f = FromHexString(result[0].ToString("X4") + result[1].ToString("X4"));
                            Console.WriteLine("Glob-ID: " + i.ToString() + " Controller-Typ: " + ControllerType(f.ToString()) + "\n");

                            unitExist[i] = i;
                            controllerType[i] = ControllerType(f.ToString());
                        }
                        catch (Exception ex)
                        {
                            //The server return error code.You can get the function code and exception code.
                            if (ex.Message.Equals("The operation has timed out."))
                            {
                                Console.WriteLine("Glob-ID: " + i + " Controller-Typ: existiert nöd\n");
                                unitExist[i] = 999;
                                controllerType[i] = "unknown";
                            }
                            else
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        if (i == opt.Endscan) weiter = 1;
                    }
                }
                if (weiter == 1)    // unit type
                {
                    datapoint = 2;
                    Console.WriteLine("=================================");
                    for (int i = opt.Startscan; i < opt.Endscan+1; i++) //248
                    {
                        try
                        {
                            // ReadHoldingRegisters = 0x03,        // FC03
                            ushort[] result = _modbusMaster.ReadHoldingRegisters(Convert.ToByte(i), (ushort)datapoint, 2);
                            var f = FromHexString(result[0].ToString("X4") + result[1].ToString("X4"));
                            Console.WriteLine("Glob-ID: " + i.ToString() + " Unit-Typ: " + unitType(f.ToString()) + "\n");

                            geraeteType[i] = ControllerType(f.ToString());
                        }
                        catch (Exception ex)
                        {
                            //The server return error code.You can get the function code and exception code.
                            if (ex.Message.Equals("The operation has timed out."))
                            {
                                Console.WriteLine("Glob-ID: " + i + " Unit-Typ: existiert nöd\n");
                                geraeteType[i] = "unknown";
                            }
                            else
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        if (i == opt.Endscan) weiter = 2;
                    }
                }

                if (weiter == 2)    // software version
                {
                    datapoint = 6;
                    Console.WriteLine("=================================");
                    for (int i = opt.Startscan; i < opt.Endscan + 1; i++) //248
                    {
                        try
                        {
                            // ReadHoldingRegisters = 0x03,        // FC03
                            ushort[] result = _modbusMaster.ReadHoldingRegisters(Convert.ToByte(i), (ushort)datapoint, 2);
                            var f = FromHexString(result[0].ToString("X4") + result[1].ToString("X4"));
                            f = f / 100;
                            Console.WriteLine("Glob-ID: " + i.ToString() + " Unit-SW: " + f.ToString() + "\n");

                            geraeteSW[i] = f;
                        }
                        catch (Exception ex)
                        {
                            //The server return error code.You can get the function code and exception code.
                            if (ex.Message.Equals("The operation has timed out."))
                            {
                                Console.WriteLine("Glob-ID: " + i + " Unit-SW: existiert nöd\n");
                                geraeteSW[i] = 000;
                            }
                            else
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        if (i == opt.Endscan) weiter = 3;
                    }
                }
                if (weiter == 3)    // bus ID
                {
                    datapoint = 10;
                    Console.WriteLine("=================================");
                    for (int i = opt.Startscan; i < opt.Endscan + 1; i++) //248
                    {
                        try
                        {
                            // ReadHoldingRegisters = 0x03,        // FC03
                            ushort[] result = _modbusMaster.ReadHoldingRegisters(Convert.ToByte(i), (ushort)datapoint, 2);
                            var f = FromHexString(result[0].ToString("X4") + result[1].ToString("X4"));
                            f = f / 100;
                            Console.WriteLine("Glob-ID: " + i.ToString() + " Unit-SW: " + f.ToString() + "\n");

                            geraeteSW[i] = f;
                        }
                        catch (Exception ex)
                        {
                            //The server return error code.You can get the function code and exception code.
                            if (ex.Message.Equals("The operation has timed out."))
                            {
                                Console.WriteLine("Glob-ID: " + i + " Unit-SW: existiert nöd\n");
                                geraeteSW[i] = 000;
                            }
                            else
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        if (i == opt.Endscan) weiter = 4;
                    }
                }
                if (weiter == 4)    // glob ID
                {
                    datapoint = 12;
                    Console.WriteLine("=================================");
                    for (int i = opt.Startscan; i < opt.Endscan + 1; i++) //248
                    {
                        try
                        {
                            // ReadHoldingRegisters = 0x03,        // FC03
                            ushort[] result = _modbusMaster.ReadHoldingRegisters(Convert.ToByte(i), (ushort)datapoint, 2);
                            var f = FromHexString(result[0].ToString("X4") + result[1].ToString("X4"));
                            Console.WriteLine("Glob-ID: " + i.ToString() + " Glob-ID: " + f.ToString() + "\n");

                            globID[i] = (int)f;
                        }
                        catch (Exception ex)
                        {
                            //The server return error code.You can get the function code and exception code.
                            if (ex.Message.Equals("The operation has timed out."))
                            {
                                Console.WriteLine("Glob-ID: " + i + " Glob-ID: existiert nöd\n");
                                globID[i] = 0;
                            }
                            else
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        if (i == opt.Endscan) weiter = 5;
                    }
                }


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
            string ControllerType(string num)
        {
            string controllerTyp ="";
            int nummer = Convert.ToInt32(num);

            switch (nummer)
            {
                case 0:
                    controllerTyp = "unknown";
                    break;
                case 1:
                    controllerTyp = "C4000";
                    break;
                case 2:
                    controllerTyp = "C1001";
                    break;
                case 3:
                    controllerTyp = "C1002";
                    break;
                case 4:
                    controllerTyp = "C5000";
                    break;
                case 5:
                    controllerTyp = "C6000";
                    break;
                case 6:
                    controllerTyp = "C1010";
                    break;
                case 7:
                    controllerTyp = "C7000IOC";
                    break;
                case 8:
                    controllerTyp = "C7000AT";
                    break;
                case 9:
                    controllerTyp = "C7000PT";
                    break;
                case 10:
                    controllerTyp = "C5MSC";
                    break;
                case 11:
                    controllerTyp = "C7000PT2";
                    break;
                case 12:
                    controllerTyp = "C2020";
                    break;
                case 13:
                    controllerTyp = "C100";
                    break;
                case 14:
                    controllerTyp = "C102";
                    break;
                case 15:
                    controllerTyp = "C103";
                    break;
                case 16:
                    controllerTyp = "C7000TP";
                    break;
            }
            return controllerTyp;
            }
 
            string unitType(string num)
            {
                string unitTyp = "";
                int nummer = Convert.ToInt32(num);

                switch (nummer)
                {
                    case 0:
                        unitTyp = "MC";
                        break;
                    case 1:
                        unitTyp = "DX";
                        break;
                    case 2:
                        unitTyp = "CW";
                        break;
                    case 3:
                        unitTyp = "CH";
                        break;
                    case 4:
                        unitTyp = "ECO-COOL";
                        break;
                    case 5:
                        unitTyp = "MSC";
                        break;
                    case 6:
                        unitTyp = "BE1";
                        break;
                    case 7:
                        unitTyp = "GE2";
                        break;
                    case 8:
                        unitTyp = "Dualfluid";
                        break;
                    case 9:
                        unitTyp = "CW2";
                        break;
                    case 10:
                        unitTyp = "CMD";
                        break;
                    case 11:
                        unitTyp = "CHP";
                        break;
                    case 12:
                        unitTyp = "FAU";
                        break;
                    case 13:
                        unitTyp = "CPP";
                        break;
                    case 14:
                        unitTyp = "Predator";
                        break;
                    case 15:
                        unitTyp = "Prodogy";
                        break;
                    case 16:
                        unitTyp = "ENS";
                        break;
                    case 17:
                        unitTyp = "CyberRow A";
                        break;
                    case 18:
                        unitTyp = "CyberRow CW";
                        break;
                    case 19:
                        unitTyp = "CyberRow G";
                        break;
                    case 20:
                        unitTyp = "EC-Tower";
                        break;
                    case 21:
                        unitTyp = "Explorer";
                        break;
                    case 255:
                        unitTyp = "unknown";
                        break;
                }
                return unitTyp;
            }
        }
    }
}