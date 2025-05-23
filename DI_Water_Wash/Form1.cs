using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using DI_Water_Wash;
using System.IO.Ports;
using log4net;
using DI_Water_Wash.Sequence;


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
        private Font F = new Font("Arial", 9);
        private string sSetup_Folder = "C:\\Aavid_Test\\Setup-ini\\Flushing_Part_Numbers.txt";
        private string sFlushing_File = "";
        int coreCount = 6;
        public Cls_DBMsSQL ParameterDB = new Cls_DBMsSQL();
        public Cls_DBMsSQL ProductionDB = new Cls_DBMsSQL();
        public string[] AssyPN;
        Dictionary<string, string> myDict = new Dictionary<string, string>();
        private UC_PartNumberInfor[] uC_PartNumberInfors = new UC_PartNumberInfor[3];
        private UC_PartNumberInfor.SecsionTest SecsionTest = UC_PartNumberInfor.SecsionTest.DIWaterWash;
        private int[] Baudrates = { 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200 };
        private int[] DataBits = { 5, 6, 7, 8 };
        private Parity[] Paritys = { Parity.None, Parity.Odd, Parity.Even };
        private StopBits[] stopBits = { StopBits.One, StopBits.None, StopBits.OnePointFive, StopBits.Two };
        private Label[] relayLabels = new Label[30];
        private bool UpdateRelayTB = true;
        delegate void Deltact();
        delegate void DelResource(); 
        private bool AutoMode = false;

        NestMenu nestMenu = NestMenu.None;
        enum NestMenu
        {
            None,Nest1, Nest2, Nest3, Nest4,
        }
        private void GetDictionary()
        {

        }
        public Form1()
        {
            InitializeComponent();
            
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
                StateCommon.LoadingText += "Config ASP Serial Port completed\r\n";
                StateCommon.LoadingValue = 40;
                StateCommon.LoadingText += "Config All unit for test...\r\n";
                InitializeUnitManager(ASP_ControlPort);
                tabControl3.SelectedIndexChanged += TabControl3_SelectedIndexChanged;
                tabControl3.SelectedIndex = 0;
                txt_SN_Lenght.Text = ClsUnitManagercs.cls_Units[0].SN_Length.ToString();
                txt_ControlString.Text = ClsUnitManagercs.cls_Units[0].PN_Ctl_String;
                txt_Leght.Text = ClsUnitManagercs.cls_Units[0].Ctl_Str_Length.ToString();
                txt_Offset.Text = ClsUnitManagercs.cls_Units[0].Ctl_Str_Offset.ToString();
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
                    ClsUnitManagercs.cls_Units[i].cls_SequencyCommon.AutoMode = AutoMode;
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
                StateCommon.LoadingText += "Complete\r\n";
                StateCommon.LoadingValue = 100;
                StateCommon.bLoading = false;
            }
            else
            {
                System.Windows.Forms.Application.Exit();
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
            bool currentStatus = relayLabels[index].BackColor == Color.LimeGreen;
            bool newStatus = !currentStatus;
            try
            {
                if (ASP_ControlPort.isConnected)
                {
                    await ASP_ControlPort.SetRelayONOFFAsync(index + 1, newStatus ? 1 : 0);
                    await ASP_ControlPort.GetAllRelay(); // Chờ nhận xong dữ liệu
                    //UpdateRelayStatus(ASP_ControlPort.relayStates); // Sau đó mới cập nhật UI
                }    
                else
                    MessageBox.Show("SerialPort chưa mở!");
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
                this.cbbComPort.Items.AddRange(SerialPort.GetPortNames());
                this.cbbBaudrate.Items.AddRange(Baudrates.Cast<object>().ToArray());
                this.cbbDataBit.Items.AddRange(DataBits.Cast<object>().ToArray());
                this.cbbParity.Items.AddRange(Paritys.Cast<object>().ToArray());
                this.cbbStopBit.Items.AddRange(stopBits.Cast<object>().ToArray());
                string SerialCom = ASP_ControlPort.SerialCom;
                int Baudrate = ASP_ControlPort.Baudrate;
                int DataBit = ASP_ControlPort.DataBit;
                Parity Parity = ASP_ControlPort.Parity;
                StopBits StopBit = ASP_ControlPort.StopBit;
                cbbComPort.SelectedItem = SerialCom;
                cbbBaudrate.SelectedItem = Baudrate;
                cbbDataBit.SelectedItem = DataBit;
                cbbParity.SelectedItem = Parity;
                cbbStopBit.SelectedItem = StopBit;
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
            string[] allLine = File.ReadAllLines(@"C:\Aavid_Test\Setup-ini\Flushing_Part_Numbers.txt");
            AssyPN = new string[allLine.Length-1];
            for (int i = 0; i < AssyPN.Length; i++)
            {
                string[] line = allLine[i+1].Split(',');
                AssyPN[i] = line[1].Trim();
            }
            ClsUnitManagercs.Initialize(AssyPN, cls_AS);
            GenerateProcess();
            Panel[]panel = new Panel[3];
            panel[0] = pl_PNInfor1;
            panel[1] = pl_PNInfor2;
            panel[2] = pl_PNInfor3;
            for (int i = 0; i < ClsUnitManagercs.cls_Units.Length; i++)
            {
                uC_PartNumberInfors[i] = new UC_PartNumberInfor(i, SecsionTest);
                uC_PartNumberInfors[i].InitUI();
                uC_PartNumberInfors[i].Dock = DockStyle.Fill;
                panel[i].Controls.Add(uC_PartNumberInfors[i]);
                ClsUnitManagercs.cls_Units[i].cls_SequencyCommon.OnRequestUpdateStage += (sender) =>
                {
                    if (InvokeRequired)
                        BeginInvoke(new MethodInvoker(() => funUpdateStage_Auto(sender)));
                    else
                        funUpdateStage_Auto(sender);
                };
            }

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
            DataGridView[] dgv = new DataGridView[3];
            dgv[0] = dgv1;
            dgv[1] = dgv2;
            dgv[2] = dgv3;
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
                for (int index = 0; index < ClsUnitManagercs.cls_Units[i].verifyFlags.Length; index++)
                {
                    dgv[i].Rows.Add(ClsUnitManagercs.cls_Units[i].verifyFlags[index]);
                }
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
        }

        private void btn_SaveASPPort_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Do you want to save configuration for ASP control Serial Port?","Notification", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                ASP_ControlPort.SerialCom = cbbComPort.Text;
                ASP_ControlPort.Baudrate = int.Parse(cbbBaudrate.Text);
                ASP_ControlPort.DataBit = int.Parse(cbbDataBit.Text);
                ASP_ControlPort.Parity = (Parity)Enum.Parse(typeof(Parity), cbbParity.Text);
                ASP_ControlPort.StopBit = (StopBits)Enum.Parse(typeof(StopBits), cbbStopBit.Text);
                ASP_ControlPort.SaveASPSerialPortInformation();
                log.Error($"Save ASP Serial Port Information: {ASP_ControlPort.SerialCom} {ASP_ControlPort.Baudrate} {ASP_ControlPort.DataBit} {ASP_ControlPort.Parity} {ASP_ControlPort.StopBit}");
                ASP_ControlPort.DisconnectASPSerial();
                ASP_ControlPort.ConnectASPSerial();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string cmd = txtx_Command.Text + "\n";
            ASP_ControlPort.ASPSerialWriteCommand(cmd);
        }
        public void funAddLog_Auto(Cls_ASPcontrol unit)
        {
            // Lấy UC_Show tương ứng
            // Hiển thị ảnh lên PictureBox (Image_box)
            if (rtb_ASPPortBuffer.InvokeRequired)
            {
                rtb_ASPPortBuffer.BeginInvoke(new MethodInvoker(() =>
                {
                    StateCommon.AddTextToRichTextBox(unit.DataReceive,rtb_ASPPortBuffer);
                }));
            }
            else
            {
                StateCommon.AddTextToRichTextBox(unit.DataReceive, rtb_ASPPortBuffer);
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
            //UpdateRelayStatus(ASP_ControlPort.relayStates); // Sau đó mới cập nhật UI
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
                    UpdateLEDStatus();
                    lb_Nest1.BackColor = Color.LimeGreen;
                    break;
                case NestMenu.Nest2:
                    RefeshAllNestlb();
                    UpdateLEDStatus();
                    lb_Nest2.BackColor = Color.LimeGreen;
                    break;
                case NestMenu.Nest3:
                    RefeshAllNestlb();
                    UpdateLEDStatus();
                    lb_Nest3.BackColor = Color.LimeGreen;
                    break;
            }
        }
        private void RefeshAllNestlb()
        {
            lb_Nest1.BackColor = Color.Gray;
            lb_Nest2.BackColor = Color.Gray;
            lb_Nest3.BackColor = Color.Gray;
        }
        private async void UpdateLEDStatus()
        {
            await ASP_ControlPort.GetAllRelay(); // Chờ dữ liệu relay về
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
        
        private void btn_ChangeMode_Click(object sender, EventArgs e)
        {
            if(!AutoMode)
            {
                AutoMode = true;
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
                btn_ChangeMode.Text = "MANUAL";
                for (int i = 0; i < ClsUnitManagercs.cls_Units.Length; i++)
                {
                    ClsUnitManagercs.cls_Units[i].cls_SequencyCommon.AutoMode = false;
                    ClsUnitManagercs.cls_Units[i].cls_SequencyCommon.AutoMode = false;
                }
            }    
        }

        private void Tbtn_3Way_CheckedChanged(object sender, EventArgs e)
        {
            switch (nestMenu)
            {
                case NestMenu.Nest1:
                    if (Tbtn_3Way.Checked)
                        ClsUnitManagercs.cls_Units[0].cls_SequencyTest.SwitchRelay3Way_Air_Water(true);
                    else
                        ClsUnitManagercs.cls_Units[0].cls_SequencyTest.SwitchRelay3Way_Air_Water(false);
                    break;
                case NestMenu.Nest2:
                    if (Tbtn_3Way.Checked)
                        ClsUnitManagercs.cls_Units[1].cls_SequencyTest.SwitchRelay3Way_Air_Water(true);
                    else
                        ClsUnitManagercs.cls_Units[1].cls_SequencyTest.SwitchRelay3Way_Air_Water(false);
                    break;
                case NestMenu.Nest3:
                    if (Tbtn_3Way.Checked)
                        ClsUnitManagercs.cls_Units[2].cls_SequencyTest.SwitchRelay3Way_Air_Water(true);
                    else
                        ClsUnitManagercs.cls_Units[2].cls_SequencyTest.SwitchRelay3Way_Air_Water(false);
                    break;
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
    }
}
