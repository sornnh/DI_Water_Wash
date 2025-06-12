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

namespace Hot_Air_Drying
{
    public partial class UC_MaintTest : UserControl
    {
        public int UnitIndex { get; private set; }
        public UC_MaintTest(int index)
        {
            InitializeComponent();
            UnitIndex = index;
            lb_Fixture.Text = $"Fixture {index + 1}";
            lb_PN.Text = ClsUnitManagercs.cls_Units[UnitIndex].AssyPN;
            lb_Status.Text= "Ready for test";
            lb_TotalCycle.Text= ClsUnitManagercs.cls_Units[UnitIndex].iWash_Cycle.ToString();
            timer1.Start();
            pictureBox1.Image = Image.FromFile(@"panel.jpg");
        }
        public void funAddLog_Auto(string _text)
        {
            if (!richTextBox1.Created) return;
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.BeginInvoke(new MethodInvoker(delegate {
                    richTextBox1.AppendText(_text + "\n");
                    richTextBox1.ScrollToCaret();
                }));
            }
            else
            {
                richTextBox1.AppendText(_text + "\n");
                richTextBox1.ScrollToCaret();
            }
        }
        public void SetSNForTest()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => SetSNForTest()));
                return;
            }
            txt_SN.Enabled = true;
            txt_SN.Text = "";
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
        //private void txt_SN_KeyDown(object sender, KeyEventArgs e)
        //{
        //    string SN = txt_SN.Text.Trim();
        //    if(SN.Length == ClsUnitManagercs.cls_Units[UnitIndex].SN_Length )
        //    {
        //        if (txt_SN.Text.Length == 0 )
        //        {
        //            MessageBox.Show("Please input the serial number.");
        //            return;
        //        }
        //        // Assuming there's a method to handle the test with the provided serial number
        //        ClsUnitManagercs.cls_Units[UnitIndex].cls_SequencyTest.SN = txt_SN.Text.Trim();
        //        txt_SN.Enabled = false;
        //    }
        //}
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
                    try
                    {
                        if (label == lb_CurrentTime && int.TryParse(lb_CurrentTime.Text, out int currentTime) && int.TryParse(lb_TimeStep.Text, out int timeStep) && timeStep > 0)
                        {
                            int percen = currentTime * 100 / timeStep;
                            UpdatePercent(percen + 1);
                        }
                    }
                    catch { }
                    
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
        private void UpdatePercent(int value)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => UpdatePercent(value)));
                return;
            }
            else
                progressBar1.Value = value;
        }
        private void UpdatePercentTotal(int value)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => UpdatePercent(value)));
                return;
            }
            else
                procTesting.Value = value;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            switch(ClsUnitManagercs.cls_Units[UnitIndex].cls_SequencyTest.testSeq)
            {
                case Cls_SequencyTest.TestSeq.WAIT:
                    txt_SN.Enabled = false;
                    Updatelabel("Ready for test", lb_Status);
                    break;
                case Cls_SequencyTest.TestSeq.SN_INSERT:
                    txt_SN.Enabled = true;
                    txt_SN.Focus();
                    Updatelabel("Wait for insert SN", lb_Status);
                    break;
                case Cls_SequencyTest.TestSeq.SN_CHECK_ROUTER:
                    txt_SN.Enabled = false;
                    Updatelabel("Checking Router", lb_Status);
                    break;
                case Cls_SequencyTest.TestSeq.SN_CHECK_ROUTER_OK:
                    txt_SN.Enabled = false;
                    Updatelabel("Start test",lb_Status);
                    break;
                case Cls_SequencyTest.TestSeq.ERROR:
                    txt_SN.Enabled=false;
                    Updatelabel("ERROR", lb_Status);
                    Updatelabel("ERROR", lb_Result);
                    break;
                case Cls_SequencyTest.TestSeq.PRE_WASHING:
                    Updatelabel((ClsUnitManagercs.cls_Units[UnitIndex].cls_SequencyTest.StepTesting+1).ToString(), lb_CurentCycle);
                    Updatelabel("Pre Washing", lb_CycleTest);
                    Updatelabel(ClsUnitManagercs.cls_Units[UnitIndex].iPre_Washing_Time.ToString(), lb_TimeStep);
                    Updatelabel((ClsUnitManagercs.cls_Units[UnitIndex].cls_SequencyTest.sw_prewash.ElapsedMilliseconds / 1000).ToString("0"), lb_CurrentTime);
                    UpdatePercentTotal(10);
                    break;
                case Cls_SequencyTest.TestSeq.FLUSHING_WASHING:
                    UpdatePercentTotal(40);
                    Updatelabel("Washing", lb_CycleTest);
                    Updatelabel(ClsUnitManagercs.cls_Units[UnitIndex].iWashing_Time.ToString(), lb_TimeStep);
                    Updatelabel((ClsUnitManagercs.cls_Units[UnitIndex].cls_SequencyTest.sw_flush.ElapsedMilliseconds / 1000).ToString("0"), lb_CurrentTime);
                    break;
                case Cls_SequencyTest.TestSeq.REVERSE_WASHING:
                    Updatelabel("Reverse Washing", lb_CycleTest);
                    Updatelabel(ClsUnitManagercs.cls_Units[UnitIndex].iWashing_Time.ToString(), lb_TimeStep);
                    Updatelabel((ClsUnitManagercs.cls_Units[UnitIndex].cls_SequencyTest.sw_reverse.ElapsedMilliseconds / 1000).ToString("0"), lb_CurrentTime);
                    break;
                case Cls_SequencyTest.TestSeq.DRYING:
                    UpdatePercentTotal(70);
                    Updatelabel("Drying", lb_CycleTest);
                    Updatelabel(ClsUnitManagercs.cls_Units[UnitIndex].iDI_Drying_Time.ToString(), lb_TimeStep);
                    Updatelabel((ClsUnitManagercs.cls_Units[UnitIndex].cls_SequencyTest.sw_drying.ElapsedMilliseconds / 1000).ToString("0"), lb_CurrentTime);
                    break;
                case Cls_SequencyTest.TestSeq.REVERSE_DRYING:
                    Updatelabel("Reverse Drying", lb_CycleTest);
                    Updatelabel(ClsUnitManagercs.cls_Units[UnitIndex].iReverse_DI_Drying_Time.ToString(), lb_TimeStep);
                    Updatelabel((ClsUnitManagercs.cls_Units[UnitIndex].cls_SequencyTest.sw_reverse_drying.ElapsedMilliseconds / 1000).ToString("0"), lb_CurrentTime);
                    UpdatePercentTotal(100);
                    break;
                default:
                    txt_SN.Enabled = false;
                    Updatelabel("Testing", lb_Status);
                    
                    break;
            }
            switch (ClsUnitManagercs.cls_Units[UnitIndex].cls_SequencyCommon.process)
            {
                case StateCommon.ProcessState.Idle:
                    Updatelabel("Idle", lb_StatusMachine);
                    lb_StatusMachine.BackColor =Color.DarkOrange;
                    break;
                case StateCommon.ProcessState.Running:
                    Updatelabel("Running", lb_StatusMachine);
                    lb_StatusMachine.BackColor = Color.Yellow;
                    UpdatelabelwithColor("P/F", Color.Yellow, lb_Result);
                    break;
                case StateCommon.ProcessState.CompletedPass:
                    Updatelabel("Pass", lb_StatusMachine);
                    lb_StatusMachine.BackColor = Color.Green;
                    Updatelabel("Pass", lb_Result);
                    lb_Result.BackColor = Color.Green;
                    break;
                case StateCommon.ProcessState.CompletedFail:
                    Updatelabel("Fail", lb_StatusMachine);
                    lb_StatusMachine.BackColor = Color.Red;
                    Updatelabel("Fail", lb_Result);
                    lb_Result.BackColor = Color.Red;
                    break;
                case StateCommon.ProcessState.Error:
                    Updatelabel("Error", lb_StatusMachine);
                    lb_StatusMachine.BackColor = Color.Red;
                    UpdatelabelwithColor("ERROR", Color.Red, lb_Result);
                    break;
            }
        }

        private void txt_SN_TextChanged(object sender, EventArgs e)
        {
            string SN = txt_SN.Text.Trim();

            // Kiểm tra nếu độ dài của SN đã đủ
            if (SN.Length == ClsUnitManagercs.cls_Units[UnitIndex].SN_Length)
            {
                if (txt_SN.Text.Length == 0)
                {
                    MessageBox.Show("Please input the serial number.");
                    return;
                }
                // Nếu đủ độ dài, thực hiện hành động cần thiết
                ClsUnitManagercs.cls_Units[UnitIndex].cls_SequencyTest.SN = txt_SN.Text.Trim();
                txt_SN.Enabled = false;  // Vô hiệu hóa TextBox sau khi nhập đủ SN
            }
        }
    }
}
