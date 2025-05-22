using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace DI_Water_Wash
{
    public static class ClsIO
    {
        public static void CreateNewIniFile(Dictionary<string, Dictionary<string, string>> iniData, string path)
        {
            iniData = new Dictionary<string, Dictionary<string, string>>()
                {
                    {
                        "General", new Dictionary<string, string>()
                        {
                            { "AppName", "MyApplication" },
                            { "Version", "1.0.0" }
                        }
                    },
                    {
                        "Settings", new Dictionary<string, string>()
                        {
                            { "MaxUsers", "100" },
                            { "Timeout", "30" }
                        }
                    }
                };
            string DirectoryPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }
            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (var section in iniData)
                {
                    writer.WriteLine($"[{section.Key}]");

                    foreach (var keyValue in section.Value)
                    {
                        writer.WriteLine($"{keyValue.Key} = {keyValue.Value}");
                    }
                    writer.WriteLine();  // Dòng trống giữa các section
                }
            }
        }
        // Đọc tất cả các section và key-value từ file .ini
        public static Dictionary<string, Dictionary<string, string>> ReadIniFile(string path)
        {
            Dictionary<string, Dictionary<string, string>> iniData = new Dictionary<string, Dictionary<string, string>>();
            try
            {
                string[] lines = File.ReadAllLines(path);
                string currentSection = null;
                Dictionary<string, string> currentSectionData = null;

                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();

                    // Bỏ qua các dòng trống và dòng comment
                    if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith(";"))
                    {
                        continue;
                    }

                    // Nếu là section, bắt đầu một section mới
                    if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                    {
                        currentSection = trimmedLine.Trim('[', ']');
                        currentSectionData = new Dictionary<string, string>();
                        iniData[currentSection] = currentSectionData;
                    }
                    else
                    {
                        // Nếu là key-value pair
                        if (currentSectionData != null)
                        {
                            string[] keyValue = trimmedLine.Split(new char[] { '=' }, 2);
                            if (keyValue.Length == 2)
                            {
                                string key = keyValue[0].Trim();
                                string value = keyValue[1].Trim();
                                currentSectionData[key] = value;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when read ini file: " + ex.Message);
            }
            

            return iniData;
        }

        // Đọc giá trị của một key trong một section cụ thể
        public static string ReadValue(string section, string key , string defaultValue, string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    CreateNewIniFile(new Dictionary<string, Dictionary<string, string>>(), path);
                }
                var iniData = ReadIniFile(path);
                if (iniData.ContainsKey(section) && iniData[section].ContainsKey(key))
                {
                    return iniData[section][key];
                }
                else
                {
                    return defaultValue; // Hoặc có thể trả về giá trị mặc định nếu không tìm thấy
                }
            }
            catch (Exception ex)
            {
                return defaultValue;
            }
        }

        // Ghi dữ liệu vào file .ini
        public static void WriteToIniFile(Dictionary<string, Dictionary<string, string>> iniData, string path ) 
        {
            if (!File.Exists(path))
            {
                CreateNewIniFile(iniData, path);
            }
            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (var section in iniData)
                {
                    writer.WriteLine($"[{section.Key}]");

                    foreach (var keyValue in section.Value)
                    {
                        writer.WriteLine($"{keyValue.Key} = {keyValue.Value}");
                    }

                    writer.WriteLine();  // Dòng trống giữa các section
                }
            }
        }

        // Ghi hoặc cập nhật một giá trị cho một key trong một section cụ thể
        public static void WriteValue(string section, string key, string value, string path)
        {
            var iniData = ReadIniFile(path);
            // Nếu chưa có section, tạo mới
            if (!iniData.ContainsKey(section))
            {
                iniData[section] = new Dictionary<string, string>();
            }
            // Cập nhật hoặc thêm key-value mới vào section
            iniData[section][key] = value;

            // Ghi lại toàn bộ dữ liệu vào file
            WriteToIniFile(iniData , path);
        }
    }
}
