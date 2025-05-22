using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DI_Water_Wash.Unit
{
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
            txt_Max_Humidity.Text = ClsUnitManagercs.cls_Units[UnitIndex].iDecayMaxHumidity.ToString();
            txt_Stabilize_Time.Text = ClsUnitManagercs.cls_Units[UnitIndex].iDecayStabilize_Time.ToString();
            txt_Decay_Time.Text = ClsUnitManagercs.cls_Units[UnitIndex].iDecayTest_Time.ToString();
            txt_Pressure.Text = ClsUnitManagercs.cls_Units[UnitIndex].iDecayPressure.ToString();
            txt_Fill_Pressure_Tolerance.Text = ClsUnitManagercs.cls_Units[UnitIndex].iDecayPressureTolerance.ToString();
            txt_Max_dP.Text = ClsUnitManagercs.cls_Units[UnitIndex].iDecayMax_dP.ToString();
            txt_Fill_Pressure.Text = ClsUnitManagercs.cls_Units[UnitIndex].iN2FillPressure.ToString();
            txt_Fill_Pressure_Tolerance.Text = ClsUnitManagercs.cls_Units[UnitIndex].iN2FillPressureTolerance.ToString();
            if(ClsUnitManagercs.cls_Units[UnitIndex].bDecay_Pre_Drying)
                cBox_Pre_Drying.Checked = true;
            else
                cBox_Pre_Drying.Checked = false;
            if (ClsUnitManagercs.cls_Units[UnitIndex].bN2Fill_to_Ship)
                cBox_Fill_N2_to_Ship.Checked = true;
            else
                cBox_Fill_N2_to_Ship.Checked = false;
        }

        private void UC_PressureDecay_Load(object sender, EventArgs e)
        {

        }

    }
}
