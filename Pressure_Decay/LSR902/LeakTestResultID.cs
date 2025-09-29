using System;

public class LeakTestResultID : LeakTestResultBase
{
    public string TesterId { get; set; }
    public string Mode { get; set; }

    public double Leak { get; set; }
    public double LeakUL { get; set; }
    public double LeakLL { get; set; }
    public double Compensation { get; set; }

    public double Pressure { get; set; }
    public double PressureUL { get; set; }
    public double PressureLL { get; set; }

    public static LeakTestResultID Parse(string raw)
    {
        var cleaned = raw.Split(':')[0].Trim();
        if (!cleaned.StartsWith("#"))
            throw new FormatException("Invalid ID format");

        var p = cleaned.Substring(1).Split('_');

        if (p.Length < 11)
            throw new FormatException("ID format incomplete");

        string resultCode = p[2];
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

        return new LeakTestResultID
        {
            TesterId = p[0],
            Mode = p[1],
            Result = resultText,
            Leak = double.Parse(p[3]),
            LeakUL = double.Parse(p[4]),
            LeakLL = double.Parse(p[5]),
            Compensation = double.Parse(p[6]),
            Pressure = double.Parse(p[7]),
            PressureUL = double.Parse(p[8]),
            PressureLL = double.Parse(p[9]),
            Channel = int.Parse(p[10]),
            Timestamp = DateTime.Now // ID Format không có timestamp, gán thời điểm nhận
        };
    }
}

