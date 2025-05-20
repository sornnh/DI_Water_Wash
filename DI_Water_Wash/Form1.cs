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

namespace DI_Water_Wash
{
    public partial class Form1 : Form
    {
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
            ProductionDB.Initialize("10.102.4.20", "Parameters_SZ", "sa", "nuventixleo");
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
                StateCommon.LoadingText += "Config All unit for test...\r\n";
                InitializeUnitManager();
                tabControl3.SelectedIndexChanged += TabControl3_SelectedIndexChanged;
                tabControl3.SelectedIndex = 0;
                txt_SN_Lenght.Text = ClsUnitManagercs.cls_Units[0].SN_Length.ToString();
                txt_ControlString.Text = ClsUnitManagercs.cls_Units[0].PN_Ctl_String;
                txt_Leght.Text = ClsUnitManagercs.cls_Units[0].Ctl_Str_Length.ToString();
                txt_Offset.Text = ClsUnitManagercs.cls_Units[0].Ctl_Str_Offset.ToString();
                StateCommon.LoadingText = "Complete\r\n";
                StateCommon.LoadingValue = 100;
                StateCommon.bLoading = false;
            }
            else
            {
                System.Windows.Forms.Application.Exit();
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

        private void InitializeUnitManager()
        {
            string[] allLine = File.ReadAllLines(@"C:\Aavid_Test\Setup-ini\Flushing_Part_Numbers.txt");
            AssyPN = new string[allLine.Length-1];
            for (int i = 0; i < AssyPN.Length; i++)
            {
                string[] line = allLine[i+1].Split(',');
                AssyPN[i] = line[1].Trim();
            }
            ClsUnitManagercs.Initialize(AssyPN);

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
        delegate void Deltact();
        delegate void DelResource();
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
        }
    }
}
