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

namespace DI_Water_Wash
{
    public class Cls_ASPcontrol
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Cls_ASPcontrol));
        public string SerialCom { get; set; }
        public int Baudrate { get; set; }
        public int DataBit { get; set; }
        public Parity Parity { get; set; }
        public StopBits StopBit { get; set; }
        public SerialPort SerialPort { get; private set; }
        public string DataReceive { get; private set; }
        public string AllDataReceive { get; private set; }
        public bool[] relayStates { get; private set; }
        private bool Asyncrelay = false;
        public delegate void DelShow_addLog_Request(string sender);
        public event DelShow_addLog_Request OnRequestAddLog;
        public delegate void DelShow_UpdateRelayStatus_Request(Cls_ASPcontrol sender);
        public event DelShow_UpdateRelayStatus_Request OnRequestUpdateRelayStatus;
        public delegate void DelShow_UpdateADC_Request(Cls_ASPcontrol sender);
        public event DelShow_UpdateADC_Request OnRequestUpdateADC;
        private bool _isConnected = false;
        private double[] _ADVs = new double[] { 0.0, 0.0, 0.0, 0.0 };
        private double[] _ADIs = new double[] { 0.0, 0.0, 0.0, 0.0 };
        public double[] ADVs
        {
            get { return _ADVs; }
        }
        public double[] ADIs
        {
            get { return _ADIs; }
        }
        public bool isConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; }
        }
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
                string response = await SendCommandAndWaitResponseAsync("RR000\n");
                string bitString = response.Substring(5, 30);
                bool[] result = bitString.Select(c => c == '1').ToArray();

                if (result.Length != 30)
                {
                    log.Error("bit length incorrect: " + bitString.Length);
                }
                relayStates = result;
                UpdaterelayStatus();
            }
            catch (Exception ex)
            {
                log.Error("Error in GetAllRelay", ex);
            }
        }
        public async Task SetRelayONOFFAsync(int relay, int onOff)
        {
            if (relayStates[relay-1] == (onOff == 1))
            {
                return;
            }
            string cmd = onOff == 1 ? $"WR{relay:D3}1\n" : $"WR{relay:D3}0\n";
            try
            {
                string response = await SendCommandAndWaitResponseAsync(cmd);    
            }
            catch (Exception ex)
            {
                log.Error($"Lỗi gửi lệnh: {ex.Message}");
            }
        }
        public async Task<double> GetADVAsync(int iFlowChannel)
        {
            string cmd =$"RADV{iFlowChannel}\n";
            try
            {
                string response = await SendCommandAndWaitResponseAsync(cmd);
                string subresponse = "";
                if (response.Contains(cmd.Trim()))
                {
                    subresponse = response.Substring(cmd.Length-1);
                }
                _ADVs[iFlowChannel - 1] = double.Parse(subresponse) * 1000.0; 
            }
            catch (Exception ex)
            {
                log.Error($"Lỗi gửi lệnh: {ex.Message}");
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
                    subresponse = response.Substring(cmd.Length-1);
                }
                _ADIs[iFlowChannel-1] = double.Parse(subresponse);
            }
            catch (Exception ex)
            {
                log.Error($"Lỗi gửi lệnh: {ex.Message}");
            }
            UpdateADC();
            return _ADIs[iFlowChannel-1];
        }
        public async Task<bool> SetRelayONOFFAsyncCheckResult(int relay, int onOff)
        {
            if (relayStates[relay-1] == (onOff == 1))
            {
                return true;
            }
            string cmd = onOff == 1 ? $"WR{relay:D3}1\n" : $"WR{relay:D3}0\n";
            try
            {
                string response = await SendCommandAndWaitResponseAsync(cmd);
                // Cập nhật trạng thái sau khi gửi lệnh
                await GetAllRelay();
                // Kiểm tra trạng thái relay có đúng như yêu cầu không
                if (relayStates[relay - 1] == (onOff == 1))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.Error($"Lỗi gửi lệnh: {ex.Message}");
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
                AddLogText($"{dt.ToString("HH:mm:ss")}+ ASP Serial open");
            }
            catch (Exception ex)
            {
                log.Error($"Exception when connect to ASP Serial :" + ex.Message);
            }
         }
        public void DisconnectASPSerial()
        {
            try
            {
                SerialPort.Close();
                _isConnected = false;
                DateTime dt = DateTime.Now;
                AddLogText($"{dt.ToString("HH:mm:ss")}+ ASP Serial closed");
            }
            catch (Exception ex)
            {
                log.Error($"Exception when disconnect to ASP Serial :" + ex.Message);
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
                DateTime dt = DateTime.Now;
                AddLogText($"{dt.ToString("HH:mm:ss")}+ ASP Serial RE-open");
            }
            try
            {
                DateTime dt = DateTime.Now;
                AddLogText($"{dt.ToString("HH:mm:ss")}+ ASP Serial write: " + text);
                SerialPort.Write(text);
            }
            catch (Exception ex)
            {
                log.Error($"Exception when write to ASP Serial :" + ex.Message);
            }
        }
        public void UpdateADC()
        {
            OnRequestUpdateADC?.Invoke(this);
        }
        public void UpdaterelayStatus()
        {
            OnRequestUpdateRelayStatus?.Invoke(this);
        }
        public void AddLogText(string _text)
        {
            OnRequestAddLog?.Invoke(_text);
        }
        private List<byte> _buffer = new List<byte>();
        // Replace the nullable reference type declaration with a non-nullable type
        private TaskCompletionSource<string> _tcs = new TaskCompletionSource<string>();

        public bool _isWaitingResponse { get; private set; } // trạng thái đang chờ phản hồi
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
                    DateTime dt = DateTime.Now;
                    AddLogText($"{dt.ToString("HH:mm:ss")}+ ASP Serial Re-open");
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
                // Chờ dữ liệu trả về hoặc timeout
                var completedTask = await Task.WhenAny(_tcs.Task, Task.Delay(timeoutResponse));
                if(_tcs==null)
                {
                    throw new InvalidOperationException("TaskCompletionSource is null.");
                }
                if (completedTask == _tcs.Task)
                {
                    string result = await _tcs.Task;
                    return result;
                }
                else
                {
                    throw new TimeoutException("Timeout waiting for response.");
                }
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
                    throw new TimeoutException("Timeout waiting for previous response to complete.");
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            DataReceive = "";
            try
            {
                var sp = (SerialPort)sender;
                string data = sp.ReadExisting();
                _buffer.AddRange(Encoding.ASCII.GetBytes(data));

                if (_buffer.Contains(0x0A)) // khi thấy LF (0x0A)
                {
                    string fullData = Encoding.ASCII.GetString(_buffer.ToArray()).Trim();
                    _buffer.Clear();
                    _tcs?.TrySetResult(fullData);
                    DateTime dt = DateTime.Now;
                    AddLogText($"{dt:HH:mm:ss}+ ASP Serial read: {fullData}");
                }
            }
            catch (Exception ex)
            {
                _tcs?.TrySetException(ex);
            }
        }
        private void GetASPSerialPortInformation()
        {
            SerialCom = ClsIO.ReadValue("ASPSerialPort", "SerialCom", "COM1", @"C:\Aavid_Test\Setup-ini\SerialPort.ini");
            Baudrate = int.Parse(ClsIO.ReadValue("ASPSerialPort", "Baudrate", "9600", @"C:\Aavid_Test\Setup-ini\SerialPort.ini"));
            DataBit = int.Parse(ClsIO.ReadValue("ASPSerialPort", "DataBit", "8", @"C:\Aavid_Test\Setup-ini\SerialPort.ini"));
            string parityy = ClsIO.ReadValue("ASPSerialPort", "Parity", "None", @"C:\Aavid_Test\Setup-ini\SerialPort.ini");
            switch (parityy)
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
            string stoppit = ClsIO.ReadValue("ASPSerialPort", "StopBit", "One", @"C:\Aavid_Test\Setup-ini\SerialPort.ini");
            switch (stoppit)
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
            ClsIO.WriteValue("ASPSerialPort", "SerialCom", SerialCom, @"C:\Aavid_Test\Setup-ini\SerialPort.ini");
            ClsIO.WriteValue("ASPSerialPort", "Baudrate", Baudrate.ToString(), @"C:\Aavid_Test\Setup-ini\SerialPort.ini");
            ClsIO.WriteValue("ASPSerialPort", "DataBit", DataBit.ToString(), @"C:\Aavid_Test\Setup-ini\SerialPort.ini");
            ClsIO.WriteValue("ASPSerialPort", "Parity", Parity.ToString(), @"C:\Aavid_Test\Setup-ini\SerialPort.ini");
            ClsIO.WriteValue("ASPSerialPort", "StopBit", StopBit.ToString(), @"C:\Aavid_Test\Setup-ini\SerialPort.ini");
        }
    }
}
