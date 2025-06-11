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

namespace DI_Water_Wash
{
    public class Cls_ASPcontrol
    {
        public delegate void DelShow_addLog_Request(string sender);

        public delegate void DelShow_UpdateRelayStatus_Request(Cls_ASPcontrol sender);

        public delegate void DelShow_UpdateADC_Request(Cls_ASPcontrol sender);

        private static readonly ILog log = LogManager.GetLogger(typeof(Cls_ASPcontrol));

        private bool Asyncrelay = false;

        private bool _isConnected = false;

        private double[] _ADVs = new double[4];

        private double[] _ADIs = new double[4];

        private List<byte> _buffer = new List<byte>();

        private TaskCompletionSource<string> _tcs = new TaskCompletionSource<string>();

        public string SerialCom { get; set; }

        public int Baudrate { get; set; }

        public int DataBit { get; set; }

        public Parity Parity { get; set; }

        public StopBits StopBit { get; set; }

        public SerialPort SerialPort { get; private set; }

        public string DataReceive { get; private set; }

        public string AllDataReceive { get; private set; }

        public bool[] relayStates { get; private set; }

        public double[] ADVs => _ADVs;

        public double[] ADIs => _ADIs;

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

        public Cls_ASPcontrol()
        {
            GetASPSerialPortInformation();
            ConnectASPSerial();
            DataReceive = "";
            AllDataReceive = "";
            relayStates = new bool[30];
            for (int i = 0; i < relayStates.Length; i++)
            {
                relayStates[i] = false;
            }
        }

        public void StartRelayReading()
        {
            Asyncrelay = true;
        }

        public async Task GetAllRelay()
        {
            try
            {
                string bitString = (await SendCommandAndWaitResponseAsync("RR000\n")).Substring(5, 30);
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

        public async Task SetRelayONOFFAsync(int relay, int onOff)
        {
            if (relayStates[relay - 1] == (onOff == 1))
            {
                return;
            }
            string cmd = ((onOff == 1) ? $"WR{relay:D3}1\n" : $"WR{relay:D3}0\n");
            try
            {
                await SendCommandAndWaitResponseAsync(cmd);
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
                string response = await SendCommandAndWaitResponseAsync(cmd);
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
                string response = await SendCommandAndWaitResponseAsync(cmd);
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
                await SendCommandAndWaitResponseAsync(cmd);
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
            await GetAllRelay();
            for (int i=0;i<relayStates.Length;i++)
            {
                if (relayStates[i] == false)
                {
                    continue;
                }
                int iRelay = i + 1;
                string cmd =$"WR{iRelay:D3}0\n";
                log.Debug((object)($"Set Relay {iRelay} to OFF: " + cmd));
                await SendCommandAndWaitResponseAsync(cmd);
            }
        }
        public async Task<bool> SetFlowRateCheckResult(int channel, int FlowRate)
        {
            try
            {
                string cmd = $"WFLOW{channel}{FlowRate:D4}\n";
                string response = await SendCommandAndWaitResponseAsync(cmd);
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
                AddLogText(dt.ToString("HH:mm:ss") + "+ ASP Serial open");
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
                AddLogText(DateTime.Now.ToString("HH:mm:ss") + "+ ASP Serial closed");
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
                AddLogText(DateTime.Now.ToString("HH:mm:ss") + "+ ASP Serial RE-open");
            }
            try
            {
                AddLogText(DateTime.Now.ToString("HH:mm:ss") + "+ ASP Serial write: " + text);
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

        public async Task<string> SendCommandAndWaitResponseAsync(string command, int timeoutResponse = 1500, int timeoutAwait = 3500)
        {
            if (SerialPort == null || !SerialPort.IsOpen)
            {
                try
                {
                    SerialPort = new SerialPort(SerialCom, Baudrate, Parity, DataBit, StopBit);
                    SerialPort.Open();
                    SerialPort.DataReceived += SerialPort_DataReceived;
                    _isConnected = true;
                    AddLogText(DateTime.Now.ToString("HH:mm:ss") + "+ ASP Serial Re-open");
                }
                catch (Exception)
                {
                    throw new InvalidOperationException("Serial port can not re-open.");
                }
            }
            await WaitUntilNoPendingResponse(timeoutAwait);
            _isWaitingResponse = true;
            _buffer.Clear();
            _tcs = new TaskCompletionSource<string>();
            try
            {
                DateTime dt = DateTime.Now;
                AddLogText($"{dt:HH:mm:ss}+ ASP Serial write: {command}");
                SerialPort.Write(command);
                Task completedTask = await Task.WhenAny(_tcs.Task, Task.Delay(timeoutResponse));
                if (_tcs == null)
                {
                    throw new InvalidOperationException("TaskCompletionSource is null.");
                }
                if (completedTask == _tcs.Task)
                {
                    return await _tcs.Task;
                }
                throw new TimeoutException("Timeout waiting for response.");
            }
            finally
            {
                _isWaitingResponse = false;
                _tcs = null;
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
            DataReceive = "";
            try
            {
                SerialPort serialPort = (SerialPort)sender;
                string s = serialPort.ReadExisting();
                _buffer.AddRange(Encoding.ASCII.GetBytes(s));
                if (_buffer.Contains(10))
                {
                    string text = Encoding.ASCII.GetString(_buffer.ToArray()).Trim();
                    _buffer.Clear();
                    _tcs?.TrySetResult(text);
                    DateTime now = DateTime.Now;
                    AddLogText($"{now:HH:mm:ss}+ ASP Serial read: {text}");
                }
            }
            catch (Exception exception)
            {
                _tcs?.TrySetException(exception);
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

        public void SaveASPSerialPortInformation()
        {
            ClsIO.WriteValue("ASPSerialPort", "SerialCom", SerialCom, "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
            ClsIO.WriteValue("ASPSerialPort", "Baudrate", Baudrate.ToString(), "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
            ClsIO.WriteValue("ASPSerialPort", "DataBit", DataBit.ToString(), "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
            ClsIO.WriteValue("ASPSerialPort", "Parity", Parity.ToString(), "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
            ClsIO.WriteValue("ASPSerialPort", "StopBit", StopBit.ToString(), "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
        }
    }

}
