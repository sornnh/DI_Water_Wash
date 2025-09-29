using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

    public partial class UC_HeLeakage : UserControl
    {
        private int UnitIndex;
        public UC_HeLeakage(int unitIndex)
        {
            InitializeComponent();
            UnitIndex = unitIndex;
            InitializeDryingParameters();
        }

        private void InitializeDryingParameters()
        {
            txt_Pre_Vacuum.Text = ClsUnitManagercs.cls_Units.iPre_Vacuum.ToString();
            txt_Roughing_Time_On.Text = ClsUnitManagercs.cls_Units.iRoughing_Time_On.ToString();
            txt_Gross_Leak_Pressure.Text = ClsUnitManagercs.cls_Units.iGross_Leak_Pressure.ToString();
            txt_Helium_Valve_Open_Time.Text =   ClsUnitManagercs.cls_Units.iHelium_Valve_Open_Time.ToString();
            txt_Normal_Pressure.Text = ClsUnitManagercs.cls_Units.iNormal_pressure.ToString();
            txt_OpeningDelay.Text = ClsUnitManagercs.cls_Units.iOpeningDelay.ToString();
            txt_VentTime.Text = ClsUnitManagercs.cls_Units.iVentTime.ToString();
            txt_Leak_Test_Time.Text = ClsUnitManagercs.cls_Units.iLeak_Test_Time.ToString();
            txt_Min.Text = ClsUnitManagercs.cls_Units.iMin.ToString("E");
            txt_Max.Text =  ClsUnitManagercs.cls_Units.iMax.ToString("E");
            if (ClsUnitManagercs.cls_Units.bPre_Vacuum)
                cBox_Pre_Vacuum.Checked = true;
            else
                cBox_Pre_Vacuum.Checked = false;
            if (ClsUnitManagercs.cls_Units.bRoughing_Time_On)
                cBox_Roughing_Time_On.Checked = true;
            else
                cBox_Roughing_Time_On.Checked = false;
            if (ClsUnitManagercs.cls_Units.bVent_Valve_Control)
            {
                cBox_Automatic.Checked = true;
                cBox_Manual.Checked = false;
            }    
            else
            {
                cBox_Automatic.Checked = false;
                cBox_Manual.Checked = true;
            }    
        }
    }
