using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;
using System.Reflection;
using System.IO;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]

namespace DI_Water_Wash
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();
            if(!Directory.Exists(@"C:\Aavid_Test\Logs"))
                Directory.CreateDirectory(@"C:\Aavid_Test\Logs"); // Tạo thư mục nếu chưa có
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var itemDB = new Cls_DBMsSQL();
            itemDB.Initialize("10.102.4.20", "ApprovalDB", "sa", "nuventixleo");
            string password = itemDB.GetPasswordByUser("ChangeMode");
            if (password == null)
            {
                MessageBox.Show("Can't connect to DataBase. Please contact PIE Technical or IT!!!",
                                "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Application.Run(new Form1(password));
        }
    }
}
