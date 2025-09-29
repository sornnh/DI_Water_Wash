using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        public Cls_SequencyCommon(Cls_ASPcontrol cls_ASPcontrol)
        {
            Cls_ASPcontrol = cls_ASPcontrol;
            iRelayRed = 9;
            iRelayYellow = 10;
            iRelayGreen = 11;
            iRelayBuzzer = 12;
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
                    if (Cls_ASPcontrol.relayStates[iRelayRed-1])
                    {
                        await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayRed, 0);
                    }
                    if (!Cls_ASPcontrol.relayStates[iRelayGreen-1])
                    {
                        await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayGreen, 1);
                    }
                    if (!Cls_ASPcontrol.relayStates[iRelayYellow - 1])
                    {
                        await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayYellow, 1);
                    }
                    if (Cls_ASPcontrol.relayStates[iRelayBuzzer - 1])
                    {
                        await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayBuzzer, 0);
                    }
                    break;
                case StateCommon.ProcessState.Running:
                    if (Cls_ASPcontrol.relayStates[iRelayRed - 1])
                    {
                        await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayRed, 0);
                    }
                    if (!Cls_ASPcontrol.relayStates[iRelayGreen - 1])
                    {
                        await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayGreen, 1);
                    }
                    if (Cls_ASPcontrol.relayStates[iRelayYellow - 1])
                    {
                        await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayYellow, 0);
                    }
                    if (Cls_ASPcontrol.relayStates[iRelayBuzzer - 1])
                    {
                        await Cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayBuzzer, 0);
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
