using System;
using System.Globalization;
using System.Text.RegularExpressions;

public class LeakTestResultM : LeakTestResultBase
{
    public string TesterId { get; set; }
    public string Channel { get; set; }
    public string ErrorCode { get; set; }
    public string ResultCode { get; set; }
    public string ResultText { get; set; }
    public string LeakTestType { get; set; }
    public string PresureTestType { get; set; }
    public double Leak { get; set; }
    public double TestPressure { get; set; }
    public double DeltaP { get; set; }
    public double KValue { get; set; }
    public double ChargeDelay_Time { get; set; } //DL1
    public double Pressure_Time { get; set; } //CHG

    public double Balance1_Time { get; set; } //BAL1
    public double Balance2_Delay { get; set; } //BAL2 Delay
    public double Balance2_Time { get; set; } //BAL2 Time
    public double Detection_Time { get; set; } // DET
    public double Blow_Time { get; set; } //BLW
    public double Exhaust_Time { get; set; } // EXH
    public double End_Time { get; set; } // END


    public double[] StageTimes { get; set; } = new double[11]; // DL1, CHG, BAL1, BAL2, DET, EXH, ...
    public int LeakUnit { get; set; }
    public int PressureUnit { get; set; }
    public int KUnit { get; set; }

    public double BAL_UL { get; set; }
    public double BAL_LL { get; set; }
    public double DET_UL { get; set; }
    public double DET_UL2 { get; set; }
    public double DET_LL { get; set; }
    public double DET_LL2 { get; set; }
    public double TestPressureUL { get; set; }
    public double TestPressureLL { get; set; }

    public int[] Flags { get; set; } = new int[4]; // mode, press type, comp type, etc.

    public double[] DPThresholds { get; set; } = new double[6];
    public int NR { get; set; }
    public int SampleCount { get; set; }

    public double EPPressure { get; set; }
    public double EPPrecharge { get; set; }

    public DateTime Timestamp { get; set; }
    public string Checksum { get; set; }

    public static LeakTestResultM Parse(string raw)
    {
        var cleaned = raw.Split(':')[0].Trim();
        var parts = raw.Trim().Split(':');
        if (!cleaned.StartsWith("#")) throw new FormatException("Invalid M format");
        string checksum = parts.Length > 1 ? parts[1] : null;
        string content = parts[0].TrimStart('#').Trim();
        var p = cleaned.Substring(1).Split(' ');

        if (p.Length < 48)
            throw new FormatException("M format incomplete");
        var culture = CultureInfo.InvariantCulture;
        var style = NumberStyles.Float;
        string resultCode = p[3];
        string resultText;

        switch (resultCode)
        {
            case "1": resultText = "FAIL (LL)"; break;
            case "2": resultText = "PASS"; break;
            case "4": resultText = "FAIL (UL)"; break;
            case "9": resultText = "FAIL (LL2)"; break;
            case "C": resultText = "FAIL (UL2)"; break;
            case "D": resultText = "ERROR"; break;
            default: resultText = "Unknown (" + resultCode + ")"; break;
        }

        return new LeakTestResultM
        {
            TesterId = p[0],
            Channel = p[1],
            ErrorCode = GetErrorCode(p[2]),
            ResultCode = p[3],
            ResultText = resultText,
            Leak = double.Parse(p[4], style, culture),
            TestPressure = double.Parse(p[5], style, culture),
            DeltaP = double.Parse(p[6], style, culture),
            KValue = double.Parse(p[7], style, culture),
            ChargeDelay_Time = double.Parse(p[8], style, culture),
            Pressure_Time = double.Parse(p[9], style, culture),
            Balance1_Time = double.Parse(p[10], style, culture),
            Balance2_Time = double.Parse(p[11], style, culture),
            Detection_Time = double.Parse(p[12], style, culture),
            Blow_Time = double.Parse(p[13], style, culture),
            End_Time = double.Parse(p[14], style, culture),
            Exhaust_Time = double.Parse(p[15], style, culture),
            StageTimes = new double[]
            {
                double.Parse(p[8], style, culture),
                double.Parse(p[9], style, culture),
                double.Parse(p[10], style, culture),
                double.Parse(p[11], style, culture),
                double.Parse(p[12], style, culture),
                double.Parse(p[13], style, culture),
                double.Parse(p[14], style, culture),
                double.Parse(p[15], style, culture),
                double.Parse(p[16], style, culture),
                double.Parse(p[17], style, culture),
                double.Parse(p[18], style, culture),
                double.Parse(p[19], style, culture),
                double.Parse(p[20], style, culture),
            },

            LeakUnit = int.Parse(p[21]),
            LeakTestType = GetLeakUnitType(int.Parse(p[21])),
            PressureUnit = int.Parse(p[22]),
            PresureTestType = GetPressureUnitType(int.Parse(p[22])),
            KUnit = int.Parse(p[23]),

            BAL_UL = double.Parse(p[24]),
            BAL_LL = double.Parse(p[25]),
            DET_UL = double.Parse(p[26]),
            DET_UL2 = double.Parse(p[27]),
            DET_LL = double.Parse(p[28]),
            DET_LL2 = double.Parse(p[29]),
            TestPressureUL = double.Parse(p[30], style, culture),
            TestPressureLL = double.Parse(p[31], style, culture),

            Flags = new int[]
            {
                int.Parse(p[32]),
                int.Parse(p[33]),
                int.Parse(p[34]),
                int.Parse(p[35])
            },

            DPThresholds = new double[]
            {
                double.Parse(p[36], style, culture),
                double.Parse(p[37], style, culture),
                double.Parse(p[38], style, culture),
                double.Parse(p[39], style, culture),
                double.Parse(p[40], style, culture),
                double.Parse(p[41], style, culture),
            },

            NR = int.Parse(p[42]),
            SampleCount = int.Parse(p[43]),

            EPPressure = double.Parse(p[44], style, culture),
            EPPrecharge = double.Parse(p[45], style, culture),

            Timestamp = DateTime.ParseExact(p[46] + p[47], "ddMMyyHHmmss", culture),
            Checksum = checksum
        };

    }
    public static string GetErrorCode(string  iCode)
    {
        switch(iCode)
        {
            case "00": return "Not Error (Pass/Fail)";
            case "01": return "ERROR 23: Mastering Error";
            case "02": return "ERROR 52: AD Communication Error";
            case "04": return "ERROR 22: Stop Valves Closed";
            case "06": return "ERROR 1: PS Offset Error";
            case "08": return "ERROR 3 Improper Test Pressure";
            case "10": return "ERROR 2: PS Output Out of Range";
            case "15": return "ERROR 11 to 16 Air Operated Valve Error";
            default: return "Unknown error code: " + iCode;
        }
    }
    //00: kPa, 01: MPa, 02: psi, 03: kg/cm2, 04: bar, 05: mbar, 06: mmHg,
    //07: cmHg, 08: inHg
    public static string GetPressureUnitType(int itype)
    {
        switch (itype)
        {
            case 0: return "kPa";
            case 1: return "MPa";
            case 2: return "psi";
            case 3: return "kg/cm²";
            case 4: return "bar";
            case 5: return "mbar";
            case 6: return "mmHg";
            case 7: return "cmHg";
            case 8: return "inHg";
            default: return $"Unknown ({itype})";
        }
    }
    public static string GetLeakUnitType(int itype)
    {
        switch (itype)
        {
            case 0: return "Pa";
            case 1: return "kPa";
            case 2: return "mmH₂O";
            case 3: return "inH₂O";
            case 4: return "mmHg";
            case 5: return "mL/s";
            case 6: return "mL/min";
            case 7: return "in³/min";
            case 8: return "in³/d";
            case 9: return "L/min";
            case 10: return "ft³/h";
            case 11: return "Pa·m³/s";
            case 12: return "E-3 Pa·m³/s";
            case 13: return "Pa/s";
            case 14: return "Pa/min";
            case 15: return "*Pa/s";
            case 16: return "*Pa/min";
            default: return $"Unknown ({itype})";
        }
    }

}
