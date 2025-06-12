using log4net;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Device;
using System.Windows.Forms;

namespace Hot_Air_Drying
{
    public class ClsInverterModbus
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
        private IModbusSerialMaster _modbusMaster;
        // Địa chỉ thanh ghi
        private const ushort ADDRESS_COMMAND_OPERATION = 0x0006; // Lệnh điều khiển
        private const ushort ADDRESS_COMMAND_FREQUENCY = 0x0005; // Tần số lệnh (0.01 Hz)
        private const ushort ADDRESS_COMMAND_FREQUENCY_OUT = 0x000A; // Tần số đầu ra (0.01 Hz)
        private bool _IsOpen = false;
        public bool IsOpen
        {
            get { return _IsOpen; }
            set { _IsOpen = value; }
        }
        public ClsInverterModbus()
        {
            GetInverterSerialPortInformation();
            SerialPort = new SerialPort(SerialCom, Baudrate, Parity, DataBit, StopBit);
            Open();
            SerialPort.DataReceived += SerialPort_DataReceived;
        }

        private void GetInverterSerialPortInformation()
        {
            SerialCom = ClsIO.ReadValue("InverterSerialPort", "SerialCom", "COM1", @"C:\Aavid_Test\Setup-ini\SerialPort.ini");
            Baudrate = int.Parse(ClsIO.ReadValue("InverterSerialPort", "Baudrate", "9600", @"C:\Aavid_Test\Setup-ini\SerialPort.ini"));
            DataBit = int.Parse(ClsIO.ReadValue("InverterSerialPort", "DataBit", "8", @"C:\Aavid_Test\Setup-ini\SerialPort.ini"));
            string parityy = ClsIO.ReadValue("InverterSerialPort", "Parity", "None", @"C:\Aavid_Test\Setup-ini\SerialPort.ini");
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
            string stoppit = ClsIO.ReadValue("InverterSerialPort", "StopBit", "One", @"C:\Aavid_Test\Setup-ini\SerialPort.ini");
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

        public void Open()
        {
            try
            {
                if (!SerialPort.IsOpen)
                    SerialPort.Open();
                _modbusMaster = ModbusSerialMaster.CreateRtu(SerialPort);
                _IsOpen = true;
            }
            catch (Exception ex)
            {
                log.Error("Error opening RS485 serial port: " + ex.Message);
            }
            
        }
        public void Close()
        {
            try
            {
                if (SerialPort.IsOpen)
                    SerialPort.Close();
                _modbusMaster?.Dispose();
                _IsOpen = false;
            }
            catch (Exception ex)
            {
                log.Error("Error closing RS485 serial port: " + ex.Message);
            }
            
        }
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }
        // Đọc holding registers
        public ushort[] ReadRegisters(byte _slaveId, ushort startAddress, ushort numberOfPoints)
        {
            if(!_IsOpen)
            {
                try
                {
                    log.Info("Re-Opening RS485 serial port: " + SerialCom);
                    Open();
                }
                catch (Exception ex)
                {
                    log.Error("Error opening RS485 serial port: " + ex.Message);
                    return null;
                }
            }
            return _modbusMaster.ReadHoldingRegisters(_slaveId, startAddress, numberOfPoints);
        }

        // Ghi single register
        public void WriteRegister(byte _slaveId, ushort registerAddress, ushort value)
        {
            if (!_IsOpen)
            {
                try
                {
                    log.Info("Re-Opening RS485 serial port: " + SerialCom);
                    Open();
                }
                catch (Exception ex)
                {
                    log.Error("Error opening RS485 serial port: " + ex.Message);
                    return;
                }
            }
            _modbusMaster.WriteSingleRegister(_slaveId, registerAddress, value);
        }

        // Ghi multiple registers
        public void WriteRegisters(byte _slaveId, ushort startAddress, ushort[] values)
        {
            if (!_IsOpen)
            {
                try
                {
                    log.Info("Re-Opening RS485 serial port: " + SerialCom);
                    Open();
                }
                catch (Exception ex)
                {
                    log.Error("Error opening RS485 serial port: " + ex.Message);
                    return;
                }
            }
            _modbusMaster.WriteMultipleRegisters(_slaveId, startAddress, values);
        }
        public bool SetInverterOnOff(byte slaveId, bool onoff)
        {
            bool result = false;
            ushort commandValue = onoff ? (ushort)(1 << 1) : (ushort)(1 << 0); // ON = B1, OFF = B0
            result = WriteAndVerifyRegister(slaveId, ADDRESS_COMMAND_OPERATION, commandValue);
            log.Info($"Set inverter {(onoff ? "ON" : "OFF")} - Write value 0x{commandValue:X4} to register 0x{ADDRESS_COMMAND_OPERATION:X4}");
            return result;
        }
        public bool SetInverterEmergencyStop(byte slaveId)
        {
            bool result = false;
            ushort commandValue = 1<<4; // B4 = Emergency Stop
            result = WriteAndVerifyRegister(slaveId, ADDRESS_COMMAND_OPERATION, commandValue);
            log.Info($"Set inverter Emergency Stop - Write value 0x{commandValue:X4} to register 0x{ADDRESS_COMMAND_OPERATION:X4}");
            return result;
        }
        public bool WriteAndVerifyRegister(byte slaveId, ushort registerAddress, ushort value)
        {
            try
            {
                // Ghi giá trị vào thanh ghi
                WriteRegister(slaveId, registerAddress, value);

                // Đọc lại giá trị thanh ghi sau ghi
                ushort[] readBack = ReadRegisters(slaveId, registerAddress, 1);
                if (readBack == null || readBack.Length == 0)
                {
                    log.Error("Không đọc được thanh ghi để xác nhận ghi.");
                    return false;
                }

                // So sánh giá trị đọc lại với giá trị đã ghi
                bool success = readBack[0] == value;
                if (!success)
                {
                    log.Warn($"Giá trị đọc lại 0x{readBack[0]:X4} khác với giá trị ghi 0x{value:X4}");
                }
                else
                {
                    log.Info($"Ghi thanh ghi 0x{registerAddress:X4} thành công với giá trị 0x{value:X4}");
                }
                return success;
            }
            catch (Exception ex)
            {
                log.Error("Lỗi khi ghi và xác nhận thanh ghi: " + ex.Message);
                return false;
            }
        }
        public bool SetInverterFrequencyAndVerify(byte slaveId, double frequency)
        {
            try
            {
                // Chuyển tần số sang giá trị thanh ghi (scale 0.01 Hz)
                ushort valueToWrite = (ushort)(frequency / 0.01);

                // Ghi giá trị tần số
                WriteRegister(slaveId, ADDRESS_COMMAND_FREQUENCY, valueToWrite);

                // Đọc lại giá trị tần số để kiểm tra
                ushort[] readBack = ReadRegisters(slaveId, ADDRESS_COMMAND_FREQUENCY, 1);
                if (readBack == null || readBack.Length == 0)
                {
                    log.Error("Không đọc được thanh ghi tần số để xác nhận ghi.");
                    return false;
                }

                // So sánh giá trị đọc lại với giá trị ghi
                bool success = readBack[0] == valueToWrite;
                if (!success)
                {
                    log.Warn($"Giá trị tần số đọc lại 0x{readBack[0]:X4} khác với giá trị ghi 0x{valueToWrite:X4}");
                }
                else
                {
                    log.Info($"Ghi tần số thành công: {frequency} Hz (0x{valueToWrite:X4})");
                }
                return success;
            }
            catch (Exception ex)
            {
                log.Error("Lỗi khi ghi và xác nhận tần số: " + ex.Message);
                return false;
            }
        }
        public bool IsEmergencyStopActive(byte slaveId)
        {
            ushort[] registers = ReadRegisters(slaveId, ADDRESS_COMMAND_OPERATION, 1);
            if (registers == null || registers.Length == 0)
            {
                log.Error("Failed to read register for Emergency Stop status.");
                return false;
            }
            ushort value = registers[0];
            // Kiểm tra bit B4 (bit thứ 4, bắt đầu từ 0)
            bool isActive = (value & (1 << 4)) != 0;
            log.Info($"Emergency Stop bit status: {(isActive ? "ACTIVE" : "INACTIVE")}");
            return isActive;
        }
        public double GetInverterFrequency(byte slaveId)
        {
            ushort[] registers = ReadRegisters(slaveId, ADDRESS_COMMAND_FREQUENCY_OUT, 1);
            if (registers == null || registers.Length == 0)
            {
                log.Error("Không đọc được thanh ghi tần số đầu ra.");
                return -1; // Hoặc giá trị mặc định khác
            }
            // Chuyển đổi giá trị thanh ghi về tần số (0.01 Hz)
            double frequency = registers[0] * 0.01;
            log.Info($"Tần số đầu ra: {frequency} Hz");
            return frequency;
        }
        public void SaveInverterSerialPortInformation()
        {
            ClsIO.WriteValue("InverterSerialPort", "SerialCom", SerialCom, @"C:\Aavid_Test\Setup-ini\SerialPort.ini");
            ClsIO.WriteValue("InverterSerialPort", "Baudrate", Baudrate.ToString(), @"C:\Aavid_Test\Setup-ini\SerialPort.ini");
            ClsIO.WriteValue("InverterSerialPort", "DataBit", DataBit.ToString(), @"C:\Aavid_Test\Setup-ini\SerialPort.ini");
            ClsIO.WriteValue("InverterSerialPort", "Parity", Parity.ToString(), @"C:\Aavid_Test\Setup-ini\SerialPort.ini");
            ClsIO.WriteValue("InverterSerialPort", "StopBit", StopBit.ToString(), @"C:\Aavid_Test\Setup-ini\SerialPort.ini");
        }
    }
}
