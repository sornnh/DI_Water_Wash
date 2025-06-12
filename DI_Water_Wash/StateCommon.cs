using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hot_Air_Drying
{
    public class StateCommon
    {
        private delegate void AddText(string _text, RichTextBox richTextBox);

        public enum ProcessState
        {
            Idle,
            Running,
            CompletedPass,
            CompletedFail,
            Error
        }

        public enum InverterType
        {
            ASP,
            RS485
        }

        public static bool bLoading = false;

        public static string LoadingText = "";

        public static int LoadingValue = 0;

        public static List<string> DBQuery = new List<string>
    {
        "Verify_Fans_Test", "Verify_Serial_No", "Verify_High_Pressure", "Verify_DeltaT_Test", "Verify_Flow_Test", "Verify_Bubble_Test", "Verify_Thermal_Test", "Verify_Leak_Test", "Verify_Pressure_Dwell", "Verify_Flatness",
        "Verify_Humidity", "Verify_Final_Inspection", "Water_Filling", "Pressure_Leak_Test", "Sniffing_He_Leak", "Fill_N2_to_Ship", "Verify_PartN_Revisions", "HiPot_Test", "Reflow_Inspection", "Solder_Tin_Weight",
        "Hardw_Assembly", "Scan_Links_at_Thermal", "Verify_Links_at_Thermal", "Brazing_Log", "CMM", "CNC_Inspection", "Thread_bushing", "Washing_Drying", "Ultrasonic_Inspection", "Verify_QuickQ_Test",
        "LCP_DeltaT_Test", "LCP_Screw_Assembly", "ORing_Install", "Hose_Install", "Valve_Test", "Grease_Application", "Grease_Flash_Off", "FOD_Inspection", "Final_Motor_Test", "DI_Water_Wash",
        "Scan_Final_Inspection_at_Packing", "Zero_Setting", "Manifold_Test", "QA_Inspection", "AOI_Inspection", "CNC_1", "CMM_1", "CNC_2", "CMM_2", "CNC_3",
        "CMM_3", "CNC_4", "CMM_4"
    };

        public static Dictionary<string, string> ProcessValue = new Dictionary<string, string>();

        public static List<string> Processes = new List<string>
    {
        "CNC", "CNC_Inspection", "CMM", "Dimensions_Check", "Soldering", "PCBA_Inspection", "PCB_ICTest", "PCBA_Functional_Test", "Ultrasonic_Welding", "Induction_Welding",
        "Generic_Welding", "Straightening", "Platting", "Brazing", "Water_Filling", "Coolant_Filling", "Handle_Screw_Assembly", "Screw_Assembly", "O-Ring_Install", "Zero_Setting",
        "Manifold_Test", "Valve_Test", "Burn_in_Test", "Hose_Install", "Hose_Install_Victor", "Solder_Tin_Weight", "Bubble_Test", "Ultrasonic_Inspection", "Washing", "DI_Water_Wash",
        "Drying", "Vacuum_Drying", "Hot_Air_N2_Drying", "Pressure_Decay", "Delta_T", "Quick_Q", "Reflow_Inspection", "Thread_Bushing", "Hardware_Assembly", "Flow_Test",
        "Thermal_Resistance_Test", "LCP_DeltaT_Test", "Sniffing_Test", "Pre_Vacuum", "Helium_Leak_Test", "Pressure_Leak_Test", "Flatness_Test", "Grease_Application", "Grease_Flash-Off", "Grease_Flash-Off-Extended",
        "FOD_Inspection", "Pressure_Dwell", "HiPot_Test", "Pneuma_Flow_Test", "Final_Motor_Test", "Final_Test", "AOI_Inspection", "Final_Inspection", "Fill_N2_to_Ship", "QA_Inspection",
        "QC_Inspection", "MRB_Rework", "Packing"
    };

        public static List<string> DBProcesses = new List<string>
    {
        "CNC_1", "CMM_1", "CNC_2", "CMM_2", "CNC_3", "CMM_3", "CNC_4", "CMM_4", "Brazing", "Water_Filling",
        "Screw_Assembly", "Handle_Screw_Assembly", "O-Ring_Install", "Zero_Setting", "Manifold_Test", "Valve_Test", "Hose_Install", "Hose_Install_Victor", "Solder_Tin_Weight", "CMM_measurement",
        "Bubble_Test", "Untrasonic_Inspection", "CNC_Inspection", "Washing_Drying", "Pressure_Decay", "Delta_T", "Reflow_Inspection", "Thread_Bushing", "Hardware_Assembly", "Thermal_Resistance_Test",
        "LCP_DeltaT_Test", "Flow_Test", "DI_Water_Wash", "Hot_Air_N2 Drying", "Sniffing_Test", "He_Leak_Test", "Pressure_Leak_Test", "Flatness", "Grease_Application", "Grease_Flash-Off",
        "Grease_Flash-Off-Extended", "FOD_Inspection", "QA_Inspection", "Pressure_Dwell", "HiPot_Test", "Final_Motor_Test", "AOI_Inspection", "Final_Inspection", "Fill_N2_to_Ship", "Packing",
        "Any_QC_Inspection", "MRB_Rework"
    };

        public static void AddTextToRichTextBox(string _text, RichTextBox richTextBox)
        {
            if (richTextBox.InvokeRequired)
            {
                AddText method = AddTextToRichTextBox;
                richTextBox.Invoke(method, _text, richTextBox);
            }
            else
            {
                richTextBox.AppendText(_text + "\n");
                richTextBox.ScrollToCaret();
            }
        }

        public static bool CheckFileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public static void GetTableName(string Process, out string TableName, out string SummaryTableName)
        {
            TableName = "";
            SummaryTableName = "";
            switch (Process)
            {
                case "CNC":
                    TableName = "CNC_Inspection_Log";
                    break;
                case "CNC_Inspection":
                    TableName = "CNC_Inspection_Log";
                    break;
                case "CMM":
                    TableName = "CMM_Inspection_Log";
                    break;
                case "Dimensions_Check":
                    TableName = "Production_Flow_All_Months_Log";
                    break;
                case "Soldering":
                    TableName = "Soldering_Log";
                    break;
                case "PCBA_Inspection":
                    TableName = "PCBA_Inspection_Log";
                    break;
                case "PCB_ICTest":
                    TableName = "Production_Flow_All_Months_Log";
                    break;
                case "PCBA_Functional_Test":
                    TableName = "Production_Flow_All_Months_Log";
                    break;
                case "Ultrasonic_Welding":
                    TableName = "Ultrasonic_Welding_log";
                    break;
                case "Induction_Welding":
                    TableName = "Induction_Welding_Log";
                    break;
                case "Generic_Welding":
                    TableName = "Generic_Welding_Log";
                    break;
                case "Straightening":
                    TableName = "Straightening_Log";
                    break;
                case "Platting":
                    TableName = "Platting_Log";
                    break;
                case "Flattening":
                    TableName = "Flattening_Log";
                    break;
                case "Brazing":
                    TableName = "Aavid_Brazing_Log";
                    break;
                case "Water_Filling":
                    TableName = "Vapor_Chamber_Water_Filling";
                    SummaryTableName = "Aavid_Water_Filling_Hourly_Summary";
                    break;
                case "Coolant_Filling":
                    TableName = "Coolant_Filling_Log";
                    break;
                case "Handle_Screw_Assembly":
                    TableName = "Production_Flow_All_Months_Log";
                    break;
                case "Screw_Assembly":
                    TableName = "LCS_Plates_Assembly_Log";
                    break;
                case "O-Ring_Install":
                    TableName = "LCS_ORing_Install_Log";
                    break;
                case "Zero_Setting":
                    TableName = "Valve_Zero_Setting_Log";
                    break;
                case "Manifold_Test":
                    TableName = "Manifold_Test_Log";
                    break;
                case "Valve_Test":
                    TableName = "Valve_Functional_Test";
                    break;
                case "Burn_in_Test":
                    TableName = "Burn_in_Test_Log";
                    break;
                case "Hose_Install_Victor":
                    TableName = "Production_Flow_All_Months_Log";
                    break;
                case "Solder_Tin_Weight":
                    TableName = "Solder_Paste_Tin_Weight_Control";
                    break;
                case "Bubble_Test":
                    SummaryTableName = "Aavid_Bubble_Leak_Test_Hourly_Summary";
                    TableName = "Bubble_Leak_Test_Log";
                    break;
                case "Ultrasonic_Inspection":
                    TableName = "Aavid_LCS_Ultrasonic_Inspection_Log_Station";
                    break;
                case "Washing":
                    TableName = "Washing_Drying_Manual_Log";
                    break;
                case "DI_Water_Wash":
                    TableName = "DI_Water_Wash_Log";
                    break;
                case "Drying":
                    TableName = "Drying_Log";
                    break;
                case "Vacuum_Drying":
                    TableName = "Production_Flow_All_Months_Log";
                    break;
                case "Hot_Air_N2_Drying":
                    SummaryTableName = "Aavid_Humidity_Test_Hourly_Summary";
                    TableName = "LCS_Drying_Humidity_Log";
                    break;
                case "Hot_Air_N2 Drying":
                    SummaryTableName = "Aavid_Humidity_Test_Hourly_Summary";
                    TableName = "LCS_Drying_Humidity_Log";
                    break;
                case "Pressure_Decay":
                    SummaryTableName = "Aavid_Pressure_Decay_Test_Hourly_Summary";
                    TableName = "LCS_Pressure_and_Sniffing_Test_Log";
                    break;
                case "Delta_T":
                    SummaryTableName = "Aavid_Thermal_Test_Hourly_Summary";
                    TableName = "LCS_DT_Test_Data";
                    break;
                case "Quick_Q":
                    SummaryTableName = "Aavid_Vapor_Chamber_QuickQ_Hourly_Summary";
                    TableName = "Quick_Q";
                    break;
                case "Reflow_Inspection":
                    TableName = "Reflow_Inspection_Log";
                    break;
                case "Thread_Bushing":
                    TableName = "Thread_Bushing_Log";
                    break;
                case "Hardware_Assembly":
                    TableName = "Hardware_Assembly_Log";
                    break;
                case "Flow_Test":
                    SummaryTableName = "Aavid_Flow_Test_Hourly_Summary";
                    TableName = "LCS_Flow_Test_Data";
                    break;
                case "Thermal_Resistance_Test":
                    SummaryTableName = "Aavid_Thermal_Test_Hourly_Summary";
                    TableName = "LCS_Thermal_Test_Data";
                    break;
                case "LCP_DeltaT_Test":
                    SummaryTableName = "Aavid_Thermal_Test_Hourly_Summary";
                    TableName = "LCS_DT_Test_Data";
                    break;
                case "Sniffing_Test":
                    SummaryTableName = "Aavid_Sniffing_Leak_Test_Hourly_Summary";
                    TableName = "Sniffing_He_Leak_Test_Log";
                    break;
                case "Pre_Vacuum":
                    SummaryTableName = "";
                    TableName = "Pre_Vacuum_Log";
                    break;
                case "Helium_Leak_Test":
                    SummaryTableName = "Aavid_Leak_Test_Hourly_Summary";
                    TableName = "Leak_Test_Data";
                    break;
                case "Pressure_Leak_Test":
                    SummaryTableName = "Aavid_Pressure_Leak_Test_Hourly_Summary";
                    TableName = "Pressure_Leak_Test_Log";
                    break;
                case "Flatness_Test":
                    SummaryTableName = "Aavid_Flatness_Test_Hourly_Summary";
                    TableName = "LCS_Flatness_Test";
                    break;
                case "Grease_Application":
                    SummaryTableName = "";
                    TableName = "LCS_Grease_Application";
                    break;
                case "Grease_Flash-Off":
                    SummaryTableName = "";
                    TableName = "LCS_Grease_Flash_Off";
                    break;
                case "Grease_Flash-Off-Extended":
                    SummaryTableName = "";
                    TableName = "Production_Flow_All_Months_Log";
                    break;
                case "FOD_Inspection":
                    SummaryTableName = "";
                    TableName = "LCS_FOD_Inspection";
                    break;
                case "Pressure_Dwell":
                    SummaryTableName = "Aavid_Pressure_Dwell_Test_Hourly_Summary";
                    TableName = "LCS_Pressure_Dwell_Log";
                    break;
                case "HiPot_Test":
                    SummaryTableName = "";
                    TableName = "HiPot_Test_Log";
                    break;
                case "Pneuma_Flow_Test":
                    SummaryTableName = "";
                    TableName = "LCS_Pneuma_Flow_Test_Data";
                    break;
                case "Final_Motor_Test":
                    SummaryTableName = "";
                    TableName = "Aavid_LCS_Final_Test_Station";
                    break;
                case "Final_Test":
                    SummaryTableName = "";
                    TableName = "General_Final_Test_Log";
                    break;
                case "AOI_Inspection":
                    SummaryTableName = "";
                    TableName = "Aavid_AOI_Inspection_Log_Station";
                    break;
                case "Final_Inspection":
                    SummaryTableName = "";
                    TableName = "Aavid_LCS_Inspection_Log_Station";
                    break;
                case "Fill_N2_to_Ship":
                    SummaryTableName = "LCS_Nitrogen_Filling_Log_Summary";
                    TableName = "LCS_Nitrogen_Filling_Log";
                    break;
                case "QA_Inspection":
                    SummaryTableName = "";
                    TableName = "QA_Inspection_Log";
                    break;
                case "QC_Inspection":
                    SummaryTableName = "";
                    TableName = "QC_Inspection_Log";
                    break;
                case "MRB_Rework":
                    SummaryTableName = "";
                    TableName = "Aavid_MRB_Rework_Log_Station";
                    break;
                case "Packing":
                    SummaryTableName = "";
                    TableName = "LCS_BoxID_SN_XRef";
                    break;
            }
        }

        public static string ConvertDBtoProcess(string processName)
        {
            string result = "";
            switch (processName)
            {
                case "CNC_1":
                    result = "CNC";
                    break;
                case "CMM_1":
                    result = "CMM";
                    break;
                case "CNC_2":
                    result = "CNC";
                    break;
                case "CMM_2":
                    result = "CMM";
                    break;
                case "CNC_3":
                    result = "CNC";
                    break;
                case "CMM_3":
                    result = "CMM";
                    break;
                case "CNC_4":
                    result = "CNC";
                    break;
                case "CMM_4":
                    result = "CMM";
                    break;
                case "Brazing":
                    result = "Brazing";
                    break;
                case "Water_Filling":
                    result = "Water_Filling";
                    break;
                case "Screw_Assembly":
                    result = "Screw_Assembly";
                    break;
                case "Handle_Screw_Assembly":
                    result = "Handle_Screw_Assembly";
                    break;
                case "O-Ring_Install":
                    result = "O-Ring_Install";
                    break;
                case "Zero_Setting":
                    result = "Zero_Setting";
                    break;
                case "Manifold_Test":
                    result = "Manifold_Test";
                    break;
                case "Valve_Test":
                    result = "Valve_Test";
                    break;
                case "Hose_Install":
                    result = "Hose_Install";
                    break;
                case "Hose_Install_Victor":
                    result = "Hose_Install_Victor";
                    break;
                case "Solder_Tin_Weight":
                    result = "Solder_Tin_Weight";
                    break;
                case "CMM_measurement":
                    result = "CMM_measurement";
                    break;
                case "Bubble_Test":
                    result = "Bubble_Test";
                    break;
                case "Untrasonic_Inspection":
                    result = "Ultrasonic_Inspection";
                    break;
                case "CNC_Inspection":
                    result = "CNC_Inspection";
                    break;
                case "Washing_Drying":
                    result = "Washing";
                    break;
                case "Pressure_Decay":
                    result = "Pressure_Decay";
                    break;
                case "Delta_T":
                    result = "Delta_T";
                    break;
                case "Reflow_Inspection":
                    result = "Reflow_Inspection";
                    break;
                case "Thread_Bushing":
                    result = "Thread_Bushing";
                    break;
                case "Hardware_Assembly":
                    result = "Hardware_Assembly";
                    break;
                case "Thermal_Resistance_Test":
                    result = "Thermal_Resistance_Test";
                    break;
                case "LCP_DeltaT_Test":
                    result = "LCP_DeltaT_Test";
                    break;
                case "Flow_Test":
                    result = "Flow_Test";
                    break;
                case "DI_Water_Wash":
                    result = "DI_Water_Wash";
                    break;
                case "Hot_Air_N2 Drying":
                    result = "Hot_Air_N2_Drying";
                    break;
                case "Sniffing_Test":
                    result = "Sniffing_Test";
                    break;
                case "He_Leak_Test":
                    result = "Helium_Leak_Test";
                    break;
                case "Pressure_Leak_Test":
                    result = "Pressure_Leak_Test";
                    break;
                case "Flatness":
                    result = "Flatness_Test";
                    break;
                case "Grease_Application":
                    result = "Grease_Application";
                    break;
                case "Grease_Flash-Off":
                    result = "Grease_Flash-Off";
                    break;
                case "Grease_Flash-Off-Extended":
                    result = "Grease_Flash-Off-Extended";
                    break;
                case "FOD_Inspection":
                    result = "FOD_Inspection";
                    break;
                case "QA_Inspection":
                    result = "QA_Inspection";
                    break;
                case "Pressure_Dwell":
                    result = "Pressure_Dwell";
                    break;
                case "HiPot_Test":
                    result = "HiPot_Test";
                    break;
                case "Final_Motor_Test":
                    result = "Final_Motor_Test";
                    break;
                case "AOI_Inspection":
                    result = "AOI_Inspection";
                    break;
                case "Final_Inspection":
                    result = "Final_Inspection";
                    break;
                case "Fill_N2_to_Ship":
                    result = "Fill_N2_to_Ship";
                    break;
                case "Packing":
                    result = "Packing";
                    break;
                case "Any_QC_Inspection":
                    result = "QC_Inspection";
                    break;
                case "MRB_Rework":
                    result = "MRB_Rework";
                    break;
            }
            return result;
        }
    }
}
