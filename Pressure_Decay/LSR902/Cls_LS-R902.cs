using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;
using static Cls_LS_R902;

public class Cls_LS_R902
{
    private SerialPort _serialPort;
    private string _portName;
    private int _baudRate;
    private Parity _parity;
    private int _dataBits;
    private StopBits _stopBits;
    private OutputFormat _outputFormat = OutputFormat.ID_Format;
    // Delegate và Event để gửi kết quả về MainForm
    public delegate void ResultReceivedHandler(object sender, LeakTestResultBase result);
    public event ResultReceivedHandler OnResultReceived;
    public delegate void DataReceivedHandler(string result);
    public event DataReceivedHandler OnDataReceived;
    public LeakTestResultBase result;
    public bool IsConnected { get;private set; }
    public enum OutputFormat
    {
        ID_Format,
        K_Format,
        M_Format,
        T_Format,
        Waveform_Format,
        XChart_Format
    }
    public bool _IsgetResultSuccess = false;
    public static string[] GetAvailableFormats()
    {
        return Enum.GetNames(typeof(OutputFormat));
    }
    public OutputFormat CurrentOutputFormat
    {
        get => _outputFormat;
        set
        {
            _outputFormat = value;
        }
    }
    public string PortName
    {
        get => _portName;
        set
        {
            if (_serialPort.IsOpen)
                throw new InvalidOperationException("Cannot change port name while the port is open.");
            _portName = value;
        }
    }
    public int BaudRate
    {
        get => _baudRate;
        set
        {
            if (_serialPort.IsOpen)
                throw new InvalidOperationException("Cannot change baud rate while the port is open.");
            _baudRate = value;
        }
    }
    public Parity Parity
    {
        get => _parity;
        set
        {
            if (_serialPort.IsOpen)
                throw new InvalidOperationException("Cannot change parity while the port is open.");
            _parity = value;
        }
    }
    public int DataBits
    {
        get => _dataBits;
        set
        {
            if (_serialPort.IsOpen)
                throw new InvalidOperationException("Cannot change data bits while the port is open.");
            _dataBits = value;
        }
    }
    public StopBits StopBits
    {
        get => _stopBits;
        set
        {
            if (_serialPort.IsOpen)
                throw new InvalidOperationException("Cannot change stop bits while the port is open.");
            _stopBits = value;
        }
    }

    public Cls_LS_R902()
    {
        _portName = ClsIO.ReadValue("LSR902", "SerialCom", "COM3", "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
        _baudRate = int.Parse(ClsIO.ReadValue("LSR902", "Baudrate", "9600", "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini"));
        _dataBits = int.Parse(ClsIO.ReadValue("LSR902", "DataBit", "8", "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini"));
        switch (ClsIO.ReadValue("LSR902", "Parity", "None", "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini"))
        {
            case "None":
                _parity = Parity.None;
                break;
            case "Odd":
                _parity = Parity.Odd;
                break;
            case "Even":
                _parity = Parity.Even;
                break;
        }
        switch (ClsIO.ReadValue("LSR902", "StopBit", "One", "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini"))
        {
            case "One":
                _stopBits = StopBits.One;
                break;
            case "OnePointFive":
                _stopBits = StopBits.OnePointFive;
                break;
            case "Two":
                _stopBits = StopBits.Two;
                break;
        }
        switch(ClsIO.ReadValue("LSR902", "M_Format", "None", "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini"))
        {
            case "M_Format":
                _outputFormat = OutputFormat.M_Format;
                break;
            default:
                _outputFormat = OutputFormat.M_Format;
                break;
        }
        _serialPort = new SerialPort(_portName, _baudRate, _parity, _dataBits, _stopBits);
    }
    public void Close()
    {
        try
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }
        catch {}
    }

    public void Open()
    {
        _serialPort = new SerialPort(_portName, _baudRate, _parity, _dataBits, _stopBits);
        try
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
                IsConnected = true;
            }
            _serialPort.DataReceived += SerialPort_DataReceived;
        }
        catch
        {
            IsConnected = false;
        }
    }
    public void SaveLSR902SerialPortInformation()
    {
        ClsIO.WriteValue("LSR902", "SerialCom", _portName.ToString(), "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
        ClsIO.WriteValue("LSR902", "Baudrate", _baudRate.ToString(), "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
        ClsIO.WriteValue("LSR902", "DataBit", _dataBits.ToString(), "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
        ClsIO.WriteValue("LSR902", "Parity", _parity.ToString(), "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
        ClsIO.WriteValue("LSR902", "StopBit", _stopBits.ToString(), "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
    }
    private StringBuilder receiveBuffer = new StringBuilder();
    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            string line = _serialPort.ReadExisting(); // Đọc dữ liệu từ cổng nối tiếp
            receiveBuffer.Append(line);
            while(receiveBuffer.ToString().Contains("\n")|| receiveBuffer.ToString().Contains("\r")|| receiveBuffer.ToString().Contains("\r\n"))
            {
                result = null;
                UpdateDataReceived(receiveBuffer.ToString());
                switch (_outputFormat)
                {
                    case OutputFormat.M_Format:
                        result = LeakTestResultM.Parse(receiveBuffer.ToString());
                        break;
                    case OutputFormat.K_Format:
                        result = LeakTestResultK.Parse(receiveBuffer.ToString());
                        break;
                    case OutputFormat.ID_Format:
                        result = LeakTestResultID.Parse(receiveBuffer.ToString());
                        break;
                }

                if (result != null)
                    RaiseResult(result); // Gửi dữ liệu ra ngoài
                _IsgetResultSuccess = true;
                receiveBuffer.Clear();
            }    
        }
        catch
        {
            receiveBuffer.Clear();
        }
    }
    public void RaiseResult(LeakTestResultBase result)
    {
        OnResultReceived?.Invoke(this, result);
    }
    public void UpdateDataReceived(string data)
    {
        OnDataReceived?.Invoke(data);
    }
}
