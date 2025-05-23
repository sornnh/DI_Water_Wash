using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;

namespace DI_Water_Wash
{
    public class Cls_SequencyTest
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Cls_ASPcontrol));
        public int iRelay3Way_Air_Water { get; private set; }
        public int iRelayPump { get; private set; }
        public int iRelay3Way_Reverse { get; private set; }
        public int iIndex { get; private set; }
        private StateCommon.ProcessState _process = StateCommon.ProcessState.Idle;
        Cls_ASPcontrol Cls_ASPcontrol;
        private bool _AutoMode = false;
        public bool AutoMode
        {
            get { return _AutoMode; }
            set { _AutoMode = value; }
        }
        public StateCommon.ProcessState process
        {
            get { return _process; }
            set { _process = value; }
        }
        public Cls_SequencyTest(int Index, Cls_ASPcontrol cls_ASPcontrol)
        {
            iIndex = Index;
            Cls_ASPcontrol = cls_ASPcontrol;
            switch (Index)
            {
                case 0:
                    iRelay3Way_Air_Water = 1;
                    iRelayPump = 8;
                    iRelay3Way_Reverse = 22;
                    break;
                case 1:
                    iRelay3Way_Air_Water = 2;
                    iRelayPump = 7;
                    iRelay3Way_Reverse = 23;
                    break;
                case 2:
                    iRelay3Way_Air_Water = 3;
                    iRelayPump = 6;
                    iRelay3Way_Reverse = 24;
                    break;
                case 3:
                    iRelay3Way_Air_Water = 4;
                    iRelayPump = 5;
                    iRelay3Way_Reverse = 25;
                    break;
            }
        }
        public async void SwitchRelay3Way_Air_Water(bool bOnOff)
        {
            if (bOnOff)
            {
                bool bSuccess = await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelay3Way_Air_Water, 1);
                if(!bSuccess)
                {
                    log.Error($"Relay 3Way {iRelay3Way_Air_Water} failed to turn on.");
                    MessageBox.Show($"Relay 3Way {iRelay3Way_Air_Water} failed to turn on.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                bool bSuccess = await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelay3Way_Air_Water,0 );
                if (!bSuccess)
                {
                    log.Error($"Relay 3Way {iRelay3Way_Air_Water} failed to turn off.");
                    MessageBox.Show($"Relay 3Way {iRelay3Way_Air_Water} failed to turn off.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public async void SwitchRelayPump(bool bOnOff)
        {
            if (bOnOff)
            {
                bool bSuccess = await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayPump, 1);
                if (!bSuccess)
                {
                    log.Error($"Relay Pump {iRelayPump} failed to turn on.");
                    MessageBox.Show($"Relay Pump {iRelayPump} failed to turn on.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                bool bSuccess = await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayPump, 0);
                if (!bSuccess)
                {
                    log.Error($"Relay Pump {iRelayPump} failed to turn off.");
                    MessageBox.Show($"Relay Pump {iRelayPump} failed to turn off.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        public async void SwitchRelay3Way_Reverse(bool bOnOff)
        {
            if (bOnOff)
            {
                bool bSuccess = await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelay3Way_Reverse, 1);
                if (!bSuccess)
                {
                    log.Error($"Relay 3Way Reverse {iRelay3Way_Reverse} failed to turn on.");
                    MessageBox.Show($"Relay 3Way Reverse {iRelay3Way_Reverse} failed to turn on.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                bool bSuccess = await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelay3Way_Reverse, 0);
                if (!bSuccess)
                {
                    log.Error($"Relay 3Way Reverse {iRelay3Way_Reverse} failed to turn off.");
                    MessageBox.Show($"Relay 3Way Reverse {iRelay3Way_Reverse} failed to turn off.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        public void LoopTest()
        {
            if (!_AutoMode) return;
        }
    }
}
