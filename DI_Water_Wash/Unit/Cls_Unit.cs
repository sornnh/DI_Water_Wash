using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DI_Water_Wash.Sequence;

namespace DI_Water_Wash
{
    public class Cls_Unit
    {
        public Cls_DBMsSQL ParameterDB = new Cls_DBMsSQL();

        public Cls_DBMsSQL ProductionDB = new Cls_DBMsSQL();

        private int _UnitIndex = 0;

        private string _AssyPN;
        private string _WO;
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

        public Cls_ASPcontrol cls_ASPcontrol { get; private set; }

        public ClsInverterModbus Cls_InverterModbus { get; private set; }

        public Cls_SequencyCommon cls_SequencyCommon { get; private set; }

        public Cls_SequencyTest cls_SequencyTest { get; private set; }

        public int UnitIndex => _UnitIndex;

        public string AssyPN => _AssyPN;
        public string WO => _WO;
        public string[] verifyFlags { get; private set; }

        public int SN_Length { get; private set; }

        public string PN_Ctl_String { get; private set; }

        public int Ctl_Str_Length { get; private set; }

        public int Ctl_Str_Offset { get; private set; }

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

        public Cls_Unit(int unitIndex, string assyPN,string Workoder , Cls_ASPcontrol cls_AS, ClsInverterModbus clsInverter)
        {
            try
            {
                _UnitIndex = unitIndex;
                _AssyPN = assyPN;
                _WO = Workoder;
                cls_ASPcontrol = cls_AS;
                Cls_InverterModbus = clsInverter;
                cls_SequencyCommon = new Cls_SequencyCommon(unitIndex, cls_ASPcontrol);
                Thread thrcommon = new Thread(funThreadCommon);
                thrcommon.IsBackground = true;
                thrcommon.Name = "ThreadCommon";
                thrcommon.Start();
                cls_SequencyTest = new Cls_SequencyTest(unitIndex, cls_ASPcontrol, Cls_InverterModbus, StateCommon.InverterType.ASP);
                Thread thrTest = new Thread(funThreadTest);
                thrTest.IsBackground = true;
                thrTest.Name = "ThreadCommon";
                thrTest.Start();
                cls_SequencyCommon.AutoMode = true;
                cls_SequencyCommon.process = StateCommon.ProcessState.Idle;
                ParameterDB.Initialize("10.102.4.20", "Parameters_SZ", "sa", "nuventixleo");
                ParameterDB.Open();
                ProductionDB.Initialize("10.102.4.20", "Production_SZ", "sa", "nuventixleo");
                ProductionDB.Open();
                GenerateProcessTesting();
                GetLimitTesting();
                GetSNVerify();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Initialize class Failed: " + ex.Message);
            }
        }

        private void funThreadCommon()
        {
            Thread.Sleep(5000);
            while (true)
            {
                cls_SequencyCommon.LoopTowerLamp();
                Thread.Sleep(500);
            }
        }
        private void funThreadTest()
        {
            Thread.Sleep(1000);
            while (true)
            {
                cls_SequencyTest.LoopTest();
                Thread.Sleep(500);
            }
        }

        private void GetLimitTesting()
        {
            string query = "SELECT *FROM dbo.Leak_Test_Limits Where Assy_PN ='" + _AssyPN + "' order by Date_Time";
            DataTable dt = ParameterDB.ExecuteQuery(query);
            GetLimit_DIWaterWash(dt);
            GetLimit_HeliumLeakageTest(dt);
            GetLimit_DryingTest(dt);
            GetLimit_DecayTest(dt);
        }

        private void GetLimit_DecayTest(DataTable dt)
        {
            DataRow dataRow = dt.Rows[dt.Rows.Count - 1];
            foreach (DataColumn column in dataRow.Table.Columns)
            {
                switch (column.ColumnName)
                {
                    case "PDc_Max_Humidity":
                        iDecayMaxHumidity = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Stabilize_Time":
                        iDecayStabilize_Time = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Decay_Time":
                        iDecayTest_Time = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Pressure_Decay_Test":
                        iDecayPressure = Convert.ToDouble(dataRow[column]);
                        break;
                    case "P_Tolerance":
                        iDecayPressureTolerance = Convert.ToDouble(dataRow[column]);
                        break;
                    case "Max_dP":
                        iDecayMax_dP = Convert.ToDouble(dataRow[column]);
                        break;
                    case "Pre_Drying":
                        bDecay_Pre_Drying = Convert.ToBoolean(dataRow[column]);
                        break;
                    case "Fill_N2_to_Ship":
                        bN2Fill_to_Ship = Convert.ToBoolean(dataRow[column]);
                        break;
                    case "Fill_Pressure":
                        iN2FillPressure = Convert.ToDouble(dataRow[column]);
                        break;
                    case "FP_Tolerance":
                        iN2FillPressureTolerance = Convert.ToDouble(dataRow[column]);
                        break;
                }
            }
        }

        private void GetLimit_DryingTest(DataTable dt)
        {
            DataRow dataRow = dt.Rows[dt.Rows.Count - 1];
            foreach (DataColumn column in dataRow.Table.Columns)
            {
                switch (column.ColumnName)
                {
                    case "Number_of_Drying_cycles":
                        iNumber_of_Drying_Clycle = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Hot_Air_Flushing_Time":
                        iHot_Air_Flushing_Time = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Hot_Air_Reverse_Flow_Time":
                        iHot_Air_Reverse_Flow_Time = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Max_Humidity":
                        iMax_Drying_Humidity = Convert.ToInt32(dataRow[column]);
                        break;
                    case "N2_Pressure":
                        iN2_Pressure = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Drying_Time":
                        iN2_Drying_Time = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Vacuum_Pressure":
                        iVacuum_Pressure = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Vac_Baking_Temp":
                        iVac_Baking_Temp = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Vac_Max_Humidity":
                        iVac_Max_Humidity = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Baking_Time":
                        iBaking_Time = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Reverse_Hot_Flushing_Flow":
                        bReverse_Hot_Air_Flushing_Flow = Convert.ToInt32(dataRow[column]) == 1;
                        break;
                    case "Use_Nitrogen_to_Dry":
                        bUse_Nitrogen_to_Dry = Convert.ToInt32(dataRow[column]) == 1;
                        break;
                    case "Check_Humidity":
                        bCheck_Humidity = Convert.ToInt32(dataRow[column]) == 1;
                        break;
                }
            }
        }

        private void GetLimit_HeliumLeakageTest(DataTable dt)
        {
            DataRow dataRow = dt.Rows[dt.Rows.Count - 1];
            foreach (DataColumn column in dataRow.Table.Columns)
            {
                switch (column.ColumnName)
                {
                    case "Pre_Vacuum":
                        bPre_Vacuum = Convert.ToInt32(dataRow[column]) == 1;
                        break;
                    case "Roughing_Time_On":
                        bRoughing_Time_On = Convert.ToInt32(dataRow[column]) == 1;
                        break;
                    case "Vent_Valve_Control":
                        bVent_Valve_Control = Convert.ToInt32(dataRow[column]) == 1;
                        break;
                    case "Vacuum_Time":
                        iPre_Vacuum = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Roughing_Time":
                        iRoughing_Time_On = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Gross_Leak_Pressure":
                        iGross_Leak_Pressure = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Helium_Valve_Open_Time":
                        iHelium_Valve_Open_Time = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Normal_Pressure":
                        iNormal_pressure = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Opening_Delay":
                        iOpeningDelay = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Vent_Minutes":
                    case "Vent_Seconds":
                        {
                            int num = (dataRow.Table.Columns.Contains("Vent_Minutes") ? Convert.ToInt32(dataRow["Vent_Minutes"]) : 0);
                            int num2 = (dataRow.Table.Columns.Contains("Vent_Seconds") ? Convert.ToInt32(dataRow["Vent_Seconds"]) : 0);
                            iVentTime = num * 60 + num2;
                            break;
                        }
                    case "Leak_Test_Time":
                        iLeak_Test_Time = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Reject_Threshold":
                        {
                            if (float.TryParse(dataRow[column]?.ToString(), out var result2))
                            {
                                iMax = result2;
                            }
                            break;
                        }
                    case "Min_Threshold":
                        {
                            if (float.TryParse(dataRow[column]?.ToString(), out var result))
                            {
                                iMin = result;
                            }
                            break;
                        }
                }
            }
        }

        private void GetLimit_DIWaterWash(DataTable dt)
        {
            DataRow dataRow = dt.Rows[dt.Rows.Count - 1];
            foreach (DataColumn column in dataRow.Table.Columns)
            {
                switch (column.ColumnName)
                {
                    case "Number_of_Wash_cycles":
                        iWash_Cycle = Convert.ToInt32(dataRow[column]);
                        break;
                    case "DI_Pre_Wash_Time":
                        iPre_Washing_Time = Convert.ToInt32(dataRow[column]);
                        break;
                    case "DI_Washing_Time":
                        iWashing_Time = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Reverse_Washing_Flow":
                        bReverse_Washing_Flow = Convert.ToInt32(dataRow[column]) == 1;
                        break;
                    case "DI_Reverse_Washing_Time":
                        iDI_Reverse_Washing_Time = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Reverse_DI_Flushing_Flow":
                        bReverse_DI_Flushing_Flow = Convert.ToInt32(dataRow[column]) == 1;
                        break;
                    case "DI_Drying_Time":
                        iDI_Drying_Time = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Reversed_DI_Drying_Time":
                        iReverse_DI_Drying_Time = Convert.ToInt32(dataRow[column]);
                        break;
                    case "DI_Flow_Rate":
                        iDi_Flow_Rate = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Check_DI_Humidity":
                        bCheck_DI_Huminity = Convert.ToInt32(dataRow[column]) == 1;
                        break;
                    case "DI_Max_Humidity":
                        iDI_Max_Huminity = Convert.ToInt32(dataRow[column]);
                        break;
                }
            }
        }

        private void GenerateProcessTesting()
        {
            string query = "SELECT * FROM Process_Flow_for_Part_Numbers WHERE Assy_PN = '" + _AssyPN + "' order by Date_Time";
            DataTable dt = ParameterDB.ExecuteQuery(query);
            LoadVerifyValues(dt);
        }

        public void LoadVerifyValues(DataTable dt)
        {
            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("No data found for the given Assy_PN.");
                verifyFlags = new string[1] { "" };
                return;
            }
            DataRow dataRow = dt.Rows[0];
            foreach (DataColumn column in dt.Columns)
            {
                string columnName = column.ColumnName;
                if (columnName == "Process_List")
                {
                    string[] array = dataRow[columnName].ToString().Split(',');
                    verifyFlags = new string[array.Length];
                    for (int i = 0; i < array.Length; i++)
                    {
                        verifyFlags[i] = array[i].Trim();
                    }
                }
            }
        }

        public void GetSNVerify()
        {
            string query = "SELECT \r\nSN_Length,\r\nPN_Ctl_String,\r\nCtl_Str_Length,\r\nCtl_Str_Offset\r\nFROM [Parameters_SZ].[dbo].[Aavid_Part_Numbers]\r\nWHERE [ASSY_PN] = '" + AssyPN + "' order by Date_Time";
            DataTable dataTable = ParameterDB.ExecuteQuery(query);
            DataRow dataRow = dataTable.Rows[dataTable.Rows.Count - 1];
            foreach (DataColumn column in dataRow.Table.Columns)
            {
                switch (column.ColumnName)
                {
                    case "SN_Length":
                        SN_Length = Convert.ToInt32(dataRow[column]);
                        break;
                    case "PN_Ctl_String":
                        PN_Ctl_String = dataRow[column].ToString();
                        break;
                    case "Ctl_Str_Length":
                        Ctl_Str_Length = Convert.ToInt32(dataRow[column]);
                        break;
                    case "Ctl_Str_Offset":
                        Ctl_Str_Offset = Convert.ToInt32(dataRow[column]);
                        break;
                }
            }
        }
    }

}
