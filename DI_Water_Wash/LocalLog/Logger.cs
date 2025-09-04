using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

public class Logger
{
    private readonly string filePath;

    public Logger(string filePath)
    {
        this.filePath = filePath;

        // Tạo header nếu chưa có file
        if (!File.Exists(filePath))
        {
            // Tạo thư mục nếu chưa tồn tại
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            // Tạo file và ghi header
            File.AppendAllText(filePath, FlushingLog.GetCsvHeader() + "\n");
        }
    }
    public void FlushingTest(FlushingLog log)
    {
        File.AppendAllText(filePath, log.ToCsvLine() + "\n");
    }
    public void PressureDecay(PressureDecayLog log)
    {
        File.AppendAllText(filePath, log.ToCsvLine() + "\n");
    }
    public void LeakSensorTest(LeakSensorLog log)
    {
        File.AppendAllText(filePath, log.ToCsvLine() + "\n");
    }
}

