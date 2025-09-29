using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

    public partial class UC_PressureDecay : UserControl
    {
        private int UnitIndex;
        public UC_PressureDecay( int unitIndex)
        {
            InitializeComponent();
            UnitIndex = unitIndex;
            InitializeDecayParameters();
        }

        private void InitializeDecayParameters()
        {
            txt_Max_Humidity.Text = ClsUnitManagercs.cls_Units.iDecayMaxHumidity.ToString();
            txt_Stabilize_Time.Text = ClsUnitManagercs.cls_Units.iDecayStabilize_Time.ToString();
            txt_Decay_Time.Text =   ClsUnitManagercs.cls_Units.iDecayTest_Time.ToString();
            txt_Pressure.Text = ClsUnitManagercs.cls_Units.iDecayPressure.ToString();
            txt_Pressuretolerance.Text = ClsUnitManagercs.cls_Units.iDecayPressureTolerance.ToString();
            txt_Max_dP.Text = ClsUnitManagercs.cls_Units.iDecayMax_dP.ToString();
            txt_Fill_Pressure.Text = ClsUnitManagercs.cls_Units.iN2FillPressure.ToString();
            txt_Fill_Pressure_Tolerance.Text = ClsUnitManagercs.cls_Units.iN2FillPressureTolerance.ToString();
            if(ClsUnitManagercs.cls_Units.bDecay_Pre_Drying)
                cBox_Pre_Drying.Checked = true;
            else
                cBox_Pre_Drying.Checked = false;
            if (ClsUnitManagercs.cls_Units.bN2Fill_to_Ship)
                cBox_Fill_N2_to_Ship.Checked = true;
            else
                cBox_Fill_N2_to_Ship.Checked = false;
        }

        private void UC_PressureDecay_Load(object sender, EventArgs e)
        {

        }

    }
