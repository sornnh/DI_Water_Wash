using System;

public class LeakTestResultK : LeakTestResultBase
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

    public double KAuto { get; set; }
    public double KValue { get; set; }
    public int KCheckPercent { get; set; }

    public string ErrorCode { get; set; }

    public static LeakTestResultK Parse(string raw)
    {
        var cleaned = raw.Split(':')[0].Trim();
        if (!cleaned.StartsWith("#"))
            throw new FormatException("Invalid K format");

        var p = cleaned.Substring(1).Split(',');

        if (p.Length < 17)
            throw new FormatException("K format incomplete");

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

        return new LeakTestResultK
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
            KAuto = double.Parse(p[11]),
            KValue = double.Parse(p[12]),
            KCheckPercent = int.Parse(p[13]),
            ErrorCode = p[14],
            Timestamp = DateTime.Parse($"{p[15]} {p[16]}")
        };
    }
}

