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
        public bool isConnected { get; private set; }
        public bool[] relayStates { get; private set; }
        private bool Asyncrelay = false;
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
                    MessageBox.Show("bit length incorrect.");
                }
                relayStates = result;
            }
            catch (Exception ex)
            {
                log.Error("Error in GetAllRelay", ex);
                MessageBox.Show("Failed to read relays: " + ex.Message);
            }
        }
        public async Task SetRelayONOFFAsync(int relay, int onOff)
        {
            string cmd = onOff == 1 ? $"WR{relay:D3}1\n" : $"WR{relay:D3}0\n";
            try
            {
                string response = await SendCommandAndWaitResponseAsync(cmd);
                // Bạn có thể kiểm tra phản hồi nếu muốn
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi gửi lệnh: {ex.Message}");
            }
        }
        public void ConnectASPSerial()
        {
            try
            {
                SerialPort = new SerialPort(SerialCom, Baudrate, Parity, DataBit, StopBit);
                SerialPort.Open();
                SerialPort.DataReceived += SerialPort_DataReceived;
                isConnected = true;
                DateTime dt = DateTime.Now;
                DataReceive = $"{dt.ToString("HH:mm:ss")}+ ASP Serial open";
                AddLogText();
            }
            catch (Exception ex)
            {
                log.Error($"Exception when connect to ASP Serial :" + ex.Message);
                MessageBox.Show($"Exception when connect to ASP Serial :" + ex.Message);
            }
        }
        public void DisconnectASPSerial()
        {
            try
            {
                SerialPort.Close();
                isConnected = false;
                DateTime dt = DateTime.Now;
                DataReceive = $"{dt.ToString("HH:mm:ss")}+ ASP Serial closed";
                AddLogText();
            }
            catch (Exception ex)
            {
                log.Error($"Exception when disconnect to ASP Serial :" + ex.Message);
                MessageBox.Show($"Exception when disconnect to ASP Serial :" + ex.Message);
            }
        }
        public void ASPSerialWriteCommand(string text)
        {
            if (!isConnected)
            {
                ConnectASPSerial();
            }
            try
            {
                DateTime dt = DateTime.Now;
                DataReceive = "";
                DataReceive = $"{dt.ToString("HH:mm:ss")}+ ASP Serial write: " + text;
                AddLogText();
                SerialPort.Write(text);
            }
            catch (Exception ex)
            {
                log.Error($"Exception when write to ASP Serial :" + ex.Message);
                MessageBox.Show($"Exception when write to ASP Serial :" + ex.Message);
            }
        }
        public delegate void DelShow_addLog_Request(Cls_ASPcontrol sender);
        public event DelShow_addLog_Request OnRequestAddLog;

        public void AddLogText()
        {
            OnRequestAddLog?.Invoke(this);
        }
        private List<byte> _buffer = new List<byte>();
        // Replace the nullable reference type declaration with a non-nullable type
        private TaskCompletionSource<string> _tcs;

        public bool _isWaitingResponse { get; private set; } // trạng thái đang chờ phản hồi
        public async Task<string> SendCommandAndWaitResponseAsync(string command, int timeoutMs = 1500)
        {
            if (SerialPort == null || !SerialPort.IsOpen)
                throw new InvalidOperationException("Serial port not open.");
            if (_isWaitingResponse)
                throw new InvalidOperationException("A response is already being awaited.");
            _isWaitingResponse = true;
            _buffer.Clear();
            _tcs = new TaskCompletionSource<string>();
            try
            {
                DateTime dt = DateTime.Now;
                DataReceive = $"{dt:HH:mm:ss}+ ASP Serial write: {command}";
                AddLogText();
                SerialPort.Write(command);
                // Chờ dữ liệu trả về hoặc timeout
                var completedTask = await Task.WhenAny(_tcs.Task, Task.Delay(timeoutMs));
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
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
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
                    DataReceive = $"{dt:HH:mm:ss}+ ASP Serial read: {fullData}";
                    AddLogText();
                }
            }
            catch (Exception ex)
            {
                _tcs?.TrySetException(ex);
            }
        }
        private void GetASPSerialPortInformation()
        {
            SerialCom = ClsIO.ReadValue("ASPSerialPort", "SerialCom", "COM1", @"C:\Aavid_Test\Setup-ini\ASPSerialPort.ini");
            Baudrate = int.Parse(ClsIO.ReadValue("ASPSerialPort", "Baudrate", "9600", @"C:\Aavid_Test\Setup-ini\ASPSerialPort.ini"));
            DataBit = int.Parse(ClsIO.ReadValue("ASPSerialPort", "DataBit", "8", @"C:\Aavid_Test\Setup-ini\ASPSerialPort.ini"));
            string parityy = ClsIO.ReadValue("ASPSerialPort", "Parity", "None", @"C:\Aavid_Test\Setup-ini\ASPSerialPort.ini");
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
            string stoppit = ClsIO.ReadValue("ASPSerialPort", "StopBit", "One", @"C:\Aavid_Test\Setup-ini\ASPSerialPort.ini");
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
            ClsIO.WriteValue("ASPSerialPort", "SerialCom", SerialCom, @"C:\Aavid_Test\Setup-ini\ASPSerialPort.ini");
            ClsIO.WriteValue("ASPSerialPort", "Baudrate", Baudrate.ToString(), @"C:\Aavid_Test\Setup-ini\ASPSerialPort.ini");
            ClsIO.WriteValue("ASPSerialPort", "DataBit", DataBit.ToString(), @"C:\Aavid_Test\Setup-ini\ASPSerialPort.ini");
            ClsIO.WriteValue("ASPSerialPort", "Parity", Parity.ToString(), @"C:\Aavid_Test\Setup-ini\ASPSerialPort.ini");
            ClsIO.WriteValue("ASPSerialPort", "StopBit", StopBit.ToString(), @"C:\Aavid_Test\Setup-ini\ASPSerialPort.ini");
        }
    }
}
