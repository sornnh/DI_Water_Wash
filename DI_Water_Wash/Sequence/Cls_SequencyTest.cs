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
        public int iADVFlowRate { get; private set; }
        public byte slaveID { get; private set; } 
        private StateCommon.ProcessState _process = StateCommon.ProcessState.Idle;
        Cls_ASPcontrol Cls_ASPcontrol;
        ClsInverterModbus Cls_InverterModbus;
        private bool _AutoMode = false;
        private StateCommon.InverterType _InverterType;
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
        public Cls_SequencyTest(int Index, Cls_ASPcontrol cls_ASPcontrol,ClsInverterModbus clsInverterModbus, StateCommon.InverterType inverterType)
        {
            iIndex = Index;
            Cls_ASPcontrol = cls_ASPcontrol;
            Cls_InverterModbus = clsInverterModbus;
            _InverterType = inverterType;
            switch (Index)
            {
                case 0:
                    iRelay3Way_Air_Water = 1;
                    iRelayPump = 8;
                    iRelay3Way_Reverse = 22;
                    iADVFlowRate = 1;
                    slaveID = 1;
                    break;
                case 1:
                    iRelay3Way_Air_Water = 2;
                    iRelayPump = 7;
                    iRelay3Way_Reverse = 23;
                    iADVFlowRate = 2;
                    slaveID = 2;
                    break;
                case 2:
                    iRelay3Way_Air_Water = 3;
                    iRelayPump = 6;
                    iRelay3Way_Reverse = 24;
                    iADVFlowRate = 3;
                    slaveID = 3;
                    break;
                case 3:
                    iRelay3Way_Air_Water = 4;
                    iRelayPump = 5;
                    iRelay3Way_Reverse = 25;
                    iADVFlowRate = 4;
                    slaveID = 4;
                    break;
            }

            _InverterType = inverterType;
        }
        public async void ReadADV()
        {
            try
            {
                double flowRate = await Cls_ASPcontrol.GetADVAsync(iADVFlowRate);
                if (flowRate < 0)
                {
                    log.Error($"Failed to read flow rate for ADV {iADVFlowRate}.");
                    MessageBox.Show($"Failed to read flow rate for ADV {iADVFlowRate}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    log.Info($"Flow rate for ADV {iADVFlowRate}: {flowRate} L/min");
                }
            }
            catch (Exception ex)
            {
                log.Error($"Exception while reading flow rate for ADV {iADVFlowRate}: {ex.Message}");
                MessageBox.Show($"Exception while reading flow rate for ADV {iADVFlowRate}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            bool bSuccess = false;
            if (bOnOff)
            {
                switch (_InverterType)
                {
                    case StateCommon.InverterType.ASP:
                        bSuccess = await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayPump, 1);
                        if (!bSuccess)
                        {
                            log.Error($"Relay Pump {iRelayPump} failed to turn on.");
                            MessageBox.Show($"Relay Pump {iRelayPump} failed to turn on.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        break;
                    case StateCommon.InverterType.RS485:
                        bSuccess = Cls_InverterModbus.SetInverterOnOff(slaveID, true);
                        if (!bSuccess)
                        {
                            log.Error($"Turn On Pump {slaveID} failed.");
                            MessageBox.Show($"Turn On Pump {slaveID} failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        break;
                }
            }
            else
            {
                switch (_InverterType)
                {
                    case StateCommon.InverterType.ASP:
                        bSuccess = await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayPump, 0);
                        if (!bSuccess)
                        {
                            log.Error($"Relay Pump {iRelayPump} failed to turn off.");
                            MessageBox.Show($"Relay Pump {iRelayPump} failed to turn off.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        break;
                    case StateCommon.InverterType.RS485:
                        bSuccess = Cls_InverterModbus.SetInverterOnOff(slaveID, false);
                        if (!bSuccess)
                        {
                            log.Error($"Turn Off Pump {slaveID} failed.");
                            MessageBox.Show($"Turn Off Pump {slaveID} failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        break;
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
