using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DI_Water_Wash
{
    public class Cls_Unit
    {
        public bool Verify_Serial_No { get; private set; }
        public bool Verify_Thermal_Test { get; private set; }
        public bool Verify_Fans_Test { get; private set; }
        public bool Verify_Flatness { get; private set; }
        public bool Verify_Leak_Test { get; private set; }
        public bool Verify_Pressure_Dwell { get; private set; }
        public bool Verify_DeltaT_Test { get; private set; }
        public bool Verify_Bubble_Test { get; private set; }
        public bool Verify_PartN_Revisions { get; private set; }
        public bool Verify_High_Pressure { get; private set; }
        public bool Verify_Flow_Test { get; private set; }
        public bool Verify_Humidity { get; private set; }
        public bool Verify_Final_Inspection { get; private set; }
        public bool Verify_QuickQ_Test { get; private set; }
        public bool Verify_Links_at_Thermal { get; private set; }
        public Cls_DBMsSQL ParameterDB = new Cls_DBMsSQL();
        public Cls_DBMsSQL ProductionDB = new Cls_DBMsSQL();
        private int _UnitIndex = 0;
        private string _AssyPN;
        public int UnitIndex { get { return _UnitIndex; } }
        public string AssyPN {  get { return _AssyPN; } }
        public string[] verifyFlags { get; private set; }
        #region SNVerify
        public int SN_Length { get; private set; }
        public string PN_Ctl_String { get; private set; }
        public int Ctl_Str_Length { get; private set; }
        public int Ctl_Str_Offset { get; private set; }
        #endregion


        #region DIWaterWash
        public int iWash_Cycle { get; private set; }

        public int iPre_Washing_Time { get; private set; }
        public int iWashing_Time { get; private set; }

        public bool bReverse_Washing_Flow { get; private set; }
        public int iDI_Reverse_Washing_Time { get; set; }
        
        public bool bReverse_DI_Flushing_Flow { get; private set; }
        public int iDI_Drying_Time { get; set; }
        public int iReverse_DI_Drying_Time { get; set; }
        public int iDi_Flow_Rate { get; private set; }
        public bool bCheck_DI_Huminity { get; private set; }
        public int iDI_Max_Huminity { get; private set; }
        #endregion


        #region HeliumLeakage
        public int iPre_Vacuum { get; private set; }
        public int iRoughing_Time_On { get; private set; }
        public int iGross_Leak_Pressure { get; private set; }
        public int iHelium_Valve_Open_Time { get; private set; }
        public int iNormal_pressure { get; private set; }
        public int iOpeningDelay { get; private set; }
        public int iVentTime { get; private set; }
        public int iLeak_Test_Time { get; private set; }
        public float iMin { get; private set; }
        public float iMax { get; private set; }
        public bool bPre_Vacuum { get; private set; }
        public bool bRoughing_Time_On { get; private set; }
        public bool bVent_Valve_Control { get; private set; }

        #endregion


        #region Drying
        public int iNumber_of_Drying_Clycle { get; private set; }
        public int iHot_Air_Flushing_Time { get; private set; }
        public int iHot_Air_Reverse_Flow_Time { get; private set; }
        public int iMax_Drying_Humidity { get; private set; }
        public int iN2_Pressure { get; private set; }
        public int iN2_Drying_Time { get; private set; }
        public int iVacuum_Pressure { get; private set; }
        public int iVac_Baking_Temp { get; private set; }
        public int iVac_Max_Humidity { get; private set; }
        public int iBaking_Time { get; private set; }
        public bool bReverse_Hot_Air_Flushing_Flow { get; private set; }
        public bool bUse_Nitrogen_to_Dry { get; private set; }
        public bool bCheck_Humidity { get; private set; }
        #endregion

        #region PressureDecay
        public int iDecayMaxHumidity { get; private set; }
        public int iDecayStabilize_Time { get; private set; }
        public int iDecayTest_Time { get; private set; }
        public double iDecayPressure { get; private set; }
        public double iDecayPressureTolerance { get; private set; }
        public double iDecayMax_dP { get; private set; }
        public bool bDecay_Pre_Drying { get; private set; }
        public bool bN2Fill_to_Ship { get; private set; }
        public double iN2FillPressure { get; private set; }
        public double iN2FillPressureTolerance { get; private set; }
        #endregion
        public Cls_Unit(int unitIndex, string assyPN)
        {
            _UnitIndex = unitIndex;
            _AssyPN = assyPN; 
            ParameterDB.Initialize("10.102.4.20", "Parameters_SZ", "sa", "nuventixleo");
            ParameterDB.Open();
            ProductionDB.Initialize("10.102.4.20", "Parameters_SZ", "sa", "nuventixleo");
            ProductionDB.Open();
            GenerateProcessTesting();
            GetLimitTesting();

            GetSNVerify();
        }
        private void GetLimitTesting()
        {
            string query = $"SELECT *FROM dbo.Leak_Test_Limits Where Assy_PN ='{_AssyPN}' order by Date_Time";
            DataTable dt = ParameterDB.ExecuteQuery(query);
            GetLimit_DIWaterWash(dt);
            GetLimit_HeliumLeakageTest(dt);
            GetLimit_DryingTest(dt);
            GetLimit_DecayTest(dt);
        }

        private void GetLimit_DecayTest(DataTable dt)
        {
            DataRow row = dt.Rows[dt.Rows.Count - 1];
            foreach (DataColumn column in row.Table.Columns)
            {
                string columnName = column.ColumnName;

                switch (columnName)
                {
                    case "PDc_Max_Humidity":
                        iDecayMaxHumidity = Convert.ToInt32(row[column]);
                        break;
                    case "Stabilize_Time":
                        iDecayStabilize_Time = Convert.ToInt32(row[column]);
                        break;
                    case "Decay_Time":
                        iDecayTest_Time = Convert.ToInt32(row[column]);
                        break;
                    case "Pressure_Decay_Test":
                        iDecayPressure = Convert.ToDouble(row[column]);
                        break;
                    case "P_Tolerance":
                        iDecayPressureTolerance = Convert.ToDouble(row[column]);
                        break;
                    case "Max_dP":
                        iDecayMax_dP = Convert.ToDouble(row[column]);
                        break;
                    case "Pre_Drying":
                        bDecay_Pre_Drying = Convert.ToBoolean(row[column]);
                        break;
                    case "Fill_N2_to_Ship":
                        bN2Fill_to_Ship = Convert.ToBoolean(row[column]);
                        break;
                    case "Fill_Pressure":
                        iN2FillPressure = Convert.ToDouble(row[column]);
                        break;
                    case "FP_Tolerance":
                        iN2FillPressureTolerance = Convert.ToDouble(row[column]);
                        break;
                }
            }
        }

        private void GetLimit_DryingTest(DataTable dt)
        {
            DataRow row = dt.Rows[dt.Rows.Count - 1];
            foreach (DataColumn col in row.Table.Columns)
            {
                switch (col.ColumnName)
                {
                    case "Number_of_Drying_cycles":
                        iNumber_of_Drying_Clycle = Convert.ToInt32(row[col]);
                        break;
                    case "Hot_Air_Flushing_Time":
                        iHot_Air_Flushing_Time = Convert.ToInt32(row[col]);
                        break;
                    case "Hot_Air_Reverse_Flow_Time":
                        iHot_Air_Reverse_Flow_Time = Convert.ToInt32(row[col]);
                        break;
                    case "Max_Humidity":
                        iMax_Drying_Humidity = Convert.ToInt32(row[col]);
                        break;
                    case "N2_Pressure":
                        iN2_Pressure = Convert.ToInt32(row[col]);
                        break;
                    case "Drying_Time": // nếu có cột riêng "N2_Drying_Time" thì đổi lại tên
                        iN2_Drying_Time = Convert.ToInt32(row[col]);
                        break;
                    case "Vacuum_Pressure":
                        iVacuum_Pressure = Convert.ToInt32(row[col]);
                        break;
                    case "Vac_Baking_Temp":
                        iVac_Baking_Temp = Convert.ToInt32(row[col]);
                        break;
                    case "Vac_Max_Humidity":
                        iVac_Max_Humidity = Convert.ToInt32(row[col]);
                        break;
                    case "Baking_Time":
                        iBaking_Time = Convert.ToInt32(row[col]);
                        break;
                    case "Reverse_Hot_Flushing_Flow":
                        bReverse_Hot_Air_Flushing_Flow = Convert.ToInt32(row[col]) == 1;
                        break;
                    case "Use_Nitrogen_to_Dry":
                        bUse_Nitrogen_to_Dry = Convert.ToInt32(row[col]) == 1;
                        break;
                    case "Check_Humidity": // hoặc "Check_Humidity" nếu cột tên khác
                        bCheck_Humidity = Convert.ToInt32(row[col]) == 1;
                        break;
                }
            }
        }

        private void GetLimit_HeliumLeakageTest(DataTable dt)
        {
            DataRow row = dt.Rows[dt.Rows.Count - 1];
            foreach (DataColumn column in row.Table.Columns)
            {
                string columnName = column.ColumnName;

                switch (columnName)
                {
                    case "Pre_Vacuum":
                        bPre_Vacuum = Convert.ToInt32(row[column]) == 1;
                        break;

                    case "Roughing_Time_On":
                        bRoughing_Time_On = Convert.ToInt32(row[column]) == 1;
                        break;

                    case "Vent_Valve_Control":
                        bVent_Valve_Control = Convert.ToInt32(row[column]) == 1;
                        break;
                    case "Vacuum_Time": // nếu là số thay vì bool
                        iPre_Vacuum = Convert.ToInt32(row[column]);
                        break;

                    case "Roughing_Time":
                        iRoughing_Time_On = Convert.ToInt32(row[column]);
                        break;

                    case "Gross_Leak_Pressure":
                        iGross_Leak_Pressure = Convert.ToInt32(row[column]);
                        break;

                    case "Helium_Valve_Open_Time":
                        iHelium_Valve_Open_Time = Convert.ToInt32(row[column]);
                        break;

                    case "Normal_Pressure":
                        iNormal_pressure = Convert.ToInt32(row[column]);
                        break;

                    case "Opening_Delay":
                        iOpeningDelay = Convert.ToInt32(row[column]);
                        break;

                    case "Vent_Minutes":
                    case "Vent_Seconds":
                        int minutes = row.Table.Columns.Contains("Vent_Minutes") ? Convert.ToInt32(row["Vent_Minutes"]) : 0;
                        int seconds = row.Table.Columns.Contains("Vent_Seconds") ? Convert.ToInt32(row["Vent_Seconds"]) : 0;
                        iVentTime = minutes * 60 + seconds;
                        break;

                    case "Leak_Test_Time":
                        iLeak_Test_Time = Convert.ToInt32(row[column]);
                        break;

                    case "Reject_Threshold":
                        if (float.TryParse(row[column]?.ToString(), out float maxThres))
                            iMax = maxThres;
                        break;
                    case "Min_Threshold":
                        if (float.TryParse(row[column]?.ToString(), out float minThres))
                            iMin = minThres;
                        break;
                }
            }
        }
        private void GetLimit_DIWaterWash(DataTable dt)
        {
            DataRow row = dt.Rows[dt.Rows.Count-1];
            foreach (DataColumn column in row.Table.Columns)
            {
                string columnName = column.ColumnName;

                switch (columnName)
                {
                    case "Number_of_Wash_cycles":
                        iWash_Cycle = Convert.ToInt32(row[column]);
                        break;

                    case "DI_Pre_Wash_Time":
                        iPre_Washing_Time = Convert.ToInt32(row[column]);
                        break;

                    case "DI_Washing_Time":
                        iWashing_Time = Convert.ToInt32(row[column]);
                        break;

                    case "Reverse_Washing_Flow":
                        bReverse_Washing_Flow = Convert.ToInt32(row[column]) == 1;
                        break;

                    case "DI_Reverse_Washing_Time":
                        iDI_Reverse_Washing_Time = Convert.ToInt32(row[column]);
                        break;

                    case "Reverse_DI_Flushing_Flow":
                        bReverse_DI_Flushing_Flow = Convert.ToInt32(row[column]) == 1;
                        break;

                    case "DI_Drying_Time":
                        iDI_Drying_Time = Convert.ToInt32(row[column]);
                        break;

                    case "Reversed_DI_Drying_Time":
                        iReverse_DI_Drying_Time = Convert.ToInt32(row[column]);
                        break;

                    case "DI_Flow_Rate":
                        iDi_Flow_Rate = Convert.ToInt32(row[column]);
                        break;

                    case "Check_DI_Humidity":
                        bCheck_DI_Huminity = Convert.ToInt32(row[column]) == 1;
                        break;

                    case "DI_Max_Humidity":
                        iDI_Max_Huminity = Convert.ToInt32(row[column]);
                        break;
                }
            }
        }
        private void GenerateProcessTesting()
        {
            string query = $"SELECT * FROM Process_Flow_for_Part_Numbers WHERE Assy_PN = '{_AssyPN}' order by Date_Time";
            //MessageBox.Show(query);
            DataTable dt = ParameterDB.ExecuteQuery(query);
            LoadVerifyValues(dt);
        }

        public void LoadVerifyValues(DataTable dt)
        {
            DataRow row = dt.Rows[0];
            foreach (DataColumn col in dt.Columns)
            {
                string key = col.ColumnName;
                if (key == "Process_List")
                {
                    string[] processList = row[key].ToString().Split(',');
                    verifyFlags = new string[processList.Length];
                    for (int i = 0; i < processList.Length; i++)
                    {
                        verifyFlags[i] = processList[i].Trim();
                    }
                }
            }
        }


        public void GetSNVerify()
        {
            string query = $"SELECT \r\nSN_Length,\r\nPN_Ctl_String,\r\nCtl_Str_Length,\r\nCtl_Str_Offset\r\nFROM [Parameters_SZ].[dbo].[Aavid_Part_Numbers]\r\nWHERE [ASSY_PN] = '{AssyPN}' order by Date_Time";
            DataTable dt = ParameterDB.ExecuteQuery(query);
            //GeneratePNVerify
            DataRow row = dt.Rows[dt.Rows.Count - 1];
            foreach (DataColumn column in row.Table.Columns)
            {
                string columnName = column.ColumnName;

                switch (columnName)
                {
                    case "SN_Length":
                        SN_Length = Convert.ToInt32(row[column]);
                        break;
                    case "PN_Ctl_String":
                        PN_Ctl_String = row[column].ToString();
                        break;
                    case "Ctl_Str_Length":
                        Ctl_Str_Length = Convert.ToInt32(row[column]);
                        break;
                    case "Ctl_Str_Offset":
                        Ctl_Str_Offset = Convert.ToInt32(row[column]);
                        break;
                }
            }
        }
    }
}
