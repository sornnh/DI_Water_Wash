using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LeakSensorLog
{
    public DateTime Time { get; set; }
    public string SerialNumber { get; set; }
    public string TestResult { get; set; } // PASS / FAIL
    // Pressure in psi, depending on the system
    public double ResistanceUSL { get; set; } // Pressure in kPa or psi
    public double ResistanceLSL { get; set; } // Pressure in kPa or psi
    public double ResistanceValue { get; set; }
    public string ToCsvLine()
    {
        return $"{Time:yyyy-MM-dd HH:mm:ss},{SerialNumber},{TestResult},{ResistanceUSL},{ResistanceLSL},{ResistanceValue}";
    }
    public static string GetCsvHeader()
    {
        return "Time,SerialNumber,TestResult,ResistanceUSL,ResistanceLSL,ResistanceValue";
    }
}
