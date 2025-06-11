using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DI_Water_Wash.Sequence
{
    public class Cls_SequencyCommon
    {
        public int iIndex { get; private set; }
        public int iRelayRed { get; private set; }
        public int iRelayGreen { get; private set; }
        public int iRelayBuzzer { get; private set; }
        public int iRelayYellow { get; private set; }
        private StateCommon.ProcessState _process = StateCommon.ProcessState.Idle;
        Cls_ASPcontrol Cls_ASPcontrol ;
        private bool _AutoMode = false;
        public delegate void DelShow_UpdateStageStatus_Request(Cls_SequencyCommon sender);
        public event DelShow_UpdateStageStatus_Request OnRequestUpdateStage;
        private bool _isTestResultShown = false;
        private static readonly ILog log = LogManager.GetLogger(typeof(Cls_SequencyCommon));
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
        public Cls_SequencyCommon(int Index, Cls_ASPcontrol cls_ASPcontrol)
        {
            iIndex = Index;
            Cls_ASPcontrol = cls_ASPcontrol;
            switch (Index)
            {
                case 0:
                    iRelayRed = 8;
                    iRelayYellow = 9;
                    iRelayGreen = 10;
                    iRelayBuzzer = 11;
                    break;
                case 1:
                    iRelayRed = 12;
                    iRelayYellow = 13;
                    iRelayGreen = 14;
                    iRelayBuzzer = 15;
                    break;
                case 2:
                    iRelayRed = 16;
                    iRelayYellow = 17;
                    iRelayGreen = 18;
                    iRelayBuzzer = 19;
                    break;
            }
        }
        public void LoopTowerLamp()
        {
            if(!_AutoMode) return;
            UpdateLedTowerLamp();
        }
        public void UpdateStageStatus()
        {
            OnRequestUpdateStage?.Invoke(this);
        }
        private async void UpdateLedTowerLamp()
        {
            UpdateStageStatus();
            switch (_process)
            {
                case StateCommon.ProcessState.Idle:
                    if (Cls_ASPcontrol.relayStates[iRelayRed])
                    {
                        await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayRed + 1, 0);
                    }
                    if (!Cls_ASPcontrol.relayStates[iRelayGreen])
                    {
                        await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayGreen + 1, 1);
                    }
                    if (!Cls_ASPcontrol.relayStates[iRelayYellow])
                    {
                        await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayYellow + 1, 1);
                    }
                    if (Cls_ASPcontrol.relayStates[iRelayBuzzer])
                    {
                        await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayBuzzer + 1, 0);
                    }
                    break;
                case StateCommon.ProcessState.Running:
                    if (Cls_ASPcontrol.relayStates[iRelayRed])
                    {
                        await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayRed + 1, 0);
                    }
                    if (!Cls_ASPcontrol.relayStates[iRelayGreen])
                    {
                        await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayGreen + 1, 1);
                    }
                    if (Cls_ASPcontrol.relayStates[iRelayYellow])
                    {
                        await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayYellow + 1, 0);
                    }
                    if (Cls_ASPcontrol.relayStates[iRelayBuzzer])
                    {
                        await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayBuzzer + 1, 0);
                    }
                    break;
                case StateCommon.ProcessState.CompletedPass:
                    if(!_isTestResultShown)
                    {
                        _isTestResultShown = true;  // Đánh dấu đang xử lý
                        log.Debug("Start set show light pass");
                        await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayRed, 0);
                        await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayYellow, 0);
                        await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayBuzzer, 0);
                        for (int i = 0; i < 10; i++)
                        {
                            await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayGreen, i % 2);  // bật/tắt xen kẽ
                            Thread.Sleep(1000);  // Giữ trong 500ms
                        }
                        log.Debug("End set show light pass");
                        _isTestResultShown = false;  // Đánh dấu đã xử lý xong
                        _process = StateCommon.ProcessState.Idle;  // Đặt lại trạng thái về Idle
                    }    
                    break;
                case StateCommon.ProcessState.CompletedFail:
                    if(!_isTestResultShown)
                    {
                        _isTestResultShown = true;  // Đánh dấu đang xử lý
                        log.Debug("Start set show light fail");
                        await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayGreen, 0);
                        await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayYellow, 0);
                        await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayBuzzer, 0);
                        for (int i = 0; i < 10; i++)
                        {
                            await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayRed, i % 2);  // bật/tắt xen kẽ
                            Thread.Sleep(1000);  // Giữ trong 500ms
                        }
                        log.Debug("End set show light fail");
                        _isTestResultShown = false;  // Đánh dấu đã xử lý xong
                        _process = StateCommon.ProcessState.Idle;  // Đặt lại trạng thái về Idle
                    }
                    break;
            }
        }
    }
}
