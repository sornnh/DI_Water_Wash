using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

public class Cls_ASPcontrol
{
    public delegate void DelShow_addLog_Request(string sender);
    public delegate void DelShow_UpdateRelayStatus_Request(Cls_ASPcontrol sender);
    public delegate void DelShow_UpdateADC_Request(Cls_ASPcontrol sender);
    public delegate void DelShow_UpdateHumnidityTemp_Request(Cls_ASPcontrol sender);
    public delegate void DelShow_UpdateSerialSN1(string sender);
    public delegate void DelShow_UpdateSerialSN2(string sender);
    public delegate void DelShow_UpdateSerialSN3(string sender);
    public delegate void DelShow_UpdateSerialSN4(string sender);
    public delegate void DelShow_UpdateStatusInput(Cls_ASPcontrol sender);
    private static readonly ILog log = LogManager.GetLogger(typeof(Cls_ASPcontrol));

    private bool Asyncrelay = false;

    private bool _isConnected = false;

    private double[] _ADVs = new double[4];

    private double[] _ADIs = new double[4];

    private double[] _Humiditys = new double[4];

    private double[] _Temps = new double[4];

    private List<byte> _buffer = new List<byte>();

    private TaskCompletionSource<string[]> _tcs = new TaskCompletionSource<string[]>();

    public string SerialCom { get; set; }

    public int Baudrate { get; set; }

    public int DataBit { get; set; }

    public Parity Parity { get; set; }

    public StopBits StopBit { get; set; }

    public SerialPort SerialPort { get; private set; }

    public string DataReceive { get; private set; }

    public string AllDataReceive { get; private set; }

    public bool[] relayStates { get; private set; }
    public bool[] inputStates { get; private set; }
    public double[] ADVs => _ADVs;

    public double[] ADIs => _ADIs;

    public double[] Humiditys => _Humiditys;
    public double[] Temps => _Temps;
    private int _expectedLineCount = 1; // mặc định là 1 dòng
    private List<string> _receivedLines = new List<string>();

    public bool isConnected
    {
        get
        {
            return _isConnected;
        }
        set
        {
            _isConnected = value;
        }
    }

    public bool _isWaitingResponse { get; private set; }

    public event DelShow_addLog_Request OnRequestAddLog;

    public event DelShow_UpdateRelayStatus_Request OnRequestUpdateRelayStatus;

    public event DelShow_UpdateADC_Request OnRequestUpdateADC;

    public event DelShow_UpdateHumnidityTemp_Request OnRequestUpdateHumnidityTemp;

    public event DelShow_UpdateSerialSN1 OnRequestUpdateSN_1;
    public event DelShow_UpdateSerialSN2 OnRequestUpdateSN_2;
    public event DelShow_UpdateSerialSN3 OnRequestUpdateSN_3;
    public event DelShow_UpdateSerialSN4 OnRequestUpdateSN_4;
    public event DelShow_UpdateStatusInput OnRequestUpdateStatusInput;
    public string SerialNumber1 { get; private set; }
    public string SerialNumber2 { get; private set; }
    public string SerialNumber3 { get; private set; }
    public string SerialNumber4 { get; private set; }
    private readonly SemaphoreSlim _cmdLock = new SemaphoreSlim(1, 1); //FIFO Queue
    public  Cls_ASPcontrol()
    {
        GetASPSerialPortInformation();
        ConnectASPSerial();
        DataReceive = "";
        AllDataReceive = "";
        relayStates = new bool[30];
        inputStates = new bool[20];
        for (int i = 0; i < relayStates.Length; i++)
        {
            relayStates[i] = false;
        }
        for (int i = 0; i < inputStates.Length; i++)
        {
            inputStates[i] = false;
        }
        GetAllInOut();
    }
    public async void GetAllInOut()
    {
        await GetAllRelay();
        await GetAllInput();
    }
    public void StartRelayReading()
    {
        Asyncrelay = true;
    }

    public async Task GetAllRelay()
    {
        try
        {
            string bitString = (await SendCommandAndWaitLineResponseAsync("RR000\n"))[0].Substring(5, 30);
            bool[] result = bitString.Select((char c) => c == '1').ToArray();
            if (result.Length != 30)
            {
                log.Error((object)("bit length incorrect: " + bitString.Length));
            }
            relayStates = result;
            UpdaterelayStatus();
        }
        catch (Exception ex)
        {
            Exception ex2 = ex;
            log.Error((object)"Error in GetAllRelay", ex2);
        }
    }
    public async Task GetAllInput()
    {
        try
        {
            string GetResult = (await SendCommandAndWaitLineResponseAsync("RI000\n"))[0];
            string bitString = GetResult.Substring(5, 20);
            bool[] result = bitString.Select((char c) => c == '1').ToArray();
            if (result.Length != 20)
            {
                log.Error((object)("bit length incorrect: " + bitString.Length));
            }
            inputStates = result;
            UpdateStatusInput();
        }
        catch (Exception ex)
        {
            Exception ex2 = ex;
            log.Error((object)"Error in GetAllInput", ex2);
        }
    }
    public async Task SetRelayONOFFAsync(int relay, int onOff)
    {
        if (relayStates[relay - 1] == (onOff == 1))
        {
            return;
        }
        string cmd = ((onOff == 1) ? $"WR{relay:D3}1\n" : $"WR{relay:D3}0\n");
        try
        {
            await SendCommandAndWaitLineResponseAsync(cmd);
        }
        catch (Exception ex)
        {
            log.Error((object)("Lỗi gửi lệnh: " + ex.Message));
        }
    }

    public async Task<double> GetADVAsync(int iFlowChannel)
    {
        string cmd = $"RADV{iFlowChannel}\n";
        try
        {
            string response = (await SendCommandAndWaitLineResponseAsync(cmd))[0];
            string subresponse = "";
            if (response.Contains(cmd.Trim()))
            {
                subresponse = response.Substring(cmd.Length - 1);
            }
            _ADVs[iFlowChannel - 1] = double.Parse(subresponse) * 1000.0;
        }
        catch (Exception ex)
        {
            Exception ex2 = ex;
            log.Error((object)("Lỗi gửi lệnh: " + ex2.Message));
        }
        UpdateADC();
        return _ADVs[iFlowChannel - 1];
    }

    public async Task<double> GetADIAsync(int iFlowChannel)
    {
        string cmd = $"RADA{iFlowChannel}\n";
        try
        {
            string response = (await SendCommandAndWaitLineResponseAsync(cmd))[0];
            string subresponse = "";
            if (response.Contains(cmd.Trim()))
            {
                subresponse = response.Substring(cmd.Length - 1);
            }
            _ADIs[iFlowChannel - 1] = double.Parse(subresponse);
        }
        catch (Exception ex)
        {
            Exception ex2 = ex;
            log.Error((object)("Lỗi gửi lệnh: " + ex2.Message));
        }
        UpdateADC();
        return _ADIs[iFlowChannel - 1];
    }

    public async Task<bool> SetRelayONOFFAsyncCheckResult(int relay, int onOff)
    {
        if (relayStates[relay - 1] == (onOff == 1))
        {
            return true;
        }
        string cmd = ((onOff == 1) ? $"WR{relay:D3}1\n" : $"WR{relay:D3}0\n");
        try
        {
            await SendCommandAndWaitLineResponseAsync(cmd);
            await GetAllRelay();
            if (relayStates[relay - 1] == (onOff == 1))
            {
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            log.Error((object)("Lỗi gửi lệnh: " + ex.Message));
            return false;
        }
    }
    public async Task SetAllRelayOFF()
    {
        try
        {
            await GetAllRelay();
            for (int i = 0; i < relayStates.Length; i++)
            {
                if (relayStates[i] == false)
                {
                    continue;
                }
                int iRelay = i + 1;
                string cmd = $"WR{iRelay:D3}0\n";
                log.Debug((object)($"Set Relay {iRelay} to OFF: " + cmd));
                await SendCommandAndWaitLineResponseAsync(cmd);
            }
        }
        catch (Exception ex)
        {
            log.Error((object)("Error send command: " + ex.Message));
        }
        
    }
    public async Task<bool> SetFlowRateCheckResult(int channel, int FlowRate)
    {
        try
        {
            string cmd = $"WFLOW{channel}{FlowRate:D4}\n";
            string response = (await SendCommandAndWaitLineResponseAsync(cmd))[0];
            if (response.Contains(cmd.Trim()))
            {
                return true;
            }
            log.Error((object)("Failed send command: " + response));
            return false;
        }
        catch (Exception ex)
        {
            Exception ex2 = ex;
            log.Error((object)("Error send command: " + ex2.Message));
            return false;
        }
    }
    public async void ConnectASPSerial()
    {
        try
        {
            SerialPort = new SerialPort(SerialCom, Baudrate, Parity, DataBit, StopBit);
            SerialPort.Open();
            SerialPort.DataReceived += SerialPort_DataReceived;
            _isConnected = true;
            DateTime dt = DateTime.Now;
            await GetAllRelay();
            AddLogText(dt.ToString("HH:mm:ss.fff") + " ASP Serial open");
        }
        catch (Exception ex)
        {
            Exception ex2 = ex;
            log.Error((object)("Exception when connect to ASP Serial :" + ex2.Message));
        }
    }

    public void DisconnectASPSerial()
    {
        try
        {
            SerialPort.Close();
            _isConnected = false;
            AddLogText(DateTime.Now.ToString("HH:mm:ss.fff")+ " ASP Serial closed");
        }
        catch (Exception ex)
        {
            log.Error((object)("Exception when disconnect to ASP Serial :" + ex.Message));
        }
    }

    public async void ASPSerialWriteCommand(string text)
    {
        if (!_isConnected)
        {
            SerialPort = new SerialPort(SerialCom, Baudrate, Parity, DataBit, StopBit);
            SerialPort.Open();
            SerialPort.DataReceived += SerialPort_DataReceived;
            _isConnected = true;
            AddLogText(DateTime.Now.ToString("HH:mm:ss.fff") + " ASP Serial RE-open");
        }
        try
        {
            AddLogText(DateTime.Now.ToString("HH:mm:ss.fff") + " ASP Serial write: " + text);
            SerialPort.Write(text);
        }
        catch (Exception ex)
        {
            log.Error((object)("Exception when write to ASP Serial :" + ex.Message));
        }
    }

    public void UpdateADC()
    {
        this.OnRequestUpdateADC?.Invoke(this);
    }

    public void UpdaterelayStatus()
    {
        this.OnRequestUpdateRelayStatus?.Invoke(this);
    }

    public void AddLogText(string _text)
    {
        this.OnRequestAddLog?.Invoke(_text);
    }
    public void UpdateHumidityTemp()
    {
        this.OnRequestUpdateHumnidityTemp?.Invoke(this);
    }
    public async Task<bool> RequestTriggerScanner(int Index)
    {
        SerialNumber1 = "";
        try
        {
            string cmd = $"WS{Index.ToString("00")}\n";
            string[] response = await SendCommandAndWaitLineResponseAsync(cmd, 2, 6000, 6000);
            if (response.Length < 2) return false;
            if (response[0].Trim() == cmd.Trim())
            {
                switch (Index)
                {
                    case 1:
                        SerialNumber1 = response[1].Trim();
                        UpdateSerialSN(Index, SerialNumber1);
                        break;
                    case 2:
                        SerialNumber2 = response[1].Trim();
                        UpdateSerialSN(Index, SerialNumber1);
                        break;
                    case 3:
                        SerialNumber3 = response[1].Trim();
                        UpdateSerialSN(Index, SerialNumber1);
                        break;
                    case 4:
                        SerialNumber4 = response[1].Trim();
                        UpdateSerialSN(Index, SerialNumber1);
                        break;
                }
                return true;
            }
            else
            {
                log.Error((object)("ASP Serial Scanner " + Index.ToString("00") + " response: " + response[0]));
                return false;
            }
        }
        catch (Exception ex)
        {
            log.Error((object)("Error send command: " + ex.Message));
            return false;
        }
        
    }
    public void UpdateSerialSN(int IndexSN, string SN)
    {
        switch (IndexSN)
        {
            case 1:
                this.OnRequestUpdateSN_1?.Invoke(SN); break;
            case 2:
                this.OnRequestUpdateSN_2?.Invoke(SN); break;
            case 3:
                this.OnRequestUpdateSN_3?.Invoke(SN); break;
            case 4:
                this.OnRequestUpdateSN_4?.Invoke(SN); break;
        }
    }
    public void UpdateStatusInput()
    {
        this.OnRequestUpdateStatusInput?.Invoke(this);
    }

    //public async Task<string> SendCommandAndWaitLineResponseAsync(string command, int timeoutResponse = 1500, int timeoutAwait = 3500)
    //{
    //    if (SerialPort == null || !SerialPort.IsOpen)
    //    {
    //        try
    //        {
    //            SerialPort = new SerialPort(SerialCom, Baudrate, Parity, DataBit, StopBit);
    //            SerialPort.Open();
    //            SerialPort.DataReceived += SerialPort_DataReceived;
    //            _isConnected = true;
    //            AddLogText(DateTime.Now.ToString("HH:mm:ss") + "+ ASP Serial Re-open");
    //        }
    //        catch (Exception)
    //        {
    //            throw new InvalidOperationException("Serial port can not re-open.");
    //        }
    //    }
    //    await WaitUntilNoPendingResponse(timeoutAwait);
    //    _isWaitingResponse = true;
    //    _buffer.Clear();
    //    _expectedLineCount = 1; // << số dòng cần nhận
    //    _tcs = new TaskCompletionSource<string>();
    //    try
    //    {
    //        DateTime dt = DateTime.Now;
    //        AddLogText($"{dt:HH:mm:ss}+ ASP Serial write: {command}");
    //        SerialPort.Write(command);
    //        Task completedTask = await Task.WhenAny(_tcs.Task, Task.Delay(timeoutResponse));
    //        if (_tcs == null)
    //        {
    //            throw new InvalidOperationException("TaskCompletionSource is null.");
    //        }
    //        if (completedTask == _tcs.Task)
    //        {
    //            return await _tcs.Task;
    //        }
    //        throw new TimeoutException("Timeout waiting for response.");
    //    }
    //    finally
    //    {
    //        _isWaitingResponse = false;
    //        _tcs = null;
    //    }
    //}
    public async Task<string[]> SendCommandAndWaitLineResponseAsync(
                 string command,
                 int expectedLineCount = 1,
                 int timeoutResponse = 1500,
                 int timeoutAwait = 7000)
    {
        await _cmdLock.WaitAsync();   // 🔒 chỉ cho 1 cmd chạy
        try
        {
            if (SerialPort == null || !SerialPort.IsOpen)
            {
                try
                {
                    SerialPort = new SerialPort(SerialCom, Baudrate, Parity, DataBit, StopBit);
                    SerialPort.Open();
                    SerialPort.DataReceived -= SerialPort_DataReceived;
                    SerialPort.DataReceived += SerialPort_DataReceived;
                    _isConnected = true;
                    AddLogText($"{DateTime.Now:HH:mm:ss.fff} ASP Serial Re-open");
                }
                catch (Exception)
                {
                    throw new InvalidOperationException("Serial port can not re-open.");
                }
            }

            await WaitUntilNoPendingResponse(timeoutAwait);

            _isWaitingResponse = true;
            _buffer.Clear();
            _receivedLines.Clear();
            _expectedLineCount = expectedLineCount;

            var tcs = new TaskCompletionSource<string[]>(TaskCreationOptions.RunContinuationsAsynchronously);
            _tcs = tcs;

            try
            {
                AddLogText($"{DateTime.Now:HH:mm:ss.fff} ASP Serial write: {command}");
                SerialPort.Write(command);

                Task completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeoutResponse));

                if (completedTask == tcs.Task)
                {
                    return await tcs.Task; // đủ dòng
                }

                _tcs = null;
                throw new TimeoutException("Timeout waiting for response.");
            }
            finally
            {
                _isWaitingResponse = false;
                if (_tcs == tcs)
                    _tcs = null;
            }
        }
        finally
        {
            _cmdLock.Release();  // 🔓 cho command kế tiếp chạy
        }
    }


    private async Task WaitUntilNoPendingResponse(int maxWaitMs = 5000, int pollIntervalMs = 50)
    {
        int waited = 0;
        while (_isWaitingResponse)
        {
            await Task.Delay(pollIntervalMs);
            waited += pollIntervalMs;
            if (waited >= maxWaitMs)
            {
                throw new TimeoutException("Timeout waiting for previous response to complete.");
            }
        }
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            SerialPort sp = (SerialPort)sender;
            string incoming = sp.ReadExisting();
            _buffer.AddRange(Encoding.ASCII.GetBytes(incoming));

            while (true)
            {

                int newLineIndex = _buffer.IndexOf(10); // LF (\n)
                if (newLineIndex < 0)
                    break;
                byte[] lineBytes = _buffer.Take(newLineIndex + 1).ToArray();
                _buffer.RemoveRange(0, newLineIndex + 1);
                string line = Encoding.ASCII.GetString(lineBytes).Trim();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    _receivedLines.Add(line);
                    AddLogText($"{DateTime.Now:HH:mm:ss.fff} ASP Serial read: {line}");
                    // chỉ set nếu vẫn còn người chờ (_tcs != null)
                    if (_tcs != null && _receivedLines.Count >= _expectedLineCount)
                    {
                        _tcs.TrySetResult(_receivedLines.ToArray());
                        _tcs = null; // clear để không set lại sau timeout
                        _receivedLines.Clear();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            log.Error("Error in SerialPort_DataReceived", ex);
            _tcs?.TrySetException(ex);
            _tcs = null;
        }
    }


    private void GetASPSerialPortInformation()
    {
        SerialCom = ClsIO.ReadValue("ASPSerialPort", "SerialCom", "COM1", "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
        Baudrate = int.Parse(ClsIO.ReadValue("ASPSerialPort", "Baudrate", "9600", "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini"));
        DataBit = int.Parse(ClsIO.ReadValue("ASPSerialPort", "DataBit", "8", "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini"));
        switch (ClsIO.ReadValue("ASPSerialPort", "Parity", "None", "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini"))
        {
            case "None":
                Parity = Parity.None;
                break;
            case "Odd":
                Parity = Parity.Odd;
                break;
            case "Even":
                Parity = Parity.Even;
                break;
        }
        switch (ClsIO.ReadValue("ASPSerialPort", "StopBit", "One", "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini"))
        {
            case "One":
                StopBit = StopBits.One;
                break;
            case "OnePointFive":
                StopBit = StopBits.OnePointFive;
                break;
            case "Two":
                StopBit = StopBits.Two;
                break;
        }
    }
    public async Task<bool> ReadSENSSORHC2(int iSensor)
    {
        string cmd = $"RHC2{iSensor}\n";
        try
        {
            string response = (await SendCommandAndWaitLineResponseAsync(cmd))[0];
            string subresponse = "";
            if (response.Contains(cmd.Trim()))
            {
                subresponse = response.Substring(cmd.Length);
            }
            string sHumidity = subresponse.Substring(0, 5);
            string sTempCmd = subresponse.Substring(5, 5);
            _Humiditys[iSensor - 1] = double.Parse(sHumidity);
            _Temps[iSensor - 1] = double.Parse(sTempCmd);
        }
        catch (Exception ex)
        {
            Exception ex2 = ex;
            log.Error((object)("Lỗi gửi lệnh: " + ex2.Message));
            return false;
        }
        UpdateHumidityTemp();
        return true;
    }
    public void SaveASPSerialPortInformation()
    {
        ClsIO.WriteValue("ASPSerialPort", "SerialCom", SerialCom, "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
        ClsIO.WriteValue("ASPSerialPort", "Baudrate", Baudrate.ToString(), "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
        ClsIO.WriteValue("ASPSerialPort", "DataBit", DataBit.ToString(), "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
        ClsIO.WriteValue("ASPSerialPort", "Parity", Parity.ToString(), "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
        ClsIO.WriteValue("ASPSerialPort", "StopBit", StopBit.ToString(), "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
    }
}

