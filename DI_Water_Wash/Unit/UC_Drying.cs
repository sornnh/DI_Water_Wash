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
    public partial class UC_Drying : UserControl
    {
        private int UnitIndex;
        public UC_Drying(int unitIndex)
        {
            InitializeComponent();
            UnitIndex = unitIndex;
            // Initialize the drying parameters
            InitializeDryingParameters();
        }

        private void InitializeDryingParameters()
        {
            txt_Number_of_Drying_cycles.Text = ClsUnitManagercs.cls_Units[UnitIndex].iNumber_of_Drying_Clycle.ToString();
            txt_Max_Humidity.Text = ClsUnitManagercs.cls_Units[UnitIndex].iMax_Drying_Humidity.ToString();
            txt_Hot_Air_Flushing_Time.Text = ClsUnitManagercs.cls_Units[UnitIndex].iHot_Air_Flushing_Time.ToString();
            txt_Hot_Air_Reverse_Flow_Time.Text = ClsUnitManagercs.cls_Units[UnitIndex].iHot_Air_Reverse_Flow_Time.ToString();
            txt_N2_Pressure.Text = ClsUnitManagercs.cls_Units[UnitIndex].iN2_Pressure.ToString();
            txt_N2_Drying_Time.Text = ClsUnitManagercs.cls_Units[UnitIndex].iN2_Drying_Time.ToString();
            txt_Vacuum_Pressure.Text = ClsUnitManagercs.cls_Units[UnitIndex].iVacuum_Pressure.ToString();
            txt_Vac_Baking_Temp.Text = ClsUnitManagercs.cls_Units[UnitIndex].iVac_Baking_Temp.ToString();
            txt_Vac_Max_Humidity.Text = ClsUnitManagercs.cls_Units[UnitIndex].iVac_Max_Humidity.ToString();
            txt_Baking_Time.Text = ClsUnitManagercs.cls_Units[UnitIndex].iBaking_Time.ToString();

            if (ClsUnitManagercs.cls_Units[UnitIndex].bCheck_Humidity)
                cBox_Check_Dry_Humidity.Checked = true;
            if(ClsUnitManagercs.cls_Units[UnitIndex].bReverse_Hot_Air_Flushing_Flow)
                cBox_Reverse_Hot_Flushing_Flow.Checked = true;
            if (ClsUnitManagercs.cls_Units[UnitIndex].bUse_Nitrogen_to_Dry)
                cBox_Use_Nitrogen_to_Dry.Checked = true;
        }
    }
}
