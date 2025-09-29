using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PressureDecayLog
{
    public DateTime Time { get; set; }
    public string SerialNumber { get; set; }
    public string TestResult { get; set; } // PASS / FAIL
    // Pressure in psi, depending on the system
    public double PressureUSL { get; set; } // Pressure in kPa or psi
    public double PressureLSL { get; set; } // Pressure in kPa or psi
    public double PressureValue { get; set; }
    public string PressureType { get; set; } // e.g., "kPa", "psi"

    // Leakage in kPa or psi, depending on the system
    public double LeakageUSL { get; set; } // Upper Specification Limit for Leakage
    public double LeakageLSL { get; set; } // Lower Specification Limit for Leakage
    public double Leakagevalue { get; set; }
    public string LeakageType { get; set; } // e.g., "kPa", "psi"

    //Time to test in seconds
    public double PressureTime { get; set; } // Time taken for the test in seconds
    public double Balance1Time { get; set; } // Balance time for the first measurement
    public double Balance2Time { get; set; } // Balance time for the second measurement
    public double DetectTime { get; set; } // Detection time for the test
    public double KVe { get; set; } // K value for the test, if applicable
    public string ToCsvLine()
    {
        return $"{Time:yyyy-MM-dd HH:mm:ss},{SerialNumber},{TestResult},{PressureUSL},{PressureLSL},{PressureValue},{PressureType},{LeakageUSL},{LeakageLSL},{Leakagevalue},{LeakageType},{PressureTime},{Balance1Time},{Balance2Time},{DetectTime},{KVe}";
    }
    public static string GetCsvHeader()
    {
        return "Time,SerialNumber,TestResult,PressureUSL,PressureLSL,PressureValue,PressureType," +
               "LeakageUSL,LeakageLSL,Leakagevalue,LeakageType," +
               "PressureTime,Balance1Time,Balance2Time,DetectTime,KVe";
    }
}
