using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DI_Water_Wash
{
    public partial class StateCommon
    {
        public static bool bLoading  = false;
        public static string LoadingText = "";
        public static int LoadingValue = 0;
        private delegate void AddText(string _text, RichTextBox richTextBox);
        public static void AddTextToRichTextBox(string _text, RichTextBox richTextBox)
        {
            if (richTextBox.InvokeRequired)
            {
                AddText addText = new AddText(AddTextToRichTextBox);
                richTextBox.Invoke(addText, _text, richTextBox);
            }
            else
            {
                richTextBox.AppendText(_text + "\n");
                richTextBox.ScrollToCaret();
            }
        }
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
        public static bool CheckFileExists(string filePath)
        {
            return File.Exists(filePath);
        }
        public static List<string> DBQuery = new List<string>
        {
            "Verify_Fans_Test",
            "Verify_Serial_No",
            "Verify_High_Pressure",
            "Verify_DeltaT_Test",
            "Verify_Flow_Test",
            "Verify_Bubble_Test",
            "Verify_Thermal_Test",
            "Verify_Leak_Test",
            "Verify_Pressure_Dwell",
            "Verify_Flatness",
            "Verify_Humidity",
            "Verify_Final_Inspection",
            "Water_Filling",
            "Pressure_Leak_Test",
            "Sniffing_He_Leak",
            "Fill_N2_to_Ship",
            "Verify_PartN_Revisions",
            "HiPot_Test",
            "Reflow_Inspection",
            "Solder_Tin_Weight",
            "Hardw_Assembly",
            "Scan_Links_at_Thermal",
            "Verify_Links_at_Thermal",
            "Brazing_Log",
            "CMM",
            "CNC_Inspection",
            "Thread_bushing",
            "Washing_Drying",
            "Ultrasonic_Inspection",
            "Verify_QuickQ_Test",
            "LCP_DeltaT_Test",
            "LCP_Screw_Assembly",
            "ORing_Install",
            "Hose_Install",
            "Valve_Test",
            "Grease_Application",
            "Grease_Flash_Off",
            "FOD_Inspection",
            "Final_Motor_Test",
            "DI_Water_Wash",
            "Scan_Final_Inspection_at_Packing",
            "Zero_Setting",
            "Manifold_Test",
            "QA_Inspection",
            "AOI_Inspection",
            "CNC_1",
            "CMM_1",
            "CNC_2",
            "CMM_2",
            "CNC_3",
            "CMM_3",
            "CNC_4",
            "CMM_4",
        };

        public static Dictionary<string, string> ProcessValue = new Dictionary<string, string> { };

        public static List<string> Processes = new List<string>
        {
            "CNC",
            "CNC_Inspection",
            "CMM",
            "Dimensions_Check",
            "Soldering",
            "PCBA_Inspection",
            "PCB_ICTest",
            "PCBA_Functional_Test",
            "Ultrasonic_Welding",
            "Induction_Welding",
            "Generic_Welding",
            "Straightening",
            "Platting",
            "Brazing",
            "Water_Filling",
            "Coolant_Filling",
            "Handle_Screw_Assembly",
            "Screw_Assembly",
            "O-Ring_Install",
            "Zero_Setting",
            "Manifold_Test",
            "Valve_Test",
            "Burn_in_Test",
            "Hose_Install",
            "Hose_Install_Victor",
            "Solder_Tin_Weight",
            "Bubble_Test",
            "Ultrasonic_Inspection",
            "Washing",
            "DI_Water_Wash",
            "Drying",
            "Vacuum_Drying",
            "Hot_Air_N2_Drying",
            "Pressure_Decay",
            "Delta_T",
            "Quick_Q",
            "Reflow_Inspection",
            "Thread_Bushing",
            "Hardware_Assembly",
            "Flow_Test",
            "Thermal_Resistance_Test",
            "LCP_DeltaT_Test",
            "Sniffing_Test",
            "Pre_Vacuum",
            "Helium_Leak_Test",
            "Pressure_Leak_Test",
            "Flatness_Test",
            "Grease_Application",
            "Grease_Flash-Off",
            "Grease_Flash-Off-Extended",
            "FOD_Inspection",
            "Pressure_Dwell",
            "HiPot_Test",
            "Pneuma_Flow_Test",
            "Final_Motor_Test",
            "Final_Test",
            "AOI_Inspection",
            "Final_Inspection",
            "Fill_N2_to_Ship",
            "QA_Inspection",
            "QC_Inspection",
            "MRB_Rework",
            "Packing",
        };
        public static List<string> DBProcesses = new List<string>
        {
            "CNC_1",
            "CMM_1",
            "CNC_2",
            "CMM_2",
            "CNC_3",
            "CMM_3",
            "CNC_4",
            "CMM_4",
            "Brazing",
            "Water_Filling",
            "Screw_Assembly",
            "Handle_Screw_Assembly",
            "O-Ring_Install",
            "Zero_Setting",
            "Manifold_Test",
            "Valve_Test",
            "Hose_Install",
            "Hose_Install_Victor",
            "Solder_Tin_Weight",
            "CMM_measurement",
            "Bubble_Test",
            "Untrasonic_Inspection",
            "CNC_Inspection",
            "Washing_Drying",
            "Pressure_Decay",
            "Delta_T",
            "Reflow_Inspection",
            "Thread_Bushing",
            "Hardware_Assembly",
            "Thermal_Resistance_Test",
            "LCP_DeltaT_Test",
            "Flow_Test",
            "DI_Water_Wash",
            "Hot_Air_N2 Drying",
            "Sniffing_Test",
            "He_Leak_Test",
            "Pressure_Leak_Test",
            "Flatness",
            "Grease_Application",
            "Grease_Flash-Off",
            "Grease_Flash-Off-Extended",
            "FOD_Inspection",
            "QA_Inspection",
            "Pressure_Dwell",
            "HiPot_Test",
            "Final_Motor_Test",
            "AOI_Inspection",
            "Final_Inspection",
            "Fill_N2_to_Ship",
            "Packing",
            "Any_QC_Inspection",
            "MRB_Rework",
        };
        public static string ConvertDBtoProcess(string processName)
        {
            string process = "";
            switch (processName)
            {
                case "CNC_1":
                    process = "CNC";
                    break;
                case "CMM_1":
                    process = "CMM";
                    break;
                case "CNC_2":
                    process = "CNC";
                    break;
                case "CMM_2":
                    process = "CMM";
                    break;
                case "CNC_3":
                    process = "CNC";
                    break;
                case "CMM_3":
                    process = "CMM";
                    break;
                case "CNC_4":
                    process = "CNC";
                    break;
                case "CMM_4":
                    process = "CMM";
                    break;
                case "Brazing":
                    process = "Brazing";
                    break;
                case "Water_Filling":
                    process = "Water_Filling";
                    break;
                case "Screw_Assembly":
                    process = "Screw_Assembly";
                    break;
                case "Handle_Screw_Assembly":
                    process = "Handle_Screw_Assembly";
                    break;
                case "O-Ring_Install":
                    process = "O-Ring_Install";
                    break;
                case "Zero_Setting":
                    process = "Zero_Setting";
                    break;
                case "Manifold_Test":
                    process = "Manifold_Test";
                    break;
                case "Valve_Test":
                    process = "Valve_Test";
                    break;
                case "Hose_Install":
                    process = "Hose_Install";
                    break;
                case "Hose_Install_Victor":
                    process = "Hose_Install_Victor";
                    break;
                case "Solder_Tin_Weight":
                    process = "Solder_Tin_Weight";
                    break;
                case "CMM_measurement":
                    process = "CMM_measurement";
                    break;
                case "Bubble_Test":
                    process = "Bubble_Test";
                    break;
                case "Untrasonic_Inspection":
                    process = "Ultrasonic_Inspection";
                    break;
                case "CNC_Inspection":
                    process = "CNC_Inspection";
                    break;
                case "Washing_Drying":
                    process = "Washing";
                    break;
                case "Pressure_Decay":
                    process = "Pressure_Decay";
                    break;
                case "Delta_T":
                    process = "Delta_T";
                    break;
                case "Reflow_Inspection":
                    process = "Reflow_Inspection";
                    break;
                case "Thread_Bushing":
                    process = "Thread_Bushing";
                    break;
                case "Hardware_Assembly":
                    process = "Hardware_Assembly";
                    break;
                case "Thermal_Resistance_Test":
                    process = "Thermal_Resistance_Test";
                    break;
                case "LCP_DeltaT_Test":
                    process = "LCP_DeltaT_Test";
                    break;
                case "Flow_Test":
                    process = "Flow_Test";
                    break;
                case "DI_Water_Wash":
                    process = "DI_Water_Wash";
                    break;
                case "Hot_Air_N2 Drying":
                    process = "Hot_Air_N2_Drying";
                    break;
                case "Sniffing_Test":
                    process = "Sniffing_Test";
                    break;
                case "He_Leak_Test":
                    process = "Helium_Leak_Test";
                    break;
                case "Pressure_Leak_Test":
                    process = "Pressure_Leak_Test";
                    break;
                case "Flatness":
                    process = "Flatness_Test";
                    break;
                case "Grease_Application":
                    process = "Grease_Application";
                    break;
                case "Grease_Flash-Off":
                    process = "Grease_Flash-Off";
                    break;
                case "Grease_Flash-Off-Extended":
                    process = "Grease_Flash-Off-Extended";
                    break;
                case "FOD_Inspection":
                    process = "FOD_Inspection";
                    break;
                case "QA_Inspection":
                    process = "QA_Inspection";
                    break;
                case "Pressure_Dwell":
                    process = "Pressure_Dwell";
                    break;
                case "HiPot_Test":
                    process = "HiPot_Test";
                    break;
                case "Final_Motor_Test":
                    process = "Final_Motor_Test";
                    break;
                case "AOI_Inspection":
                    process = "AOI_Inspection";
                    break;
                case "Final_Inspection":
                    process = "Final_Inspection";
                    break;
                case "Fill_N2_to_Ship":
                    process = "Fill_N2_to_Ship";
                    break;
                case "Packing":
                    process = "Packing";
                    break;
                case "Any_QC_Inspection":
                    process = "QC_Inspection";
                    break;
                case "MRB_Rework":
                    process = "MRB_Rework";
                    break;
            }
            return process;
        }
    }
}
