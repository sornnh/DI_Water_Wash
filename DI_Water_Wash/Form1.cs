using BoyD_Oven_Monitoring;
using DI_Water_Wash;
using DI_Water_Wash.DataSummary;
using DI_Water_Wash.ParameterInitial;
using DI_Water_Wash.Sequence;
using DI_Water_Wash.Unit;
using LinkPNReplace;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;



namespace DI_Water_Wash
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
        ClsInverterModbus clsInverterModbus;
        private Font F = new Font("Arial", 9);
        private string sSetup_Folder = "C:\\Aavid_Test\\Setup-ini\\Flushing_Part_Numbers.txt";
        private string sFlushing_File = "";
        int coreCount = 6;
        public Cls_DBMsSQL ParameterDB = new Cls_DBMsSQL();
        public Cls_DBMsSQL ProductionDB = new Cls_DBMsSQL();
        public string[] AssyPN;
        public string[] WorkOder;
        Dictionary<string, string> myDict = new Dictionary<string, string>();
        private UC_PartNumberInfor[] uC_PartNumberInfors ;
        private UC_PartNumberInfor.SecsionTest SecsionTest = UC_PartNumberInfor.SecsionTest.DIWaterWash;
        private UC_MaintTest [] uC_MaintTests;
        private string[] Testmodes = new string[4] { "Production Test", "Production Retest", "Engineering Test", "Golden Sample Test" };
        private int[] Baudrates = { 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200 };
        private int[] DataBits = { 5, 6, 7, 8 };
        private Parity[] Paritys = { Parity.None, Parity.Odd, Parity.Even };
        private StopBits[] stopBits = { StopBits.One, StopBits.None, StopBits.OnePointFive, StopBits.Two };
        private Label[] relayLabels = new Label[30];
        private bool UpdateRelayTB = true;
        delegate void Deltact();
        delegate void DelResource(); 
        private bool AutoMode = true;
        private System.Windows.Forms.TextBox[] ADVShow = new System.Windows.Forms.TextBox[4];
        private System.Windows.Forms.TextBox[] ADAShow = new System.Windows.Forms.TextBox[4];
        private System.Windows.Forms.TextBox[] ADCShow = new System.Windows.Forms.TextBox[8];
        private Panel[] pl_infor ;
        private Panel[] pl_main;
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
        public Form1(string pass)
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
            timer1.Start();
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
                clsInverterModbus = new ClsInverterModbus();
                Thread ThrCheckPortAlive = new Thread(new ThreadStart(ThreadCheckASPPortAlive));
                ThrCheckPortAlive.Name = "ThrCheckPortAlive";
                ThrCheckPortAlive.IsBackground = true;
                ThrCheckPortAlive.Start();
                InitializeADCShow();
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
                        BeginInvoke(new MethodInvoker(() => funUpdateADC_Auto(sender)));
                    else
                        funUpdateADC_Auto(sender);
                };
                StateCommon.LoadingText += "Config ASP Serial Port completed\r\n";
                StateCommon.LoadingValue = 40;
                StateCommon.LoadingText += "Config All unit for test...\r\n";
                InitializeUnitManager(ASP_ControlPort);
                tabControl3.SelectedIndexChanged += TabControl3_SelectedIndexChanged;
                tabControl3.SelectedIndex = 0;
                TrysetSNVerifyShow();
                StateCommon.LoadingText += "Config All unit for test completed\r\n";
                StateCommon.LoadingValue = 50;
                StateCommon.LoadingText += "Initial Relay Control Board...\r\n";
                InitRelayLabels();
                StateCommon.LoadingText += "Initial Relay Control Board completed\r\n";
                StateCommon.LoadingValue = 60;
                StateCommon.LoadingText += "Change all mode to Auto mode...\r\n";
                for (int i = 0; i < ClsUnitManagercs.cls_Units.Length; i++)
                {
                    ClsUnitManagercs.cls_Units[i].cls_SequencyCommon.AutoMode = AutoMode;
                    ClsUnitManagercs.cls_Units[i].cls_SequencyTest.OnRequestUpdateAnalogValue += delegate (Cls_SequencyTest sender)
                    {
                        if (base.InvokeRequired)
                        {
                            BeginInvoke((MethodInvoker)delegate
                            {
                                funUpdateAnalogValue_Auto(sender);
                            });
                        }
                        else
                        {
                            funUpdateAnalogValue_Auto(sender);
                        }
                    };
                }
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
                StateCommon.LoadingText += "Create Data Summary Table completed\r\n";
                StateCommon.LoadingValue = 80;
                StateCommon.LoadingText += "Create Data History Query ....\r\n";
                StateCommon.LoadingText += "Create Data History Query completed\r\n";
                StateCommon.LoadingValue = 80;
                StateCommon.LoadingText += "Set All Relay to turn OFF\r\n";
                SetAllRelayOFF();
                UpdateTodayHistory();
                timer1.Start();
                timer2.Start();
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

        private void InitializeADCShow()
        {
            ADVShow[0] = txt_ADV1;
            ADVShow[1] = txt_ADV2;
            ADVShow[2] = txt_ADV3;
            ADVShow[3] = txt_ADV4;
            ADAShow[0] = txt_ADI1;
            ADAShow[1] = txt_ADI2;
            ADAShow[2] = txt_ADI3;
            ADAShow[3] = txt_ADI4;
            ADCShow[0] = txt_ADC1;
            ADCShow[1] = txt_ADC2;
            ADCShow[2] = txt_ADC3;
            ADCShow[3] = txt_ADC4;
            ADCShow[4] = txt_ADC5;
            ADCShow[5] = txt_ADC6;
            ADCShow[6] = txt_ADC7;
            ADCShow[7] = txt_ADC8;
        }

        private void funUpdateADC_Auto(Cls_ASPcontrol sender)
        {
            for(int i = 0; i < ADVShow.Length; i++)
            {
                if (!ADAShow[i].Created || !ADVShow[i].Created)
                    return;
                if (ADVShow[i].InvokeRequired)
                {
                    ADVShow[i].BeginInvoke(new Action(() => ADVShow[i].Text = sender.ADVs[i].ToString("0.00")));
                    ADAShow[i].BeginInvoke(new Action(() => ADAShow[i].Text = sender.ADIs[i].ToString("0.00")));
                }
                else
                {
                    ADVShow[i].Text = sender.ADVs[i].ToString("0.00") + " V";
                    ADAShow[i].Text = sender.ADIs[i].ToString("0.00") +" mA";
                }
            }
            for (int i = 0; i < ADCShow.Length; i++)
            {
                if (!ADCShow[i].Created)
                    return;
                if (ADCShow[i].InvokeRequired)
                {
                    ADCShow[i].BeginInvoke(new Action(() => ADCShow[i].Text = sender.ADCs[i].ToString("0.00")));
                }
                else
                {
                    ADCShow[i].Text = sender.ADCs[i].ToString("0.00");
                }
            }
        }
        private void funUpdateAnalogValue_Auto(Cls_SequencyTest sender)
        {
            InitializeChartSeries(2);
            if (chart_Flow.InvokeRequired)
            {
                chart_Flow.BeginInvoke((Action)delegate
                {
                    UpdateChartDataWithDateTime(sender);
                });
            }
            else
            {
                UpdateChartDataWithDateTime(sender);
            }
            
        }

        private void InitializeChartSeries(int adiCount)
        {
            if (chart_Flow.Series.Count == adiCount)
                return; // Đã khởi tạo rồi

            chart_Flow.Series.Clear();
            var s = new System.Windows.Forms.DataVisualization.Charting.Series($"PV");
            s.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            s.BorderWidth = 2;
            chart_Flow.Series.Add(s);
            s = new System.Windows.Forms.DataVisualization.Charting.Series($"SV");
            s.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            s.BorderWidth = 2;
            chart_Flow.Series.Add(s);
        }
        private readonly object chartLock = new object();
        private void UpdateChartDataWithDateTime(Cls_SequencyTest sender)
        {
            DateTime now = DateTime.Now;
            int num = 20;
            ChartArea chartArea = chart_Flow.ChartAreas[0];
            chartArea.AxisX.LabelStyle.Format = "HH:mm:ss";
            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisX.IntervalType = DateTimeIntervalType.Seconds;
            chartArea.AxisX.Interval = 1.0;
            chartArea.AxisY.LabelStyle.Font = new Font("Arial", 10f);
            chartArea.AxisY.Title = "Lưu lượng (L/min)";
            chartArea.AxisY.TitleFont = new Font("Arial", 10f, FontStyle.Bold);
            chartArea.AxisY.Interval = 10.0;
            chartArea.AxisY.Minimum = 0.0;
            chartArea.AxisY.Maximum = 20.0;
            switch (nestMenu)
            {
                case NestMenu.Nest1:
                    chart_Flow.Series["PV"].Points.AddXY(now, GetFlowrate(ClsUnitManagercs.cls_Units[0].cls_SequencyTest.FlowRate));
                    chart_Flow.Series["SV"].Points.AddXY(now, (double)nm_SV.Value);
                    txt_PVFlow.Text = ClsUnitManagercs.cls_Units[0].cls_SequencyTest.FlowRate.ToString("0.00");
                    break;
                case NestMenu.Nest2:
                    chart_Flow.Series["PV"].Points.AddXY(now, GetFlowrate(ClsUnitManagercs.cls_Units[1].cls_SequencyTest.FlowRate));
                    chart_Flow.Series["SV"].Points.AddXY(now, (double)nm_SV.Value);
                    txt_PVFlow.Text = ClsUnitManagercs.cls_Units[1].cls_SequencyTest.FlowRate.ToString("0.00");
                    break;
                case NestMenu.Nest3:
                    chart_Flow.Series["PV"].Points.AddXY(now, GetFlowrate(ClsUnitManagercs.cls_Units[2].cls_SequencyTest.FlowRate));
                    chart_Flow.Series["SV"].Points.AddXY(now, (double)nm_SV.Value);
                    txt_PVFlow.Text = ClsUnitManagercs.cls_Units[2].cls_SequencyTest.FlowRate.ToString("0.00");
                    break;
            }   
            foreach (Series item in chart_Flow.Series)
            {
                while (item.Points.Count > num)
                {
                    item.Points.RemoveAt(0);
                }
            }
            if (chart_Flow.Series.Count > 0 && chart_Flow.Series[0].Points.Count > 0)
            {
                double xValue = chart_Flow.Series[0].Points[0].XValue;
                double xValue2 = chart_Flow.Series[0].Points[chart_Flow.Series[0].Points.Count - 1].XValue;
                chartArea.AxisX.Minimum = xValue;
                chartArea.AxisX.Maximum = xValue2;
            }
        }
        private double GetFlowrate(double mA)
        {
            double flowRate = 0;
            if (mA < 4 || mA>20) return flowRate;
            else
            {
                flowRate = ((mA - 4) / 16) * 200;
            }
            return flowRate;
        }
        private double GetAirPressure(double V)
        {
            double AirPressure = 0;
            if (V > 10) return AirPressure;
            else
            {

            }
            return AirPressure;
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
        private void TrysetSNVerifyShow()
        {
            try
            {
                txt_SN_Lenght.Text = ClsUnitManagercs.cls_Units[0].SN_Length.ToString();
                txt_ControlString.Text = ClsUnitManagercs.cls_Units[0].PN_Ctl_String;
                txt_Leght.Text = ClsUnitManagercs.cls_Units[0].Ctl_Str_Length.ToString();
                txt_Offset.Text = ClsUnitManagercs.cls_Units[0].Ctl_Str_Offset.ToString();
            }
            catch
            {
                MessageBox.Show("Error when set SN verify string");
                txt_SN_Lenght.Text = "";
                txt_ControlString.Text = "";
                txt_Leght.Text = "";
                txt_Offset.Text = "";
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
                this.cbb_InverterComPort.Items.AddRange(SerialPort.GetPortNames());
                this.cbb_InverterBaudrate.Items.AddRange(Baudrates.Cast<object>().ToArray());
                this.cbb_InverterDatabit.Items.AddRange(DataBits.Cast<object>().ToArray());
                this.cbb_InverterParity.Items.AddRange(Paritys.Cast<object>().ToArray());
                this.cbb_InverterStopbit.Items.AddRange(stopBits.Cast<object>().ToArray());
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
                SerialCom = clsInverterModbus.SerialCom;
                Baudrate = clsInverterModbus.Baudrate;
                DataBit = clsInverterModbus.DataBit;
                Parity = clsInverterModbus.Parity;
                StopBit = clsInverterModbus.StopBit;
                cbb_InverterComPort.SelectedItem = SerialCom;
                cbb_InverterBaudrate.SelectedItem = Baudrate;
                cbb_InverterDatabit.SelectedItem = DataBit;
                cbb_InverterParity.SelectedItem = Parity;
                cbb_InverterStopbit.SelectedItem = StopBit;
                cbb_TestMode.SelectedItem = 0;
            }
            catch (Exception ex )
            {
                log.Error( ex );
                MessageBox.Show("Error when generate Com Port: " + ex.Message);
            }
        }
        private void TabControl3_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = tabControl3.SelectedIndex;
            txt_SN_Lenght.Text = ClsUnitManagercs.cls_Units[selectedIndex].SN_Length.ToString();
            txt_ControlString.Text = ClsUnitManagercs.cls_Units[selectedIndex].PN_Ctl_String;
            txt_Leght.Text = ClsUnitManagercs.cls_Units[selectedIndex].Ctl_Str_Length.ToString();
            txt_Offset.Text = ClsUnitManagercs.cls_Units[selectedIndex].Ctl_Str_Offset.ToString();
        }
        private void InitializeUnitManager( Cls_ASPcontrol cls_AS)
        {
            string[] array = File.ReadAllLines("C:\\Aavid_Test\\Setup-ini\\Flushing_Part_Numbers.txt")
                     .Where(line => !string.IsNullOrWhiteSpace(line))
                     .ToArray();
            AssyPN = new string[array.Length - 1];
            WorkOder = new string[array.Length - 1];
            for (int i = 0; i < AssyPN.Length; i++)
            {
                string[] array2 = array[i + 1].Split(',');
                AssyPN[i] = array2[1].Trim();
                WorkOder[i]= array2[2].Trim();
            }
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
            ClsUnitManagercs.Initialize(AssyPN, WorkOder,this, stationID, cls_AS, clsInverterModbus);
            GenerateProcess();
            uC_MaintTests = new UC_MaintTest[ClsUnitManagercs.cls_Units.Length];
            for (int i = 0; i < ClsUnitManagercs.cls_Units.Length; i++)
            {
                uC_MaintTests[i] = new UC_MaintTest(i);
                uC_MaintTests[i].Dock = DockStyle.Fill;
                pl_main[i].Controls.Add(uC_MaintTests[i]);
            }
            uC_PartNumberInfors = new UC_PartNumberInfor[ClsUnitManagercs.cls_Units.Length];
            for (int j = 0; j < ClsUnitManagercs.cls_Units.Length; j++)
            {
                uC_PartNumberInfors[j] = new UC_PartNumberInfor(j, SecsionTest);
                uC_PartNumberInfors[j].InitUI();
                uC_PartNumberInfors[j].Dock = DockStyle.Fill;
                pl_infor[j].Controls.Add(uC_PartNumberInfors[j]);
                ClsUnitManagercs.cls_Units[j].cls_SequencyCommon.AutoMode = true;
                ClsUnitManagercs.cls_Units[j].cls_SequencyCommon.OnRequestUpdateStage += delegate (Cls_SequencyCommon sender)
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
                ClsUnitManagercs.cls_Units[j].cls_SequencyTest.TestType = cbb_TestMode.Text;

                ClsUnitManagercs.cls_Units[j].cls_SequencyTest.OnRequestAddProcessLogTest += (s, e) =>
                {
                    int fromUnit = e.UnitIndex;
                    DataTable data = e.LogData;
                    UpdateChildData(fromUnit, data);
                };
            }
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
                    Tbtn_Pump.Checked = relays[ClsUnitManagercs.cls_Units[0].cls_SequencyTest.iRelayPump - 1];
                    Tbtn_3WaySwitch.Checked = relays[ClsUnitManagercs.cls_Units[0].cls_SequencyTest.iRelay3Way_Air_Water - 1];
                    Tbtn_ReverserFlow.Checked = relays[ClsUnitManagercs.cls_Units[0].cls_SequencyTest.iRelay3Way_Reverse - 1];
                    break;

                case NestMenu.Nest2:
                    UpdateColor(lb_Red, relays[12] ? Color.Red : Color.Gray);
                    UpdateColor(lb_Yellow, relays[13] ? Color.Yellow : Color.Gray);
                    UpdateColor(lb_Green, relays[14] ? Color.Green : Color.Gray);
                    UpdateColor(lb_Buzzer, relays[15] ? Color.Blue : Color.Gray);
                    Tbtn_Pump.Checked = relays[ClsUnitManagercs.cls_Units[1].cls_SequencyTest.iRelayPump - 1];
                    Tbtn_3WaySwitch.Checked = relays[ClsUnitManagercs.cls_Units[1].cls_SequencyTest.iRelay3Way_Air_Water - 1];
                    Tbtn_ReverserFlow.Checked = relays[ClsUnitManagercs.cls_Units[1].cls_SequencyTest.iRelay3Way_Reverse - 1];
                    break;

                case NestMenu.Nest3:
                    UpdateColor(lb_Red, relays[16] ? Color.Red : Color.Gray);
                    UpdateColor(lb_Yellow, relays[17] ? Color.Yellow : Color.Gray);
                    UpdateColor(lb_Green, relays[18] ? Color.Green : Color.Gray);
                    UpdateColor(lb_Buzzer, relays[19] ? Color.Blue : Color.Gray);
                    Tbtn_Pump.Checked = relays[ClsUnitManagercs.cls_Units[2].cls_SequencyTest.iRelayPump - 1];
                    Tbtn_3WaySwitch.Checked = relays[ClsUnitManagercs.cls_Units[2].cls_SequencyTest.iRelay3Way_Air_Water - 1];
                    Tbtn_ReverserFlow.Checked = relays[ClsUnitManagercs.cls_Units[2].cls_SequencyTest.iRelay3Way_Reverse - 1];
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

            DataGridView[] dgv = new DataGridView[ClsUnitManagercs.cls_Units.Length];
            pl_infor = new Panel[ClsUnitManagercs.cls_Units.Length];
            pl_main = new Panel[ClsUnitManagercs.cls_Units.Length];
            TableLayoutPanel tbl_MainPanel = new TableLayoutPanel();
            tbl_MainPanel.Dock = DockStyle.Fill;
            tbl_MainPanel.ColumnCount = ClsUnitManagercs.cls_Units.Length;
            tbl_MainPanel.RowCount = 1;
            tbl_MainPanel.ColumnStyles.Clear();
            for (int i = 0; i < tbl_MainPanel.ColumnCount; i++)
            {
                tbl_MainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / tbl_MainPanel.ColumnCount));
            }
            tableLayoutPanel10.Controls.Add(tbl_MainPanel, 0, 0);
            for (int i = 0; i < ClsUnitManagercs.cls_Units.Length; i++)
            {
                string tabName = $"Fixture {i + 1}";
                TabPage tabPage = new TabPage(tabName);
                // Tạo TableLayoutPanel
                TableLayoutPanel tableLayout = new TableLayoutPanel();
                tableLayout.Dock = DockStyle.Fill;
                tableLayout.ColumnCount = 2;
                tableLayout.RowCount = 1; // Có thể tăng sau nếu bạn muốn nhiều hàng
                tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80F));
                tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
                tbl_MainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                tableLayout.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single; // Tùy chọn, để có viền giữa các ô
                dgv[i] = new DataGridView();
                dgv[i].Columns.Clear();
                dgv[i].Rows.Clear();
                dgv[i].ReadOnly = true;                      // Không cho phép sửa
                dgv[i].ClearSelection();                     // Bỏ chọn dòng mặc định
                dgv[i].DefaultCellStyle.SelectionBackColor = dgv[i].DefaultCellStyle.BackColor;
                dgv[i].DefaultCellStyle.SelectionForeColor = dgv[i].DefaultCellStyle.ForeColor;
                dgv[i].RowHeadersVisible = false;
                dgv[i].MultiSelect = false;
                dgv[i].Enabled = true;   
                dgv[i].Dock = DockStyle.Fill; // Để nó chiếm toàn bộ ô
                                              // Vẫn scroll được
                                              // Tạo một cột duy nhất để hiển thị giá trị
                DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                col.HeaderText = "Process";                  // Không hiện tiêu đề
                col.Name = "Process";
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv[i].Columns.Add(col);
                for (int index = 0; index < ClsUnitManagercs.cls_Units[i].verifyFlags.Length; index++)
                {
                    dgv[i].Rows.Add(ClsUnitManagercs.cls_Units[i].verifyFlags[index]);
                }
                tableLayout.Controls.Add(dgv[i], 1, 0);
                pl_infor[i] = new Panel();
                pl_infor[i].Dock = DockStyle.Fill;
                pl_main[i] = new Panel();
                pl_main[i].Dock = DockStyle.Fill;
                tbl_MainPanel.Controls.Add(pl_main[i], i, 0);
                tableLayout.Controls.Add(pl_infor[i], 0, 0);
                tabPage.Controls.Add(tableLayout);
                tabControl3.TabPages.Add(tabPage);
            }
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            txt_PNSetupfile.Text = sSetup_Folder;
            txt_PNSetupfile.Enabled = false;
            if (StateCommon.CheckFileExists(sSetup_Folder))
            {
                lbl_StatusPNfile.BackColor = Color.Green;
            }
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
            lb_StationID.Text = ClsUnitManagercs.cls_Units[0].StaionID;
            AutoMode = true;
            DisableAllControl();
            btn_ChangeMode.Text = "AUTO";
            for (int i = 0; i < ClsUnitManagercs.cls_Units.Length; i++)
            {
                ClsUnitManagercs.cls_Units[i].cls_SequencyCommon.AutoMode = true;
                ClsUnitManagercs.cls_Units[i].cls_SequencyCommon.AutoMode = true;
            }
            button4.Enabled = true;
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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string cmd = txt_Command.Text + "\n";
            ASP_ControlPort.ASPSerialWriteCommand(cmd);
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
                    lb_StageStatus.Text = ClsUnitManagercs.cls_Units[0].cls_SequencyCommon.process.ToString();
                    break;
                case NestMenu.Nest2:
                    lb_StageStatus.Text = ClsUnitManagercs.cls_Units[1].cls_SequencyCommon.process.ToString();
                    break;
                case NestMenu.Nest3:
                    lb_StageStatus.Text = ClsUnitManagercs.cls_Units[2].cls_SequencyCommon.process.ToString();
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
                case NestMenu.Nest3:
                    RefeshAllNestlb();
                    lb_Nest3.BackColor = Color.LimeGreen;
                    break;
            }
            ASP_ControlPort.GetAllRelay();
        }
        private void RefeshAllNestlb()
        {
            lb_Nest1.BackColor = Color.Gray;
            lb_Nest2.BackColor = Color.Gray;
            lb_Nest3.BackColor = Color.Gray;
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
            Tbtn_3WaySwitch.Enabled = false;
            Tbtn_Pump.Enabled = false;
            Tbtn_ReverserFlow.Enabled = false;
            btn_SetFlow.Enabled = false;
            btn_AdjustFlow.Enabled = false;
            txt_PVFlow.Enabled = false;
            nm_SV.Enabled = false;
            lb_Buzzer.Enabled = false;
            lb_Green.Enabled = false;
            lb_Yellow.Enabled = false;
            lb_Red.Enabled = false;
        }
        private void EnableAllControl()
        {
            tbl_RelayControl.Enabled = true;
            tableLayoutPanel15.Enabled = true;
            Tbtn_3WaySwitch.Enabled = true;
            Tbtn_Pump.Enabled = true;
            Tbtn_ReverserFlow.Enabled = true;
            btn_SetFlow.Enabled = true;
            btn_AdjustFlow.Enabled = true;
            txt_PVFlow.Enabled = true;
            nm_SV.Enabled = true;
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
                for (int i=0;i<ClsUnitManagercs.cls_Units.Length; i++)
                {
                    ClsUnitManagercs.cls_Units[i].cls_SequencyCommon.AutoMode = true;
                    ClsUnitManagercs.cls_Units[i].cls_SequencyCommon.AutoMode = true;
                }
            }
            else
            {
                AutoMode = false;
                EnableAllControl();
                btn_ChangeMode.Text = "MANUAL";
                for (int i = 0; i < ClsUnitManagercs.cls_Units.Length; i++)
                {
                    ClsUnitManagercs.cls_Units[i].cls_SequencyCommon.AutoMode = false;
                    ClsUnitManagercs.cls_Units[i].cls_SequencyCommon.AutoMode = false;
                }
            }    
        }
        private void Tbtn_Pump_CheckedChanged(object sender, EventArgs e)
        {
            switch(nestMenu)
            {
                case NestMenu.Nest1:
                    if (Tbtn_Pump.Checked)
                        ClsUnitManagercs.cls_Units[0].cls_SequencyTest.SwitchRelayPump(true);
                    else
                        ClsUnitManagercs.cls_Units[0].cls_SequencyTest.SwitchRelayPump(false);
                    break;
                case NestMenu.Nest2:
                    if (Tbtn_Pump.Checked)
                        ClsUnitManagercs.cls_Units[1].cls_SequencyTest.SwitchRelayPump(true);
                    else
                        ClsUnitManagercs.cls_Units[1].cls_SequencyTest.SwitchRelayPump(false);
                    break;
                case NestMenu.Nest3:
                    if (Tbtn_Pump.Checked)
                        ClsUnitManagercs.cls_Units[2].cls_SequencyTest.SwitchRelayPump(true);
                    else
                        ClsUnitManagercs.cls_Units[2].cls_SequencyTest.SwitchRelayPump(false);
                    break;
            }
        }

        private void Tbtn_3WaySwitch_CheckedChanged(object sender, EventArgs e)
        {
            switch (nestMenu)
            {
                case NestMenu.Nest1:
                    if (Tbtn_3WaySwitch.Checked)
                        ClsUnitManagercs.cls_Units[0].cls_SequencyTest.SwitchRelay3Way_Air_Water(true);
                    else
                        ClsUnitManagercs.cls_Units[0].cls_SequencyTest.SwitchRelay3Way_Air_Water(false);
                    break;
                case NestMenu.Nest2:
                    if (Tbtn_3WaySwitch.Checked)
                        ClsUnitManagercs.cls_Units[1].cls_SequencyTest.SwitchRelay3Way_Air_Water(true);
                    else
                        ClsUnitManagercs.cls_Units[1].cls_SequencyTest.SwitchRelay3Way_Air_Water(false);
                    break;
                case NestMenu.Nest3:
                    if (Tbtn_3WaySwitch.Checked)
                        ClsUnitManagercs.cls_Units[2].cls_SequencyTest.SwitchRelay3Way_Air_Water(true);
                    else
                        ClsUnitManagercs.cls_Units[2].cls_SequencyTest.SwitchRelay3Way_Air_Water(false);
                    break;
            }
        }

        private void Tbtn_ReverserFlow_CheckedChanged(object sender, EventArgs e)
        {
            switch (nestMenu)
            {
                case NestMenu.Nest1:
                    if (Tbtn_ReverserFlow.Checked)
                        ClsUnitManagercs.cls_Units[0].cls_SequencyTest.SwitchRelay3Way_Reverse(true);
                    else
                        ClsUnitManagercs.cls_Units[0].cls_SequencyTest.SwitchRelay3Way_Reverse(false);
                    break;
                case NestMenu.Nest2:
                    if (Tbtn_ReverserFlow.Checked)
                        ClsUnitManagercs.cls_Units[1].cls_SequencyTest.SwitchRelay3Way_Reverse(true);
                    else
                        ClsUnitManagercs.cls_Units[1].cls_SequencyTest.SwitchRelay3Way_Reverse(false);
                    break;
                case NestMenu.Nest3:
                    if (Tbtn_ReverserFlow.Checked)
                        ClsUnitManagercs.cls_Units[2].cls_SequencyTest.SwitchRelay3Way_Reverse(true);
                    else
                        ClsUnitManagercs.cls_Units[2].cls_SequencyTest.SwitchRelay3Way_Reverse(false);
                    break;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            switch (nestMenu)
            {
                case NestMenu.Nest1:
                    ClsUnitManagercs.cls_Units[0].cls_SequencyTest.bGetAnalogValue = !ClsUnitManagercs.cls_Units[0].cls_SequencyTest.bGetAnalogValue;
                    break;
                case NestMenu.Nest2:
                    ClsUnitManagercs.cls_Units[1].cls_SequencyTest.bGetAnalogValue = !ClsUnitManagercs.cls_Units[1].cls_SequencyTest.bGetAnalogValue;
                    break;
                case NestMenu.Nest3:
                    ClsUnitManagercs.cls_Units[2].cls_SequencyTest.bGetAnalogValue = !ClsUnitManagercs.cls_Units[2].cls_SequencyTest.bGetAnalogValue;
                    break;
            }
        }
        private void btn_InverterSave_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Do you want to save configuration for Inverter control Serial Port?", "Notification", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                clsInverterModbus.SerialCom = cbb_InverterComPort.Text;
                clsInverterModbus.Baudrate = int.Parse(cbb_InverterBaudrate.Text);
                clsInverterModbus.DataBit = int.Parse(cbb_InverterDatabit.Text);
                clsInverterModbus.Parity = (Parity)Enum.Parse(typeof(Parity), cbb_InverterParity.Text);
                clsInverterModbus.StopBit = (StopBits)Enum.Parse(typeof(StopBits), cbb_InverterStopbit.Text);
                clsInverterModbus.SaveInverterSerialPortInformation();
                log.Error($"Save Inverter Serial Port Information: {clsInverterModbus.SerialCom} {clsInverterModbus.Baudrate} {clsInverterModbus.DataBit} {clsInverterModbus.Parity} {clsInverterModbus.StopBit}");
                clsInverterModbus.Close();
                clsInverterModbus.Open();
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
            for (int i = 0; i < ClsUnitManagercs.cls_Units.Length; i++)
            {
                ClsUnitManagercs.cls_Units[i].cls_SequencyTest.TestType = cbb_TestMode.Text;
            }
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
                using (FrmMessage message = new FrmMessage("Bạn có muốn đóng chương trình không???", Color.Red, Color.Black))
                {
                    message.ShowDialog();
                    if(message.DialogResult == DialogResult.OK)
                    {
                        // Optional: Hiện thông báo hoặc loading
                        this.Enabled = false; // Khoá UI nếu cần
                        await ASP_ControlPort.SetAllRelayOFF();
                        this.Enabled = true;
                        this.Close(); // Đóng form lại sau khi hoàn tất
                    }   
                    else
                    {
                        _isClosingHandled = false; // Reset trạng thái nếu không đóng
                        return; // Trả lại trạng thái form để không đóng
                    }
                }
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

        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                if (ClsUnitManagercs.cls_Units[0] == null ||
                ClsUnitManagercs.cls_Units[1] == null ||
                ClsUnitManagercs.cls_Units[2] == null)
                {
                    return;
                }
                if (ClsUnitManagercs.cls_Units[0].cls_SequencyTest.testSeq != Cls_SequencyTest.TestSeq.SN_INSERT &&
                    ClsUnitManagercs.cls_Units[1].cls_SequencyTest.testSeq != Cls_SequencyTest.TestSeq.SN_INSERT &&
                    ClsUnitManagercs.cls_Units[2].cls_SequencyTest.testSeq != Cls_SequencyTest.TestSeq.SN_INSERT)
                {
                    txt_Fixture.Focus();
                }
            }
            catch
            { }
        }
        private void txt_Fixture_TextChanged(object sender, EventArgs e)
        {
            string fixtureText = txt_Fixture.Text.Trim().ToUpper() ;
            if (string.IsNullOrEmpty(fixtureText) && !fixtureText.StartsWith("STATION"))
            {
                return; // Không làm gì nếu TextBox rỗng
            }   
            string lastchar = fixtureText[fixtureText.Length-1].ToString();
            switch (lastchar)
            {
                case "1":
                    if ((ClsUnitManagercs.cls_Units[0].cls_SequencyTest.testSeq == Cls_SequencyTest.TestSeq.WAIT ||
                    ClsUnitManagercs.cls_Units[0].cls_SequencyTest.testSeq == Cls_SequencyTest.TestSeq.SN_INSERT||
                    ClsUnitManagercs.cls_Units[0].cls_SequencyTest.testSeq == Cls_SequencyTest.TestSeq.ERROR))
                    {
                        uC_MaintTests[0].SetSNForTest();
                        ClsUnitManagercs.cls_Units[0].cls_SequencyTest.testSeq = Cls_SequencyTest.TestSeq.SN_INSERT;
                    }
                    txt_Fixture.Text = ""; // Xóa nội dung TextBox sau khi xử lý
                    break;
                case "2":
                    if ((ClsUnitManagercs.cls_Units[1].cls_SequencyTest.testSeq == Cls_SequencyTest.TestSeq.WAIT ||
                    ClsUnitManagercs.cls_Units[1].cls_SequencyTest.testSeq == Cls_SequencyTest.TestSeq.SN_INSERT||
                    ClsUnitManagercs.cls_Units[1].cls_SequencyTest.testSeq == Cls_SequencyTest.TestSeq.ERROR))
                    {
                        uC_MaintTests[1].SetSNForTest();
                        ClsUnitManagercs.cls_Units[1].cls_SequencyTest.testSeq = Cls_SequencyTest.TestSeq.SN_INSERT;
                    }
                    txt_Fixture.Text = ""; // Xóa nội dung TextBox sau khi xử lý
                    break;
                case "3":
                    if ((ClsUnitManagercs.cls_Units[2].cls_SequencyTest.testSeq == Cls_SequencyTest.TestSeq.WAIT ||
                    ClsUnitManagercs.cls_Units[2].cls_SequencyTest.testSeq == Cls_SequencyTest.TestSeq.SN_INSERT||
                    ClsUnitManagercs.cls_Units[2].cls_SequencyTest.testSeq == Cls_SequencyTest.TestSeq.ERROR))
                    {
                        uC_MaintTests[2].SetSNForTest();
                        ClsUnitManagercs.cls_Units[2].cls_SequencyTest.testSeq = Cls_SequencyTest.TestSeq.SN_INSERT;
                    }
                    txt_Fixture.Text = ""; // Xóa nội dung TextBox sau khi xử lý
                    break;
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            UpdateTodayHistory();
        }
        private void UpdateTodayHistory()
        {
            try
            {
                string StationID = ClsUnitManagercs.cls_Units[0].StaionID;
                DateTime dt1 = DateTime.Today;           // hôm nay lúc 00:00:00
                DateTime dt2 = DateTime.Now;             // thời gian hiện tại
                string cmd = $@"SELECT [Station]
                                   ,[FailCode]
                                   ,[Date_Time]
                                   FROM [Production_SZ].[dbo].[DI_Water_Wash_Log] 
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

        private void btn_SaveAnalogInput_Click(object sender, EventArgs e)
        {

        }
    }
}
