using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace DI_Water_Wash
{
    public partial class FrmLoading : Form
    {
        public FrmLoading()
        {
            InitializeComponent();
            Timer tmrStart = new Timer();
            tmrStart.Tick += tmrStart_Tick;
            tmrStart.Interval = 100;
            tmrStart.Start();
        }

        void tmrStart_Tick(object sender, EventArgs e)
        {
            if (StateCommon.bLoading == false)
            {
                this.Close();
            }
            else
            {
                lblInfo.Text = StateCommon.LoadingText;
                progressBar1.Value = StateCommon.LoadingValue;
            }

        }
    }
}
