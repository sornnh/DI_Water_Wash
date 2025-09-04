using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DI_Water_Wash
{
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
            txt_Wash_Cycle.Text = ClsUnitManagercs.cls_Units[UnitIndex].iWash_Cycle.ToString();
            txt_DI_Flow_Rate.Text = ClsUnitManagercs.cls_Units[UnitIndex].dDi_Flow_Rate.ToString();
            txt_Flow_Tol.Text = ClsUnitManagercs.cls_Units[UnitIndex].dDi_Flow_Tol.ToString();
            txt_DI_Water_Presseure_Max.Text = ClsUnitManagercs.cls_Units[UnitIndex].dDI_Max_WaterPressure.ToString();
            txt_DI_Water_Presseure_Min.Text = ClsUnitManagercs.cls_Units[UnitIndex].dDI_Min_WaterPressure.ToString();
            txt_DI_Air_Presseure_Max.Text = ClsUnitManagercs.cls_Units[UnitIndex].dDI_Max_AirPressure.ToString();
            txt_DI_Air_Presseure_Min.Text = ClsUnitManagercs.cls_Units[UnitIndex].dDI_Min_AirPressure.ToString();
            txt_DI_Pre_Wash_Time.Text = ClsUnitManagercs.cls_Units[UnitIndex].iPre_Washing_Time.ToString();
            txt_DI_Washing_Time.Text = ClsUnitManagercs.cls_Units[UnitIndex].iWashing_Time.ToString();
            txt_DI_Reverse_Washing_Time.Text = ClsUnitManagercs.cls_Units[UnitIndex].iDI_Reverse_Washing_Time.ToString();
            txt_DI_Drying_Time.Text = ClsUnitManagercs.cls_Units[UnitIndex].iDI_Drying_Time.ToString();
            txt_DI_Reverse_Washing_Time.Text = ClsUnitManagercs.cls_Units[UnitIndex].iReverse_DI_Drying_Time.ToString();
            txt_DI_Max_Humidity.Text = ClsUnitManagercs.cls_Units[UnitIndex].iDI_Max_Huminity.ToString();
            if(ClsUnitManagercs.cls_Units[UnitIndex].bReverse_Washing_Flow)
                cBox_Reverse_Washing_Flow.Checked = true;
            else
                cBox_Reverse_Washing_Flow.Checked = false;
            if (ClsUnitManagercs.cls_Units[UnitIndex].bReverse_DI_Flushing_Flow)
                cBox_Reverse_DI_Flushing_Flow.Checked = true;
            else
                cBox_Reverse_DI_Flushing_Flow.Checked = false;
            if (ClsUnitManagercs.cls_Units[UnitIndex].bCheck_DI_Huminity)
                cBox_Check_DI_Humidity.Checked = true;
            else
                cBox_Check_DI_Humidity.Checked = false;

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label21_Click(object sender, EventArgs e)
        {

        }
    }
}
