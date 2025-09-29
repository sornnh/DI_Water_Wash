using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            txt_Number_of_Drying_cycles.Text = ClsUnitManagercs.cls_Units.iNumber_of_Drying_Clycle.ToString();
            txt_Max_Humidity.Text = ClsUnitManagercs.cls_Units.iMax_Drying_Humidity.ToString();
            txt_Hot_Air_Flushing_Time.Text = ClsUnitManagercs.cls_Units.iHot_Air_Flushing_Time.ToString();
            txt_Hot_Air_Reverse_Flow_Time.Text = ClsUnitManagercs.cls_Units.iHot_Air_Reverse_Flow_Time.ToString();
            txt_N2_Pressure.Text = ClsUnitManagercs.cls_Units.iN2_Pressure.ToString();
            txt_N2_Drying_Time.Text = ClsUnitManagercs.cls_Units.iN2_Drying_Time.ToString();
            txt_Vacuum_Pressure.Text = ClsUnitManagercs.cls_Units.iVacuum_Pressure.ToString();
            txt_Vac_Baking_Temp.Text = ClsUnitManagercs.cls_Units.iVac_Baking_Temp.ToString();
            txt_Vac_Max_Humidity.Text = ClsUnitManagercs.cls_Units.iVac_Max_Humidity.ToString();
            txt_Baking_Time.Text = ClsUnitManagercs.cls_Units.iBaking_Time.ToString();

            if (ClsUnitManagercs.cls_Units.bCheck_Humidity)
                cBox_Check_Dry_Humidity.Checked = true;
            if(ClsUnitManagercs.cls_Units.bReverse_Hot_Air_Flushing_Flow)
                cBox_Reverse_Hot_Flushing_Flow.Checked = true;
            if (ClsUnitManagercs.cls_Units.bUse_Nitrogen_to_Dry)
                cBox_Use_Nitrogen_to_Dry.Checked = true;
        }
    }
