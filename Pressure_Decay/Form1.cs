using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Routing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace Pressure_Decay
{
    public partial class Form1 : Form
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Cls_ASPcontrol));
        //2016.10.25 CDJ Main UI에 시스템 상태 표시
        public Thread thrSystemResource; //20150704 시스템 리소스 스레드
        private PerformanceCounter oCPU;// = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private PerformanceCounter oRAM;// = new PerformanceCounter("Memory", "Available MBytes");
        private PerformanceCounter oHardDiskC;// = new PerformanceCounter("LogicalDisk", "% Free Space", "C:");
        private bool bExit = false;
        private int iCPU = 0;
        private int iRAM = 0;
        private int iHardDiskC = 0;
        private int iHardDiskD = 0;
        private int iGDI = 0;
        Cls_ASPcontrol ASP_ControlPort;
        Cls_LS_R902 clsLSR902;
        private Font F = new Font("Arial", 9);
        private string sSetup_Folder = "C:\\Aavid_Test\\Setup-ini\\Flushing_Part_Numbers.txt";
        private string sFlushing_File = "";
        int coreCount = 6;
        public Cls_DBMsSQL ParameterDB = new Cls_DBMsSQL();
        public Cls_DBMsSQL ProductionDB = new Cls_DBMsSQL();
        public string AssyPN;
        public string WorkOder;
        Dictionary<string, string> myDict = new Dictionary<string, string>();
        private UC_PartNumberInfor uC_PartNumberInfors;
        private UC_PartNumberInfor.SecsionTest SecsionTest = UC_PartNumberInfor.SecsionTest.PressureDecay;
        private UC_DataSummary uC_DataSummary;
        private UC_SerialNumberHistory uC_SerialNumberHistory;
        private UC_MaintTest [] uC_MaintTests = new UC_MaintTest[3];
        private string[] Testmodes = new string[4] { "Production Test", "Production Retest", "Engineering Test", "Golden Sample Test" };
        private int[] Baudrates = { 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200 };
        private int[] DataBits = { 5, 6, 7, 8 };
        private Parity[] Paritys = { Parity.None, Parity.Odd, Parity.Even };
        private StopBits[] stopBits = { StopBits.One, StopBits.None, StopBits.OnePointFive, StopBits.Two };
        private Label[] relayLabels = new Label[30];
        private bool UpdateRelayTB = true;
        delegate void Deltact();
        delegate void DelResource();
        private bool AutoMode = Convert.ToBoolean(ClsIO.ReadValue("OPTIONS", "Fixtures_Control_Enabled", "True", @"C:\Aavid_Test\Setup-ini\LCS_Logging_Setup.ini"));
        private int iFixtureTest = 2;
        private System.Windows.Forms.TextBox[] HumidityShow = new System.Windows.Forms.TextBox[4];
        private System.Windows.Forms.TextBox[] TempShow = new System.Windows.Forms.TextBox[4];
        NestMenu nestMenu = NestMenu.None;
        private string Station;
        enum NestMenu
        {
            None,Nest1, Nest2, Nest3, Nest4,
        }
        private void GetDictionary()
        {

        }
        private string password;
        public Form1( string pass)
        {
            InitializeComponent();
            password =pass;
            ThreadPool.QueueUserWorkItem(o =>
            {
                FrmLoading frmLoading = new FrmLoading();
                frmLoading.StartPosition = FormStartPosition.CenterScreen;
                frmLoading.ShowDialog();
                frmLoading.Dispose();
            }
             );
            StateCommon.bLoading = true;
            StateCommon.LoadingText = "Loading...\r\n";
            StateCommon.LoadingText += "Setting thread update resuorce...\r\n";
            try
            {
                oCPU = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                oRAM = new PerformanceCounter("Memory", "Available MBytes");
                oHardDiskC = new PerformanceCounter("LogicalDisk", "% Free Space", "C:");
                thrSystemResource = new Thread(new ThreadStart(getSystemResouce));
                thrSystemResource.Name = "thrResource";
                thrSystemResource.IsBackground = true;
                thrSystemResource.Start();
            }
            catch
            { }
            
            StateCommon.LoadingText += "Setting thread update resuorce completed\r\n";
            StateCommon.LoadingValue = 10;
            StateCommon.LoadingText += "Create DataBase Connection...\r\n";
            ParameterDB.Initialize("10.102.4.20", "Parameters_SZ", "sa", "nuventixleo");
            ParameterDB.Open();
            ProductionDB.Initialize("10.102.4.20", "Production_SZ", "sa", "nuventixleo");
            ProductionDB.Open();
            StateCommon.LoadingText += "Create DataBase Connection completed\r\n";
            StateCommon.LoadingValue = 20;
            // 🟡 Gọi form Frm_ChangePartNumber và đợi kết quả
            StateCommon.LoadingText += "Setting Part Number ....\r\n";
            StateCommon.LoadingValue = 30;
            Frm_ChangePartNumber frmChange = new Frm_ChangePartNumber();
            frmChange.BringToFront();
            var result = frmChange.ShowDialog(); // chạy đồng bộ, chờ người dùng
            if (result == DialogResult.OK)
            {
                StateCommon.LoadingText += "Setting Part Number completed\r\n";
                StateCommon.LoadingText += "Config ASP Serial Port...\r\n";
                StateCommon.LoadingText += "GenerateComPort Compeleted\r\n ";
                ASP_ControlPort = new Cls_ASPcontrol();
                clsLSR902 = new Cls_LS_R902();
                clsLSR902.Open();
                clsLSR902.OnDataReceived += Device_OnDataReceived;
                Thread ThrCheckPortAlive = new Thread(new ThreadStart(ThreadCheckASPPortAlive));
                ThrCheckPortAlive.Name = "ThrCheckPortAlive";
                ThrCheckPortAlive.IsBackground = true;
                ThrCheckPortAlive.Start();
                ASP_ControlPort.OnRequestAddLog += (sender) =>
                {
                    if (InvokeRequired)
                        BeginInvoke(new MethodInvoker(() => funAddLog_Auto(sender)));
                    else
                        funAddLog_Auto(sender);
                };

                ASP_ControlPort.OnRequestUpdateRelayStatus += (sender) =>
                {
                    if (InvokeRequired)
                        BeginInvoke(new MethodInvoker(() => funUpdateRelayStatus_Auto(sender)));
                    else
                        funUpdateRelayStatus_Auto(sender);
                };
                ASP_ControlPort.OnRequestUpdateADC += (sender) =>
                {
                    if (InvokeRequired)
                        BeginInvoke(new MethodInvoker(() => funUpdateHumidityTemp_Auto(sender)));
                    else
                        funUpdateHumidityTemp_Auto(sender);
                };
                ASP_ControlPort.OnRequestUpdateSN_1 += (sender) =>
                {
                    if (InvokeRequired)
                        BeginInvoke(new MethodInvoker(() => funAddSerialSN1_Auto(sender)));
                    else funAddSerialSN1_Auto(sender);
                };
                ASP_ControlPort.OnRequestUpdateSN_2 += (sender) =>
                {
                    if (InvokeRequired)
                        BeginInvoke(new MethodInvoker(() => funAddSerialSN2_Auto(sender)));
                    else funAddSerialSN2_Auto(sender);
                };
                ASP_ControlPort.OnRequestUpdateSN_3 += (sender) =>
                {
                    if (InvokeRequired)
                        BeginInvoke(new MethodInvoker(() => funAddSerialSN3_Auto(sender)));
                    else funAddSerialSN3_Auto(sender);
                };
                ASP_ControlPort.OnRequestUpdateSN_4 += (sender) =>
                {
                    if (InvokeRequired)
                        BeginInvoke(new MethodInvoker(() => funAddSerialSN4_Auto(sender)));
                    else funAddSerialSN4_Auto(sender);
                };
                ASP_ControlPort.OnRequestUpdateStatusInput += (sender) =>
                {
                    if (InvokeRequired)
                        BeginInvoke(new MethodInvoker(() => funUpdateInputStatus_Auto(sender)));
                    else funUpdateInputStatus_Auto(sender);
                };
                StateCommon.LoadingText += "Config ASP Serial Port completed\r\n";
                StateCommon.LoadingValue = 40;
                StateCommon.LoadingText += "Config All unit for test...\r\n";
                InitializeUnitManager(ASP_ControlPort);
                StateCommon.LoadingText += "Config All unit for test completed\r\n";
                StateCommon.LoadingValue = 50;
                StateCommon.LoadingText += "Initial Relay Control Board...\r\n";
                InitRelayLabels();
                StateCommon.LoadingText += "Initial Relay Control Board completed\r\n";
                StateCommon.LoadingValue = 60;
                StateCommon.LoadingText += "Change all mode to Auto mode...\r\n";
                ClsUnitManagercs.cls_Units.cls_SequencyCommon.AutoMode = AutoMode;
                if(AutoMode)
                    btn_ChangeMode.Text = "AUTO";
                else
                    btn_ChangeMode.Text = "MANUAL";
                StateCommon.LoadingText += "Change all mode to Auto mode completed\r\n";
                StateCommon.LoadingValue = 60;
                StateCommon.LoadingText += "Change nest selected to Nest1 ....\r\n";
                MenuNest_Change(NestMenu.Nest1);
                StateCommon.LoadingText += "Change nest selected to Nest1 completed\r\n";
                StateCommon.LoadingValue = 70;
                StateCommon.LoadingText += "Create Data Summary Table ....\r\n";
                uC_DataSummary = new UC_DataSummary(ClsUnitManagercs.cls_Units.AssyPN, "DI Water Wash");
                uC_DataSummary.Dock = DockStyle.Fill;
                StateCommon.LoadingText += "Create Data Summary Table completed\r\n";
                StateCommon.LoadingValue = 80;
                StateCommon.LoadingText += "Create Data History Query ....\r\n";
                uC_SerialNumberHistory = new UC_SerialNumberHistory(ClsUnitManagercs.cls_Units.AssyPN);
                uC_SerialNumberHistory.Dock = DockStyle.Fill;
                StateCommon.LoadingText += "Create Data History Query completed\r\n";
                StateCommon.LoadingValue = 80;
                StateCommon.LoadingText += "Set All Relay to turn OFF\r\n";
                SetAllRelayOFF();
                timer1.Start();
                timer3.Start();
                StateCommon.LoadingText += "Complete\r\n";
                StateCommon.LoadingValue = 100;
                StateCommon.bLoading = false;
            }
            else
            {
                System.Windows.Forms.Application.Exit();
            }
        }

        private void funUpdateInputStatus_Auto(Cls_ASPcontrol sender)
        {
            bool[] input = sender.inputStates;
            ChangeToggle(Tbn_QDLeft, input[10]);
            ChangeToggle(Tbn_QDRight, input[11]);
            ChangeToggle(Tbn_Start , input[0]);
            ChangeToggle(Tbn_Abort , input[1]);
        }

        private void funAddSerialSN1_Auto(string sender)
        {
            if(txt_Scanner1.InvokeRequired)
                txt_Scanner1.Invoke(new Action(() => txt_Scanner1.Text = sender));
            else
                txt_Scanner1.Text = sender;
        }
        private void funAddSerialSN2_Auto(string sender)
        {
            if (txt_Scanner2.InvokeRequired)
                txt_Scanner2.Invoke(new Action(() => txt_Scanner2.Text = sender));
            else
                txt_Scanner2.Text = sender;
        }
        private void funAddSerialSN3_Auto(string sender)
        {
            if (txt_Scanner3.InvokeRequired)
                txt_Scanner3.Invoke(new Action(() => txt_Scanner3.Text = sender));
            else
                txt_Scanner3.Text = sender;
        }
        private void funAddSerialSN4_Auto(string sender)
        {
            if (txt_Scanner4.InvokeRequired)
                txt_Scanner4.Invoke(new Action(() => txt_Scanner4.Text = sender));
            else
                txt_Scanner4.Text = sender;
        }
        private void funUpdateHumidityTemp_Auto(Cls_ASPcontrol sender)
        {
            for(int i = 0; i < HumidityShow.Length; i++)
            {
                if (!HumidityShow[i].Created || !TempShow[i].Created)
                    return;
                if (HumidityShow[i].InvokeRequired || TempShow[i].InvokeRequired)
                {
                    HumidityShow[i].BeginInvoke(new Action(() => HumidityShow[i].Text = sender.Humiditys[i].ToString()));
                    TempShow[i].BeginInvoke(new Action(() => TempShow[i].Text = sender.Temps[i].ToString()));
                }
                else
                {
                    HumidityShow[i].Text = sender.Humiditys[i].ToString();
                    TempShow[i].Text = sender.Temps[i].ToString();
                }
            }
        }
        
        private void ThreadCheckASPPortAlive()
        {
            Thread.Sleep(5000);
            while(true)
            {
                if (ASP_ControlPort.isConnected)
                {
                    try
                    {
                        string[] PortAlive = SerialPort.GetPortNames();
                        if (!PortAlive.Contains(ASP_ControlPort.SerialCom))   
                        {
                            ASP_ControlPort.isConnected = false;
                        }
                    }
                    catch { }
                }
                Thread.Sleep(100);
            }    
        }
        private void InitRelayLabels()
        {
            tbl_RelayControl.Controls.Clear();
            tbl_RelayControl.ColumnCount = 6;
            tbl_RelayControl.RowCount = 5;
            tbl_RelayControl.ColumnStyles.Clear();
            tbl_RelayControl.RowStyles.Clear();

            for (int i = 0; i < 6; i++)
                tbl_RelayControl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 6));
            for (int i = 0; i < 5; i++)
                tbl_RelayControl.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 5));

            for (int i = 0; i < 30; i++)
            {
                int index = i; // lưu lại để dùng trong event handler
                Label lbl = new Label();
                lbl.Text = $"R{index + 1}: OFF";
                lbl.TextAlign = ContentAlignment.MiddleCenter;
                lbl.Dock = DockStyle.Fill;
                lbl.BorderStyle = BorderStyle.FixedSingle;
                lbl.BackColor = Color.LightGray;
                lbl.Cursor = Cursors.Hand;

                lbl.Click += (s, e) => ToggleRelay(index);

                relayLabels[i] = lbl;
                int row = i / 6;
                int col = i % 6;
                tbl_RelayControl.Controls.Add(lbl, col, row);
            }
        }
        private async void ToggleRelay(int index)
        {
            bool currentStatus = ASP_ControlPort.relayStates[index];
            bool newStatus = !currentStatus;
            try
            {
                await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(index + 1, newStatus ? 1 : 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception when send command to ASPPort: {ex.Message}");
            }
        }
        private void GenerateComPort()
        {
            try
            {
                this.cbb_TestMode.Items.AddRange(Testmodes);
                this.cbb_TestMode.SelectedIndex = 0;
                this.cbbASPComPort.Items.AddRange(SerialPort.GetPortNames());
                this.cbbASPBaudrate.Items.AddRange(Baudrates.Cast<object>().ToArray());
                this.cbbASPDataBit.Items.AddRange(DataBits.Cast<object>().ToArray());
                this.cbbASPParity.Items.AddRange(Paritys.Cast<object>().ToArray());
                this.cbbASPStopBit.Items.AddRange(stopBits.Cast<object>().ToArray());
                this.cbb_LSR902ComPort.Items.AddRange(SerialPort.GetPortNames());
                this.cbb_LSR902Baudrate.Items.AddRange(Baudrates.Cast<object>().ToArray());
                this.cbb_LSR902Databit.Items.AddRange(DataBits.Cast<object>().ToArray());
                this.cbb_LSR902Parity.Items.AddRange(Paritys.Cast<object>().ToArray());
                this.cbb_LSR902Stopbit.Items.AddRange(stopBits.Cast<object>().ToArray());
                this.cbb_LSR902Outputformat.Items.AddRange(Enum.GetValues(typeof(Cls_LS_R902.OutputFormat)).Cast<object>().ToArray());
                string SerialCom = ASP_ControlPort.SerialCom;
                int Baudrate = ASP_ControlPort.Baudrate;
                int DataBit = ASP_ControlPort.DataBit;
                Parity Parity = ASP_ControlPort.Parity;
                StopBits StopBit = ASP_ControlPort.StopBit;
                cbbASPComPort.SelectedItem = SerialCom;
                cbbASPBaudrate.SelectedItem = Baudrate;
                cbbASPDataBit.SelectedItem = DataBit;
                cbbASPParity.SelectedItem = Parity;
                cbbASPStopBit.SelectedItem = StopBit;
                SerialCom = clsLSR902.PortName;
                Baudrate = clsLSR902.BaudRate;
                DataBit = clsLSR902.DataBits;
                Parity = clsLSR902.Parity;
                StopBit = clsLSR902.StopBits;
                Cls_LS_R902.OutputFormat outputFormat = clsLSR902.CurrentOutputFormat;
                cbb_LSR902Outputformat.SelectedItem = outputFormat;
                cbb_LSR902ComPort.SelectedItem = SerialCom;
                cbb_LSR902Baudrate.SelectedItem = Baudrate;
                cbb_LSR902Databit.SelectedItem = DataBit;
                cbb_LSR902Parity.SelectedItem = Parity;
                cbb_LSR902Stopbit.SelectedItem = StopBit;
                cbb_TestMode.SelectedItem = 0;
            }
            catch (Exception ex )
            {
                log.Error( ex );
                MessageBox.Show("Error when generate Com Port: " + ex.Message);
            }
        }

        private void InitializeUnitManager( Cls_ASPcontrol cls_AS)
        {
            string[] array = File.ReadAllLines("C:\\Aavid_Test\\Setup-ini\\Flushing_Part_Numbers.txt");
            string[] array2 = array[1].Split(',');
            AssyPN = array2[1].Trim();
            WorkOder = array2[2].Trim();
            string stationID = ClsIO.ReadValue("LOGGING_DATA", "Tester_ID", "", "C:\\Aavid_Test\\Setup-ini\\LCS_Logging_Setup.ini").Trim().Trim('"');
            if (stationID.Length == 0)
            {
                using (var stationForm = new Frm_ShowInputStation())
                {
                    var result = stationForm.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        stationID = stationForm.EnteredStation;
                    }
                }
                ClsIO.WriteValue("LOGGING_DATA", "Tester_ID", stationID, "C:\\Aavid_Test\\Setup-ini\\LCS_Logging_Setup.ini");
            }
            ClsUnitManagercs.Initialize(AssyPN, WorkOder, stationID, cls_AS, clsLSR902);
            //GenerateProcess();
            Panel[] pl_main = new Panel[1] { pl_Main1 };
            uC_PartNumberInfors = new UC_PartNumberInfor(0, SecsionTest);
            uC_PartNumberInfors.InitUI();
            uC_PartNumberInfors.Dock = DockStyle.Fill;
            for (int i = 0; i < pl_main.Length; i++)
            {
                uC_MaintTests[i] = new UC_MaintTest(i);
                uC_MaintTests[i].Dock = DockStyle.Fill;
                pl_main[i].Controls.Add(uC_MaintTests[i]);
            }
            ClsUnitManagercs.cls_Units.cls_SequencyCommon.AutoMode = true;
            ClsUnitManagercs.cls_Units.cls_SequencyCommon.OnRequestUpdateStage += delegate (Cls_SequencyCommon sender)
            {
                if (base.InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        funUpdateStage_Auto(sender);
                    });
                }
                else
                {
                    funUpdateStage_Auto(sender);
                }
            };
            ClsUnitManagercs.cls_Units.cls_SequencyTest.TestType = cbb_TestMode.Text;

            ClsUnitManagercs.cls_Units.cls_SequencyTest.OnRequestAddProcessLogTest += (s, e) =>
            {
                int fromUnit = e.UnitIndex;
                DataTable data = e.LogData;
                UpdateChildData(fromUnit, data);
            };
        }
        public void UpdateChildData(int index, DataTable dt)
        {
            if (!uC_MaintTests[index].Created) return;
            uC_MaintTests[index].UpdateData(dt);
        }
        public void getSystemResouce()
        {
            getPhysicalMemory();
            while (!bExit)
            {
                oCPU = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                oRAM = new PerformanceCounter("Memory", "Available MBytes");
                oHardDiskC = new PerformanceCounter("LogicalDisk", "% Free Space", "C:");
                int[] _CPU = new int[coreCount];
                for (int i = 0; i < coreCount; i++)
                {
                    Thread.Sleep(500);
                    _CPU[i] = (int)oCPU.NextValue();
                }
                iCPU = GetMaxCore(_CPU);
                float availableMemory = oRAM.NextValue();
                float usedMemory = (float)maxMem - availableMemory;
                float _RAM = (usedMemory / (float)maxMem) * 100;
                iRAM = (int)_RAM;
                int _C = 100 - (int)oHardDiskC.NextValue();
                iHardDiskC = _C;
                DelResource a = new DelResource(funResource);
                if (progressBar_CPU.IsHandleCreated)
                    progressBar_CPU.BeginInvoke(a);
            }
        }
        private int GetMaxCore(int[] _iCPU)
        {
            int maxCore = 0;
            for (int i = 0; i < _iCPU.Length; i++)
            {
                maxCore = Math.Max(maxCore, _iCPU[i]);
            }
            return maxCore;
        }
        private void funResource()
        {
            int _cpu = 0;
            int _ram = 0;
            int _c = 0;
            int _d = 0;

            _cpu = iCPU;
            //Console.WriteLine("CPU: " + iCPU);

            _ram = iRAM;
            _c = iHardDiskC;
            _d = iHardDiskD;

            if (_cpu > 100)
                _cpu = 100;
            if (_ram > 100)
                _ram = 100;
            if (_c > 100)
                _c = 100;
            if (_d > 100)
                _d = 100;

            progressBar_CPU.Value = _cpu;
            lb_CPU.Text = _cpu.ToString() + "%";
            progressBar_MEM.Value = _ram;
            lb_MEM.Text = _ram.ToString() + "%";
            progressBar_C.Value = _c;
            lb_DiskC.Text = _c.ToString() + "%";
        }
        int maxMem = 0;
        private void getPhysicalMemory()
        {
            System.Management.ObjectQuery winQuery = new System.Management.ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(winQuery);
            ManagementObjectCollection queryCollection = searcher.Get();

            ulong memory = 0;
            foreach (ManagementObject item in queryCollection)
            {
                memory = ulong.Parse(item["TotalVisibleMemorySize"].ToString());
            }
            maxMem = (int)(memory / 1024);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime dtNow = DateTime.Now;
            this.lb_Date.Text = dtNow.ToString("yyyy-MM-dd HH:mm:ss");

        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            DateTime dtNow = DateTime.Now;
            this.lb_Date.Text = dtNow.ToString("yyyy-MM-dd HH:mm:ss");

        }
        private void UpdateRelayStatus(bool[] relays)
        {
            bool hasNull = relayLabels != null && relayLabels.Any(label => label == null);
            if (hasNull) return;
            for (int i = 0; i < 30; i++)
            {
                relayLabels[i].Text = $"R{i + 1}: {(relays[i] ? "ON" : "OFF")}";
                relayLabels[i].BackColor = relays[i] ? Color.LimeGreen : Color.LightGray;
                relayLabels[i].ForeColor = relays[i] ? Color.White : Color.Black;
            }
            void UpdateColor(Label label, Color color)
            {
                if (label.InvokeRequired)
                    label.Invoke(new Action(() => label.BackColor = color));
                else
                    label.BackColor = color;
            }

            switch (nestMenu)
            {
                case NestMenu.Nest1:
                    UpdateColor(lb_Red, relays[8] ? Color.Red : Color.Gray);
                    UpdateColor(lb_Yellow, relays[9] ? Color.Yellow : Color.Gray);
                    UpdateColor(lb_Green, relays[10] ? Color.Green : Color.Gray);
                    UpdateColor(lb_Buzzer, relays[11] ? Color.Blue : Color.Gray);
                    break;

                case NestMenu.Nest2:
                    UpdateColor(lb_Red, relays[12] ? Color.Red : Color.Gray);
                    UpdateColor(lb_Yellow, relays[13] ? Color.Yellow : Color.Gray);
                    UpdateColor(lb_Green, relays[14] ? Color.Green : Color.Gray);
                    UpdateColor(lb_Buzzer, relays[15] ? Color.Blue : Color.Gray);
                    break;

                case NestMenu.Nest3:
                    UpdateColor(lb_Red, relays[16] ? Color.Red : Color.Gray);
                    UpdateColor(lb_Yellow, relays[17] ? Color.Yellow : Color.Gray);
                    UpdateColor(lb_Green, relays[18] ? Color.Green : Color.Gray);
                    UpdateColor(lb_Buzzer, relays[19] ? Color.Blue : Color.Gray);
                    break;
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            lb_Ver.Text = "Ver: "+version;
        }
        private void GenerateProcess()
        {
            DataGridView[] dgv = new DataGridView[1];
            for (int i = 0; i < dgv.Length; i++)
            {
                // Xóa tất cả cột và dòng cũ
                dgv[i].Columns.Clear();
                dgv[i].Rows.Clear();
                dgv[i].ReadOnly = true;                      // Không cho phép sửa
                dgv[i].ClearSelection();                     // Bỏ chọn dòng mặc định
                dgv[i].DefaultCellStyle.SelectionBackColor = dgv[i].DefaultCellStyle.BackColor;
                dgv[i].DefaultCellStyle.SelectionForeColor = dgv[i].DefaultCellStyle.ForeColor;
                dgv[i].RowHeadersVisible = false;
                dgv[i].MultiSelect = false;
                dgv[i].Enabled = true;                       // Vẫn scroll được
                // Tạo một cột duy nhất để hiển thị giá trị
                DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                col.HeaderText = "Process";                  // Không hiện tiêu đề
                col.Name = "Process";
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv[i].Columns.Add(col);
                for (int index = 0; index < ClsUnitManagercs.cls_Units.verifyFlags.Length; index++)
                {
                    dgv[i].Rows.Add(ClsUnitManagercs.cls_Units.verifyFlags[index]);
                }
            }
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            GenerateComPort();
            if (ASP_ControlPort.isConnected)
            {
                this.lb_ASPConnected.Text = "Connected";
                this.lb_ASPConnected.BackColor = Color.Green;
            }
            else
            {
                this.lb_ASPConnected.Text = "Disconnected";
                this.lb_ASPConnected.BackColor = Color.Red;
            }
            if(clsLSR902.IsConnected)
            {
                this.lb_LSR902Connected.Text = "Connected";
                this.lb_LSR902Connected.BackColor = Color.Green;
            }
            else
            {
                this.lb_LSR902Connected.Text = "Disconnected";
                this.lb_LSR902Connected.BackColor = Color.Red;
            }
            lb_StationID.Text = ClsUnitManagercs.cls_Units.StaionID;
            AutoMode = true;
            DisableAllControl();
            btn_ChangeMode.Text = "AUTO";
            ClsUnitManagercs.cls_Units.cls_SequencyCommon.AutoMode = true;
            ClsUnitManagercs.cls_Units.cls_SequencyCommon.AutoMode = true;
        }
        
        private void ChangeToggle(ToggleButton toggleButton, bool isOn)
        {
            if (toggleButton == null)
            {
                return; // Nếu toggleButton là null, không làm gì cả
            }
            if (toggleButton.Created == false)
            {
                return; // Nếu toggleButton chưa được tạo, không làm gì cả
            }
            if (toggleButton.InvokeRequired)
            {
                toggleButton.Invoke(new Action<ToggleButton, bool>(ChangeToggle), toggleButton, isOn);
            }
            else
            {
                toggleButton.Checked = isOn;
            }
        }
        private void btn_SaveASPPort_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Do you want to save configuration for ASP control Serial Port?","Notification", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                ASP_ControlPort.SerialCom = cbbASPComPort.Text;
                ASP_ControlPort.Baudrate = int.Parse(cbbASPBaudrate.Text);
                ASP_ControlPort.DataBit = int.Parse(cbbASPDataBit.Text);
                ASP_ControlPort.Parity = (Parity)Enum.Parse(typeof(Parity), cbbASPParity.Text);
                ASP_ControlPort.StopBit = (StopBits)Enum.Parse(typeof(StopBits), cbbASPStopBit.Text);
                ASP_ControlPort.SaveASPSerialPortInformation();
                log.Error($"Save ASP Serial Port Information: {ASP_ControlPort.SerialCom} {ASP_ControlPort.Baudrate} {ASP_ControlPort.DataBit} {ASP_ControlPort.Parity} {ASP_ControlPort.StopBit}");
                ASP_ControlPort.DisconnectASPSerial();
                ASP_ControlPort.ConnectASPSerial();
            }
            if (ASP_ControlPort.isConnected)
            {
                this.lb_ASPConnected.Text = "Connected";
                this.lb_ASPConnected.BackColor = Color.Green;
            }
            else
            {
                this.lb_ASPConnected.Text = "Disconnected";
                this.lb_ASPConnected.BackColor = Color.Red;
            }
        }
        public void funAddLog_Auto(string _text)
        {
            if (!rtb_ASPPortBuffer.Created) return;
            if (rtb_ASPPortBuffer.InvokeRequired)
            {
                rtb_ASPPortBuffer.BeginInvoke(new MethodInvoker(delegate {
                    rtb_ASPPortBuffer.AppendText(_text + "\n");
                    rtb_ASPPortBuffer.ScrollToCaret();
                }));
            }
            else
            {
                rtb_ASPPortBuffer.AppendText(_text + "\n");
                rtb_ASPPortBuffer.ScrollToCaret();
            }
        }

        public void funUpdateRelayStatus_Auto(Cls_ASPcontrol unit)
        {
            UpdateRelayStatus(ASP_ControlPort.relayStates); // Sau đó mới cập nhật UI
        }
        public void funUpdateStage_Auto(Cls_SequencyCommon unit)
        {
            switch (nestMenu) 
            {
                case NestMenu.Nest1:
                    lb_StageStatus.Text = ClsUnitManagercs.cls_Units.cls_SequencyCommon.process.ToString();
                    break;
                case NestMenu.Nest2:
                    lb_StageStatus.Text = ClsUnitManagercs.cls_Units.cls_SequencyCommon.process.ToString();
                    break;
                case NestMenu.Nest3:
                    lb_StageStatus.Text = ClsUnitManagercs.cls_Units.cls_SequencyCommon.process.ToString();
                    break;
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await ASP_ControlPort.GetAllRelay(); // Chờ nhận xong dữ liệu
        }

        private void tableLayoutPanel15_Paint(object sender, PaintEventArgs e)
        {

        }
        private void MenuNest_Change(NestMenu menu)
        {
            if (nestMenu == menu)
                return;
            nestMenu = menu;
            switch (menu)
            {
                case NestMenu.Nest1:
                    RefeshAllNestlb();
                    lb_Nest1.BackColor = Color.LimeGreen;
                    break;
                case NestMenu.Nest2:
                    RefeshAllNestlb();
                    lb_Nest2.BackColor = Color.LimeGreen;
                    break;
            }
            ASP_ControlPort.GetAllRelay();
        }
        private void RefeshAllNestlb()
        {
            lb_Nest1.BackColor = Color.Gray;
            lb_Nest2.BackColor = Color.Gray;
        }
        private void lb_Nest1_Click(object sender, EventArgs e)
        {
            MenuNest_Change(NestMenu.Nest1);
        }

        private void lb_Nest2_Click(object sender, EventArgs e)
        {
            MenuNest_Change(NestMenu.Nest2);
        }

        private void lb_Nest3_Click(object sender, EventArgs e)
        {
            MenuNest_Change(NestMenu.Nest3);
        }

        private void rtb_DetailRelay_TextChanged(object sender, EventArgs e)
        {

        }
        private async void lb_Red_Click(object sender, EventArgs e)
        {
            bool[] relays = ASP_ControlPort.relayStates;
            switch (nestMenu)
            {
                case NestMenu.Nest1:
                    if(relays[8])
                       await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(9, 0);
                    else
                       await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(9, 1);
                    break;
                case NestMenu.Nest2:
                    if (relays[12])
                        await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(13, 0);
                    else
                        await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(13, 1);
                    break;;
                case NestMenu.Nest3:
                    if (relays[16])
                        await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(17, 0);
                    else
                        await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(17, 1);
                    break;
            }
            //UpdateLEDStatus();
        }

        private async void lb_Yellow_Click(object sender, EventArgs e)
        {
            bool[] relays = ASP_ControlPort.relayStates;
            switch (nestMenu)
            {
                case NestMenu.Nest1:
                    if (relays[9])
                        await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(10, 0);
                    else
                        await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(10, 1);
                    break;
                case NestMenu.Nest2:
                    if (relays[13])
                        await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(14, 0);
                    else
                        await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(14, 1);
                    break; ;
                case NestMenu.Nest3:
                    if (relays[17])
                        await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(18, 0);
                    else
                        await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(18, 1);
                    break;
            }
            //UpdateLEDStatus();
            //UpdateRelayStatus(ASP_ControlPort.relayStates); // Sau đó mới cập nhật UI
        }
        private async void lb_Green_Click(object sender, EventArgs e)
        {
            bool[] relays = ASP_ControlPort.relayStates;
            switch (nestMenu)
            {
                case NestMenu.Nest1:
                    if (relays[10])
                        await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(11, 0);
                    else
                        await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(11, 1);
                    break;
                case NestMenu.Nest2:
                    if (relays[14])
                        await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(15, 0);
                    else
                        await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(15, 1);
                    break; 
                case NestMenu.Nest3:
                    if (relays[18])
                        await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(19, 0);
                    else
                        await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(19, 1);
                    break;
            }
            //UpdateLEDStatus();
            //UpdateRelayStatus(ASP_ControlPort.relayStates); // Sau đó mới cập nhật UI
        }
        private async void lb_Buzzer_Click(object sender, EventArgs e)
        {
            bool[] relays = ASP_ControlPort.relayStates;
            switch (nestMenu)
            {
                case NestMenu.Nest1:
                    if (relays[11])
                        await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(12, 0);
                    else
                        await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(12, 1);
                    break;
                case NestMenu.Nest2:
                    if (relays[15])
                        await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(16, 0);
                    else
                        await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(16, 1);
                    break; ;
                case NestMenu.Nest3:
                    if (relays[19])
                        await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(20, 0);
                    else
                        await ASP_ControlPort.SetRelayONOFFAsyncCheckResult(20, 1);
                    break;
            }
            //UpdateLEDStatus();
            //UpdateRelayStatus(ASP_ControlPort.relayStates); // Sau đó mới cập nhật UI
        }
        private void DisableAllControl()
        {
            tbl_RelayControl.Enabled = false;
            tableLayoutPanel15.Enabled = false;
            lb_Buzzer.Enabled = false;
            lb_Green.Enabled = false;
            lb_Yellow.Enabled = false;
            lb_Red.Enabled = false;
        }
        private void EnableAllControl()
        {
            tbl_RelayControl.Enabled = true;
            tableLayoutPanel15.Enabled = true;
            lb_Buzzer.Enabled = true;
            lb_Green.Enabled = true;
            lb_Yellow.Enabled = true;
            lb_Red.Enabled = true;
        }
        private void btn_ChangeMode_Click(object sender, EventArgs e)
        {
            if(!AutoMode)
            {
                AutoMode = true;
                DisableAllControl();
                btn_ChangeMode.Text = "AUTO";
                ClsUnitManagercs.cls_Units.cls_SequencyCommon.AutoMode = true;
                ClsUnitManagercs.cls_Units.cls_SequencyCommon.AutoMode = true;
            }
            else
            {
                AutoMode = false;
                EnableAllControl();
                btn_ChangeMode.Text = "MANUAL";
                ClsUnitManagercs.cls_Units.cls_SequencyCommon.AutoMode = false;
                ClsUnitManagercs.cls_Units.cls_SequencyCommon.AutoMode = false;
            }    
        }
        private void Tbtn_Pump_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Tbtn_3WaySwitch_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Tbtn_ReverserFlow_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void btn_InverterSave_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Do you want to save configuration for LSR902 Serial Port?", "Notification", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                clsLSR902.Close();
                clsLSR902.PortName = cbb_LSR902ComPort.Text;
                clsLSR902.BaudRate = int.Parse(cbb_LSR902Baudrate.Text);
                clsLSR902.DataBits = int.Parse(cbb_LSR902Databit.Text);
                clsLSR902.Parity = (Parity)Enum.Parse(typeof(Parity), cbb_LSR902Parity.Text);
                clsLSR902.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cbb_LSR902Stopbit.Text);
                clsLSR902.SaveLSR902SerialPortInformation();
                log.Error($"Save Inverter Serial Port Information: {clsLSR902.PortName} {clsLSR902.BaudRate} {clsLSR902.DataBits} {clsLSR902.Parity} {clsLSR902.StopBits}");
                clsLSR902.Open();
            }
            if (clsLSR902.IsConnected)
            {
                this.lb_LSR902Connected.Text = "Connected";
                this.lb_LSR902Connected.BackColor = Color.Green;
            }
            else
            {
                this.lb_LSR902Connected.Text = "Disconnected";
                this.lb_LSR902Connected.BackColor = Color.Red;
            }
        }

        private void txt_Fixture_KeyDown(object sender, KeyEventArgs e)
        {
            
        }
        private int previousTestModeIndex = -1;
        private void cbb_TestMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cbb_TestMode.SelectedIndex == 2)
            {
                using (var passwordForm = new Frm_PasswordInput())
                {
                    var result = passwordForm.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        string Inputpassword = passwordForm.EnteredPassword;
                        if (Inputpassword != password)
                        {
                            MessageBox.Show("Sai mật khẩu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            // Quay lại giá trị cũ
                            cbb_TestMode.SelectedIndex = previousTestModeIndex;
                            return;
                        }
                    }
                    else
                    {
                        cbb_TestMode.SelectedIndex = previousTestModeIndex;
                        return;
                    }    
                }
            }    
            ClsUnitManagercs.cls_Units.cls_SequencyTest.TestType = cbb_TestMode.Text;
            // Cập nhật chỉ số đã chọn gần nhất
            previousTestModeIndex = cbb_TestMode.SelectedIndex;
        }
        private async void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_isClosingHandled)
            {
                e.Cancel = true; // Chặn form đóng ngay
                _isClosingHandled = true;

                // Optional: Hiện thông báo hoặc loading
                this.Enabled = false; // Khoá UI nếu cần

                await ASP_ControlPort.SetAllRelayOFF();

                this.Enabled = true;
                this.Close(); // Đóng form lại sau khi hoàn tất
            }
        }
        private bool _isClosingHandled = false;
        private async void SetAllRelayOFF()
        {
            await ASP_ControlPort.SetAllRelayOFF();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            //ClsUnitManagercs.cls_Units[1].cls_SequencyCommon.process = StateCommon.ProcessState.Running;
            //ClsUnitManagercs.cls_Units[1].cls_SequencyTest.testSeq = Cls_SequencyTest.TestSeq.TEST_PASS;
        }
        private void timer3_Tick(object sender, EventArgs e)
        {
            UpdateTodayHistory();
        }
        private void UpdateTodayHistory()
        {
            try
            {
                string StationID = ClsUnitManagercs.cls_Units.StaionID;
                DateTime dt1 = DateTime.Today;           // hôm nay lúc 00:00:00
                DateTime dt2 = DateTime.Now;             // thời gian hiện tại
                string cmd = $@"SELECT [Station]
                                   ,[FailCode]
                                   ,[Date_Time]
                                   FROM [Production_SZ].[dbo].[LCS_Pressure_and_Sniffing_Test_Log] 
                                   WHERE Station ='{StationID}' 
                                   AND Date_Time BETWEEN '{dt1}' AND '{dt2}'";
                if (ProductionDB == null)
                    return;
                DataTable dt = ProductionDB.ExecuteQuery(cmd);
                int passed = 0;
                int failed = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["FailCode"].ToString().ToUpper().Trim() == "PASS" || dr["FailCode"].ToString().ToUpper().Trim() == "OK")
                    {
                        passed++;
                    }
                    else
                    {
                        failed++;
                    }
                }
                txt_Passed.Text = passed.ToString();
                txt_Failed.Text = failed.ToString();
                double total = passed + failed;
                double passRate = total > 0 ? (passed / total) * 100 : 0;
                txt_PassRate.Text = $"{passRate}"; // Hiển thị tỷ lệ pass với 2 chữ số thập phân
                txt_Total.Text = total.ToString();
            }
            catch
            {

            }
        }
        private void label45_Click(object sender, EventArgs e)
        {

        }
        private async void Scan_Serial_NumberRequest(int Index)
        {
            try
            {
                if (ASP_ControlPort == null) return;
                bool result = await ASP_ControlPort.RequestTriggerScanner(Index);
                if (!result)
                    MessageBox.Show("Failed to read SN by Scanner: " + Index);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when Scan SN: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Device_OnDataReceived(string result)
        {
            this.Invoke(new Action(() =>
            {
                rtb_LSR902Log.AppendText(result + "\r\n");
                rtb_LSR902Log.ScrollToCaret();
            }));
        }

        private void btn_Scan1_Click(object sender, EventArgs e)
        {
            Scan_Serial_NumberRequest(1);
        }

        private void btn_Scan2_Click(object sender, EventArgs e)
        {
            Scan_Serial_NumberRequest(2);
        }

        private void btn_Scan3_Click(object sender, EventArgs e)
        {
            Scan_Serial_NumberRequest(3);
        }

        private void btn_Scan4_Click(object sender, EventArgs e)
        {
            Scan_Serial_NumberRequest(4);
        }
        private bool OnProcessFixture = false;
        private async void Tbtn_OpenCloseFixture1_CheckedChanged(object sender, EventArgs e)
        {
            if (OnProcessFixture) return;
            OnProcessFixture = true;
            if (Tbtn_OpenCloseFixture1.Checked)
            {
                bool result = await ClsUnitManagercs.cls_Units.cls_SequencyTest.OpenFixture_1();
                if (result == false)
                    MessageBox.Show("Open fixture failed");
            }
            else
            {
                bool result = await ClsUnitManagercs.cls_Units.cls_SequencyTest.OpenFixture_1();
                if (result == false)
                    MessageBox.Show("Close fixture failed");
            }
            OnProcessFixture = false;
        }

        private async void Tbtn_OpenCloseFixture2_CheckedChanged(object sender, EventArgs e)
        {
            if (OnProcessFixture) return;
            OnProcessFixture = true;
            if (Tbtn_OpenCloseFixture2.Checked)
            {
                bool result = await ClsUnitManagercs.cls_Units.cls_SequencyTest.OpenFixture_2();
                if (result == false)
                    MessageBox.Show("Open fixture failed");
            }
            else
            {
                bool result = await ClsUnitManagercs.cls_Units.cls_SequencyTest.OpenFixture_2();
                if (result == false)
                    MessageBox.Show("Close fixture failed");
            }
            OnProcessFixture = false;
        }
    }
}
