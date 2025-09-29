using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

public partial class UC_MaintTest : UserControl
{
    public int UnitIndex { get; private set; }
    public string CurrentErrorCode { get; set; } = "";
    public UC_MaintTest(int index)
    {
        InitializeComponent();
        UnitIndex = index;
        lb_Fixture.Text = $"Fixture {index + 1}";
        lb_PN.Text = ClsUnitManagercs.cls_Units.AssyPN;
        lb_Status.Text= "Ready for test";
        Update_lbErrorCode("");
        timer1.Start();
    }
    public void SetSNForTest()
    {
        if (this.InvokeRequired)
        {
            this.BeginInvoke(new Action(() => SetSNForTest()));
            return;
        }
        txt_SN.Enabled = true;
    }
    public void UpdateData(DataTable dt)
    {
        if (this.InvokeRequired)
        {
            this.BeginInvoke(new Action(() => UpdateData(dt)));
            return;
        }
        else
        {
            dgv_process.SuspendLayout(); // Tạm ngừng layout để tránh flicker
            dgv_process.ReadOnly = true; // Không cho sửa
            dgv_process.AllowUserToAddRows = false; // Không cho thêm dòng trống cuối
            dgv_process.AllowUserToDeleteRows = false; // Không cho xoá
            dgv_process.AutoGenerateColumns = true; // Tự tạo cột theo DataTable
            dgv_process.Columns.Clear(); // Xoá cột dư nếu có
            dgv_process.DataSource = dt;   // Gán nguồn dữ liệu mới
            // ❌ Không cho sort
            foreach (DataGridViewColumn col in dgv_process.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }     
    }
    private void Updatelabel(string value,Label label)
    {
        try
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => Updatelabel(value, label)));
                return;
            }
            else
            {
                label.Text = value;
            }
        }
        catch { }
    }
    private void UpdatelabelwithColor(string value, Color color, Label label)
    {
        try
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => UpdatelabelwithColor(value,color, label)));
                return;
            }
            else
            {
                label.Text = value;
                label.BackColor = color;
            }
        }
        catch { }
    }
    private void UpdatePercentTotal(int value)
    {
        if (this.InvokeRequired)
        {
            this.BeginInvoke(new Action(() => UpdatePercentTotal(value)));
            return;
        }
        else
        {
            if (value > 100)
            {
                value = 100;
            }
            procTesting.Value = value;
        }
    }
    private void timer1_Tick(object sender, EventArgs e)
    {
        switch(ClsUnitManagercs.cls_Units.cls_SequencyTest.testSeq)
        {
            case Cls_SequencyTest.TestSeq.WAIT:
                txt_SN.Enabled = false;
                Updatelabel("Ready for test", lb_Status);
                lb_Status.BackColor = Color.White;
                UpdatePercentTotal(0);
                break;
            case Cls_SequencyTest.TestSeq.SN_INSERT_1:
                if(UnitIndex == 0)
                {
                    txt_SN.Enabled = true;
                    txt_SN.Focus();
                    Updatelabel("Wait for insert SN", lb_Status);
                    lb_Status.BackColor=Color.Yellow;
                }
                break;
            case Cls_SequencyTest.TestSeq.SN_INSERT_2:
                if (UnitIndex == 1)
                {
                    txt_SN.Enabled = true;
                    txt_SN.Focus();
                    Updatelabel("Wait for insert SN", lb_Status);
                    lb_Status.BackColor = Color.Yellow;
                }
                break;
            case Cls_SequencyTest.TestSeq.SN_INSERT_3:
                if (UnitIndex == 2)
                {
                    txt_SN.Enabled = true;
                    txt_SN.Focus();
                    Updatelabel("Wait for insert SN", lb_Status);
                    lb_Status.BackColor = Color.Yellow;
                }
                break;
            case Cls_SequencyTest.TestSeq.SN_INSERT_4:
                if (UnitIndex == 3)
                {
                    txt_SN.Enabled = true;
                    txt_SN.Focus();
                    Updatelabel("Wait for insert SN", lb_Status);
                    lb_Status.BackColor = Color.Yellow;
                }
                break;
            case Cls_SequencyTest.TestSeq.SN_CHECK_ROUTER:
                switch(UnitIndex)
                {
                    case 0:
                        txt_SN.Text = ClsUnitManagercs.cls_Units.cls_SequencyTest.SN1;
                        break;
                    case 1:
                        txt_SN.Text = ClsUnitManagercs.cls_Units.cls_SequencyTest.SN2;
                        break;
                    case 2:
                        txt_SN.Text = ClsUnitManagercs.cls_Units.cls_SequencyTest.SN3;
                        break;
                    case 3:
                        txt_SN.Text = ClsUnitManagercs.cls_Units.cls_SequencyTest.SN4;
                        break;
                }    
                txt_SN.Enabled = false;
                Updatelabel("Checking Router", lb_Status);
                lb_Status.BackColor = Color.Yellow;
                break;
            case Cls_SequencyTest.TestSeq.SN_CHECK_ROUTER_OK:
                txt_SN.Enabled = false;
                Updatelabel("Start test",lb_Status);
                lb_Status.BackColor = Color.Yellow;
                break;
            case Cls_SequencyTest.TestSeq.ERROR:
                txt_SN.Enabled=false;
                Updatelabel("ERROR", lb_Status);
                Updatelabel("ERROR", lb_Result);
                lb_Status.BackColor = Color.Red;
                break;
            default:
                txt_SN.Enabled = false;
                Updatelabel("Testing", lb_Status);
                lb_Status.BackColor = Color.Yellow;
                int totalTesttime = (ClsUnitManagercs.cls_Units.iDecayStabilize_Time + ClsUnitManagercs.cls_Units.iDecayTest_Time) * 1000;
                double elapsed = ClsUnitManagercs.cls_Units.cls_SequencyTest.sw_decayTest.ElapsedMilliseconds;
                Updatelabel((elapsed/1000).ToString("000"), lb_TimeStep);
                int percent = (int)(elapsed * 100 / totalTesttime);
                UpdatePercentTotal(percent);
                break;
        }
        switch (ClsUnitManagercs.cls_Units.cls_SequencyCommon.process)
        {
            case StateCommon.ProcessState.Idle:
                Updatelabel("Idle", lb_StatusMachine);
                lb_StatusMachine.BackColor =Color.DarkOrange;
                break;
            case StateCommon.ProcessState.Running:
                Updatelabel("Running", lb_StatusMachine);
                lb_StatusMachine.BackColor = Color.Yellow;
                UpdatelabelwithColor("P/F", Color.Yellow, lb_Result);
                clearAlldata();
                break;
            case StateCommon.ProcessState.CompletedPass:
                Updatelabel("Pass", lb_StatusMachine);
                lb_StatusMachine.BackColor = Color.Green;
                Updatelabel("Pass", lb_Result);
                lb_Result.BackColor = Color.Green;
                UpdateTestData();
                break;
            case StateCommon.ProcessState.CompletedFail:
                Updatelabel("Fail", lb_StatusMachine);
                lb_StatusMachine.BackColor = Color.Red;
                Updatelabel("Fail", lb_Result);
                lb_Result.BackColor = Color.Red;
                UpdateTestData();
                break;
            case StateCommon.ProcessState.Error:
                Updatelabel("Error", lb_StatusMachine);
                lb_StatusMachine.BackColor = Color.Red;
                UpdatelabelwithColor("ERROR", Color.Red, lb_Result);
                break;
        }
        if(ClsUnitManagercs.cls_Units.cls_SequencyTest.sErrorCode != CurrentErrorCode)
        {
            CurrentErrorCode = ClsUnitManagercs.cls_Units.cls_SequencyTest.sErrorCode;
            Update_lbErrorCode(CurrentErrorCode);
        }    
    }
    private void Update_lbErrorCode(string value)
    {
        
        if (this.InvokeRequired)
        {
            this.BeginInvoke(new Action(() => Update_lbErrorCode(value)));
            return;
        }
        else
        {
            if (value == "")
            {
                lb_ErrorMess.Text = "No Error";
                lb_ErrorMess.BackColor = Color.LightGreen;
            }
            else
            {
                lb_ErrorMess.Text = value;
                lb_ErrorMess.BackColor = Color.Red;
            }
        }
    }
    private void UpdateTestData()
    {
        if (ClsUnitManagercs.cls_Units.Cls_LSR902.result is LeakTestResultM mResult)
        {
            double PressureTime = mResult.Pressure_Time;
            double BalanceTime = mResult.Balance2_Time;
            double HoldTime = mResult.Balance1_Time;
            double DectectTime = mResult.Detection_Time;
            double TestPressure = mResult.TestPressure;
            double Leak = mResult.Leak;
            Updatelabel(PressureTime.ToString("0.000"), lb_PressureFillTime);
            Updatelabel(HoldTime.ToString("0.000"), lb_PressureHoldTime);
            Updatelabel(BalanceTime.ToString("0.000"), lb_PressureBalanceTime);
            Updatelabel(DectectTime.ToString("0.000"), lb_PressureDetectTime);
            Updatelabel(TestPressure.ToString("0.000") , lb_Pressure);
            Updatelabel(Leak.ToString("0.000"), lb_Leakage);
        }
    }
    private void clearAlldata()
    {
        Updatelabel("0", lb_PressureFillTime);
        Updatelabel("0", lb_PressureHoldTime);
        Updatelabel("0", lb_PressureBalanceTime);
        Updatelabel("0", lb_PressureDetectTime);
        Updatelabel("0", lb_Pressure);
        Updatelabel("0", lb_Leakage);
    }
    private void txt_SN_TextChanged(object sender, EventArgs e)
    {
        string SN = txt_SN.Text.Trim();

        // Kiểm tra nếu độ dài của SN đã đủ
        if (SN.Length == ClsUnitManagercs.cls_Units.SN_Length)
        {
            if (txt_SN.Text.Length == 0)
            {
                MessageBox.Show("Please input the serial number.");
                return;
            }
            switch(UnitIndex)
                {
                case 0:
                    ClsUnitManagercs.cls_Units.cls_SequencyTest.SN1 = txt_SN.Text.Trim();
                    break;
                case 1:
                    ClsUnitManagercs.cls_Units.cls_SequencyTest.SN2 = txt_SN.Text.Trim();
                    break;
                case 2:
                    ClsUnitManagercs.cls_Units.cls_SequencyTest.SN3 = txt_SN.Text.Trim();
                    break;
                case 3:
                    ClsUnitManagercs.cls_Units.cls_SequencyTest.SN4 = txt_SN.Text.Trim();
                    break;
            }
            lb_OldSN.Text = txt_SN.Text.Trim(); // Lưu SN cũ để so sánh
            txt_SN.Text = ""; // Xoá nội dung TextBox
            txt_SN.Enabled = false;  // Vô hiệu hóa TextBox sau khi nhập đủ SN
        }
    }

    private void label3_Click(object sender, EventArgs e)
    {

    }
}

