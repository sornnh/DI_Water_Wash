using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FlushingLog
{
    public DateTime Time { get; set; }
    public string SerialNumber { get; set; }
    public string TestResult { get; set; } // PASS / FAIL
    // Pressure in psi, depending on the system
    public List<double> FlowRate { get; set; } 
    public List<double> WaterPressure { get; set; }
    public List<double> AirPressure { get; set; }
    public string ToCsvLine()
    {
        return $"{Time:yyyy-MM-dd HH:mm:ss},{SerialNumber},{TestResult},{string.Join(";", FlowRate.Select(v => v.ToString("F3")))},{string.Join(";", WaterPressure.Select(v => v.ToString("F3")))},{string.Join(";", AirPressure.Select(v => v.ToString("F3")))}";
    }
    public static string GetCsvHeader()
    {
        return "Time,SerialNumber,TestResult,FlowRate,WaterPressure,AirPressure";
    }
}
