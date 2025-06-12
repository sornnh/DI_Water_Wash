using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hot_Air_Drying
{
    public partial class UC_PartNumberInfor : UserControl
    {
        private UC_DIWaterWash uc_DIWaterWash;
        private UC_Drying uc_Drying;
        private UC_HeLeakage uc_HeLeakage;
        private UC_PressureDecay uC_PressureDecay;
        private int _UnitIndex;
        private SecsionTest secsion;
        private Panel[] pl_UC = new Panel[4];
        public enum SecsionTest
        {
            DIWaterWash,
            Drying,
            HeLeakage
        }
        public UC_PartNumberInfor(int _Index, SecsionTest secsionTest)
        {
            _UnitIndex = _Index;
            InitializeComponent();
            secsion = secsionTest;
            pl_UC[0] = pl_UC1;
            pl_UC[1] = panel1;
            pl_UC[2] = panel2;
            pl_UC[3] = panel3;
        }
        public void InitUI()
        {
            GetDescreption();
            uc_DIWaterWash = new UC_DIWaterWash(_UnitIndex);
            uc_Drying = new UC_Drying(_UnitIndex);
            uc_HeLeakage = new UC_HeLeakage(_UnitIndex);
            uC_PressureDecay = new UC_PressureDecay(_UnitIndex);
            _UnitIndex = _UnitIndex;
            uc_DIWaterWash.Dock = DockStyle.Fill;
            uc_Drying.Dock = DockStyle.Fill;
            uc_HeLeakage.Dock = DockStyle.Fill;
            pl_UC1.Controls.Clear();
            SafeAddToPanel(pl_UC[0], uc_DIWaterWash);
            SafeAddToPanel(pl_UC[1], uc_Drying);
            SafeAddToPanel(pl_UC[2], uc_HeLeakage);
            SafeAddToPanel(pl_UC[3], uC_PressureDecay);
            //switch (secsion)
            //{
            //    case SecsionTest.DIWaterWash:
            //        pl_UC4.Controls.Add(uc_DIWaterWash);
            //        break;
            //    case SecsionTest.Drying:
            //        pl_UC4.Controls.Add(uc_Drying);
            //        break;
            //    case SecsionTest.HeLeakage:
            //        pl_UC4.Controls.Add(uc_HeLeakage);
            //        break;
            //}
        }

        private void GetDescreption()
        {
            Cls_DBMsSQL ParameterDB = new Cls_DBMsSQL();
            ParameterDB.Initialize("10.102.4.20", "Parameters_SZ", "sa", "nuventixleo");
            ParameterDB.Open();
            string PN = ClsUnitManagercs.cls_Units[_UnitIndex].AssyPN;
            string query = $"SELECT \r\nDescription\r\nFROM [Parameters_SZ].[dbo].[Aavid_Part_Numbers]\r\nWHERE [ASSY_PN] = '{PN}' order by Date_Time";
            DataTable dt = ParameterDB.ExecuteQuery(query);
            txtPN.Text = PN;
            if (dt.Rows.Count == 0) txtDescription.Text = "";
            else txtDescription.Text = dt.Rows[dt.Rows.Count-1][0].ToString();
            ParameterDB.Close();
        }

        void SafeAddToPanel(Panel targetPanel, Control control)
        {
            if (control.Parent != null)
                ((Panel)control.Parent).Controls.Remove(control);
            targetPanel.Enabled = true;
            targetPanel.Visible = true;
            control.Visible = true;
            targetPanel.Controls.Clear();  // Xóa trước nếu muốn chỉ có 1 control
            control.Dock = DockStyle.Fill;
            targetPanel.Controls.Add(control);
        }
        private void label2_Click(object sender, EventArgs e)
        {
            InitUI();
        }
    }
}
