using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

    public partial class UC_DIWaterWash : UserControl
    {
        private int UnitIndex;
        public UC_DIWaterWash(int unitIndex)
        {
            InitializeComponent();
            UnitIndex = unitIndex;
            // Initialize the drying parameters
            InitializeDryingParameters();
        }

        private void InitializeDryingParameters()
        {
            txt_Wash_Cycle.Text = ClsUnitManagercs.cls_Units.iWash_Cycle.ToString();
            txt_DI_Flow_Rate.Text = ClsUnitManagercs.cls_Units.iDi_Flow_Rate.ToString();
            txt_DI_Pre_Wash_Time.Text = ClsUnitManagercs.cls_Units.iPre_Washing_Time.ToString();
            txt_DI_Washing_Time.Text = ClsUnitManagercs.cls_Units.iWashing_Time.ToString();
            txt_DI_Reverse_Washing_Time.Text = ClsUnitManagercs.cls_Units.iDI_Reverse_Washing_Time.ToString();
            txt_DI_Drying_Time.Text = ClsUnitManagercs.cls_Units.iDI_Drying_Time.ToString();
            txt_DI_Reverse_Washing_Time.Text = ClsUnitManagercs.cls_Units.iReverse_DI_Drying_Time.ToString();
            txt_DI_Max_Humidity.Text = ClsUnitManagercs.cls_Units.iDI_Max_Huminity.ToString();
            if(ClsUnitManagercs.cls_Units.bReverse_Washing_Flow)
                cBox_Reverse_Washing_Flow.Checked = true;
            else
                cBox_Reverse_Washing_Flow.Checked = false;
            if (ClsUnitManagercs.cls_Units.bReverse_DI_Flushing_Flow)
                cBox_Reverse_DI_Flushing_Flow.Checked = true;
            else
                cBox_Reverse_DI_Flushing_Flow.Checked = false;
            if (ClsUnitManagercs.cls_Units.bCheck_DI_Huminity)
                cBox_Check_DI_Humidity.Checked = true;
            else
                cBox_Check_DI_Humidity.Checked = false;

        }
    }
