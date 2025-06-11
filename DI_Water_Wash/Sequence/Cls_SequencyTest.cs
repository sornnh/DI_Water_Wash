using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Windows.Forms;
using static DI_Water_Wash.Cls_ASPcontrol;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace DI_Water_Wash
{
    public class Cls_SequencyTest
    {
        public delegate void DelShow_UpdateFlowRate_Request(Cls_SequencyTest sender);

        public enum TestSeq
        {
            NONE = -2, // Trạng thái không có quá trình nào đang chạy
            ERROR = -1,
            WAIT = 9999,
            SN_INSERT = 0,
            SN_VERIFY = 1,
            SN_CHECK_ROUTER = 2,
            SN_CHECK_ROUTER_OK = 3,
            SETTING_PUMP = 4,
            PRE_WASHING =5,
            FLUSHING_WASHING = 6,
            TURN_REVERSE_WASHING = 7,
            REVERSE_WASHING = 8,
            TURN_FLUSHING_WASHING = 9,
            TURN_DRYING = 11,
            DRYING = 12,
            TURN_REVERSE_DRYING = 13,
            REVERSE_DRYING = 14,
            CHECKING_RESULT = 15,
            TEST_PASS = 16,
            SET_LIGHT_TOWER_PASS = 17,
            TEST_FAIL = 18,
            SET_LIGHT_TOWER_FAIL = 19,
            CREATE_LOCAL_LOG = 20,
            SAVE_DB = 21,
        }

        //public delegate void DelShow_AddProcessLogTest_Request(DataTable sender);
        public event EventHandler<ProcessLogEventArgs> OnRequestAddProcessLogTest;
        public class ProcessLogEventArgs : EventArgs
        {
            public int UnitIndex { get; set; }
            public System.Data.DataTable LogData { get; set; }
        }
        public Stopwatch sw_prewash { get; private set; }
        public Stopwatch sw_flush { get; private set; }
        public Stopwatch sw_reverse { get; private set; }
        public Stopwatch sw_drying { get; private set; }
        public Stopwatch sw_reverse_drying { get; private set; }

        Stopwatch sw_Showpass = new Stopwatch();
        Stopwatch sw_Showfail = new Stopwatch();
        private bool _isTestResultShown = false;
        private static readonly ILog log = LogManager.GetLogger(typeof(Cls_ASPcontrol));
        private string TestResult;

        private Cls_ASPcontrol cls_ASPcontrol;

        private ClsInverterModbus Cls_InverterModbus;
        Cls_DBMsSQL ProductionDB = new Cls_DBMsSQL();

        private bool _AutoMode = true;

        private StateCommon.InverterType _InverterType;

        private bool _bGetFlowRate = false;

        private string _SN = "";

        private bool AccessInsertSN = false;
        private TestSeq _testSeq = TestSeq.NONE;
        public TestSeq testSeq
        {
            get
            {
                return _testSeq;
            }
            set
            {
                if (_testSeq == TestSeq.WAIT)
                {
                    _testSeq = value;
                }
            }
        }
        public int iRelay3Way_Air_Water { get; private set; }

        public int iRelayPump { get; private set; }

        public int iRelay3Way_Reverse { get; private set; }

        public int iIndex { get; private set; }

        public int iADAFlowRate { get; private set; }
        public int iRelayRed { get; private set; }
        public int iRelayGreen { get; private set; }
        public int iRelayBuzzer { get; private set; }
        public int iRelayYellow { get; private set; }
        public byte slaveID { get; private set; }

        public double FlowRate { get; private set; }
        private int _StepTesting = 0;
        private string _TestType = "Production Test";
        public string TestType 
        {
            get { return _TestType; }
            set { _TestType = value; }
        }
        private string _WorkOder = "";
        public string WorkOder
        {
            get { return _WorkOder; }
            set { _WorkOder = value; }
        }
        public int StepTesting
        {
            get { return _StepTesting; }
        }
        public string SN
        {
            get
            {
                return _SN;
            }
            set
            {
                if (AccessInsertSN)
                {
                    _SN = value;
                }
                else
                {
                    log.Warn((object)"Attempt to set SN when access is not allowed.");
                }
            }
        }

        public bool bGetFlowRate
        {
            get
            {
                return _bGetFlowRate;
            }
            set
            {
                _bGetFlowRate = value;
            }
        }

        public bool AutoMode
        {
            get
            {
                return _AutoMode;
            }
            set
            {
                _AutoMode = value;
            }
        }

        public event DelShow_UpdateFlowRate_Request OnRequestUpdateFlowRate;

        public Cls_SequencyTest(int Index, Cls_ASPcontrol cls_ASP, ClsInverterModbus clsInverterModbus, StateCommon.InverterType inverterType)
        {
            iIndex = Index;
            cls_ASPcontrol = cls_ASP;
            Cls_InverterModbus = clsInverterModbus;
            _InverterType = inverterType;
            switch (Index)
            {
                case 0:
                    iRelay3Way_Air_Water = 1;
                    iRelayPump = 8;
                    iRelay3Way_Reverse = 22;
                    iADAFlowRate = 1;
                    slaveID = 1;
                    break;
                case 1:
                    iRelay3Way_Air_Water = 2;
                    iRelayPump = 7;
                    iRelay3Way_Reverse = 23;
                    iADAFlowRate = 2;
                    slaveID = 2;
                    break;
                case 2:
                    iRelay3Way_Air_Water = 3;
                    iRelayPump = 6;
                    iRelay3Way_Reverse = 24;
                    iADAFlowRate = 3;
                    slaveID = 3;
                    break;
                case 3:
                    iRelay3Way_Air_Water = 4;
                    iRelayPump = 5;
                    iRelay3Way_Reverse = 25;
                    iADAFlowRate = 4;
                    slaveID = 4;
                    break;
            }
            switch (Index)
            {
                case 0:
                    iRelayRed = 9;
                    iRelayYellow = 10;
                    iRelayGreen = 11;
                    iRelayBuzzer = 12;
                    break;
                case 1:
                    iRelayRed = 13;
                    iRelayYellow = 14;
                    iRelayGreen = 15;
                    iRelayBuzzer = 16;
                    break;
                case 2:
                    iRelayRed = 17;
                    iRelayYellow = 18;
                    iRelayGreen = 19;
                    iRelayBuzzer = 20;
                    break;
            }
            sw_prewash = new Stopwatch();
            sw_flush = new Stopwatch();
            sw_reverse = new Stopwatch();
            sw_drying = new Stopwatch();
            sw_reverse_drying = new Stopwatch();
            _testSeq = TestSeq.WAIT;
            _InverterType = inverterType;
            ProductionDB.Initialize("10.102.4.20", "Production_SZ", "sa", "nuventixleo");
            ProductionDB.Open();
            Thread thread = new Thread(LoopGetFlowRate);
            thread.IsBackground = true;
            thread.Name = "ThrGetFlowRate" + Index;
            thread.Start();
        }
        public void LoopGetFlowRate()
        {
            while (true)
            {
                if (_bGetFlowRate)
                {
                    ReadADA();
                    UpdateFlowRate();
                }
                Thread.Sleep(500);
            }
        }
        public void UpdateFlowRate()
        {
            this.OnRequestUpdateFlowRate?.Invoke(this);
        }

        public async void SetFlowRate(double flowRate)
        {
            try
            {
                int flowRateInt = (int)(flowRate * 100.0);
                if (!(await cls_ASPcontrol.SetFlowRateCheckResult(iADAFlowRate, flowRateInt)))
                {
                    log.Error((object)$"Failed to set flow rate for ADA {iADAFlowRate} to {flowRate} L/min.");
                    MessageBox.Show($"Failed to set flow rate for ADA {iADAFlowRate} to {flowRate} L/min.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    log.Info((object)$"Flow rate for ADA {iADAFlowRate} set to {flowRate} L/min");
                    FlowRate = flowRate;
                }
            }
            catch (Exception ex)
            {
                Exception ex2 = ex;
                log.Error((object)$"Exception while setting flow rate for ADA {iADAFlowRate}: {ex2.Message}");
                MessageBox.Show($"Exception while setting flow rate for ADA {iADAFlowRate}: {ex2.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        public async void ReadADA()
        {
            try
            {
                double flowRate = await cls_ASPcontrol.GetADIAsync(iADAFlowRate);
                if (flowRate < 0.0)
                {
                    log.Error((object)$"Failed to read flow rate for ADA {iADAFlowRate}.");
                    MessageBox.Show($"Failed to read flow rate for ADA {iADAFlowRate}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    log.Info((object)$"Flow rate for ADA {iADAFlowRate}: {flowRate} L/min");
                }
                FlowRate = flowRate;
            }
            catch (Exception ex)
            {
                Exception ex2 = ex;
                log.Error((object)$"Exception while reading flow rate for ADA {iADAFlowRate}: {ex2.Message}");
                MessageBox.Show($"Exception while reading flow rate for ADA {iADAFlowRate}: {ex2.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        public async void SwitchRelay3Way_Air_Water(bool bOnOff)
        {
            if (bOnOff)
            {
                if (!(await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelay3Way_Air_Water, 1)))
                {
                    log.Error((object)$"Relay 3Way {iRelay3Way_Air_Water} failed to turn on.");
                    MessageBox.Show($"Relay 3Way {iRelay3Way_Air_Water} failed to turn on.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
            else if (!(await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelay3Way_Air_Water, 0)))
            {
                log.Error((object)$"Relay 3Way {iRelay3Way_Air_Water} failed to turn off.");
                MessageBox.Show($"Relay 3Way {iRelay3Way_Air_Water} failed to turn off.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        public async void SwitchRelayPump(bool bOnOff)
        {
            if (bOnOff)
            {
                switch (_InverterType)
                {
                    case StateCommon.InverterType.ASP:
                        if (!(await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayPump, 1)))
                        {
                            log.Error((object)$"Relay Pump {iRelayPump} failed to turn on.");
                            MessageBox.Show($"Relay Pump {iRelayPump} failed to turn on.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        }
                        break;
                    case StateCommon.InverterType.RS485:
                        if (!Cls_InverterModbus.SetInverterOnOff(slaveID, onoff: true))
                        {
                            log.Error((object)$"Turn On Pump {slaveID} failed.");
                            MessageBox.Show($"Turn On Pump {slaveID} failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        }
                        break;
                }
                return;
            }
            switch (_InverterType)
            {
                case StateCommon.InverterType.ASP:
                    if (!(await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayPump, 0)))
                    {
                        log.Error((object)$"Relay Pump {iRelayPump} failed to turn off.");
                        MessageBox.Show($"Relay Pump {iRelayPump} failed to turn off.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                    break;
                case StateCommon.InverterType.RS485:
                    if (!Cls_InverterModbus.SetInverterOnOff(slaveID, onoff: false))
                    {
                        log.Error((object)$"Turn Off Pump {slaveID} failed.");
                        MessageBox.Show($"Turn Off Pump {slaveID} failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                    break;
            }
        }
        private async Task ShowLighTowerError()
        {
            await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayRed, 1);
            await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayYellow, 0);
            await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayGreen, 0);
            await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayBuzzer, 1);
        }
        public async void SwitchRelay3Way_Reverse(bool bOnOff)
        {
            if (bOnOff)
            {
                if (!(await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelay3Way_Reverse, 1)))
                {
                    log.Error((object)$"Relay 3Way Reverse {iRelay3Way_Reverse} failed to turn on.");
                    MessageBox.Show($"Relay 3Way Reverse {iRelay3Way_Reverse} failed to turn on.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
            else if (!(await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelay3Way_Reverse, 0)))
            {
                log.Error((object)$"Relay 3Way Reverse {iRelay3Way_Reverse} failed to turn off.");
                MessageBox.Show($"Relay 3Way Reverse {iRelay3Way_Reverse} failed to turn off.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }
        public string[] GetProcessBefore(string[] proclist,string proc )
        {
            int index = Array.IndexOf(proclist, proc);
            if (index <= 0)
                return new string[0]; // Không tìm thấy hoặc là phần tử đầu tiên

            // Lấy tất cả phần tử từ 0 đến index - 1
            return proclist.Take(index).ToArray();
        }
        public bool[] GetResultprocessBefore(string[]proccheck)
        {
            bool[] result = new bool[proccheck.Length];
            for (int i = 0; i < proccheck.Length; i++)
            {
                string TableName = "";
                string SummaryTableName = "";
                StateCommon.GetTableName(proccheck[i], out TableName, out SummaryTableName);
                string query = $"SELECT [Serial],[Assy_PN],[Date_Time],[Station],[FailCode] " +
                    $"FROM [Production_SZ].[dbo].[{TableName}] where [Serial] ='{_SN}' order by Date_Time";
                System.Data.DataTable dt = ProductionDB.ExecuteQuery(query);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string failCode = row["FailCode"]?.ToString().Trim().ToUpper();
                        if (failCode.Contains("PASS") || failCode.Contains("OK"))
                        {
                            result[i] = true; // Có ít nhất 1 dòng PASS hoặc OK
                            break;
                        }
                        else
                            result[i] = false; // Nếu có dòng nào không PASS hoặc OK, thì không phải trong quá trình này
                    }
                }
                else
                {
                    result[i] = false; // Không có dữ liệu, coi như không thành công
                }
            }
            return result;
        }
        public System.Data.DataTable GetProcessResultsTable(string[] processList,string sSN)
        {
            System.Data.DataTable dtResult = new System.Data.DataTable();
            dtResult.Columns.Add("ProcessName", typeof(string));
            dtResult.Columns.Add("Result", typeof(bool));
            foreach (string p in processList)
            {
                string tableName = "", summaryTable = "";
                StateCommon.GetTableName(p, out tableName, out summaryTable);

                string query = $"SELECT [FailCode] FROM [Production_SZ].[dbo].[{tableName}] WHERE [Serial] = '{sSN}'";
                DataTable dt = ProductionDB.ExecuteQuery(query);

                bool isPass = false;

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string failCode = row["FailCode"]?.ToString().Trim().ToUpper();
                        if (failCode == "PASS" || failCode == "OK")
                        {
                            isPass = true;
                            break;
                        }
                        else
                            isPass = false;
                    }
                }
                dtResult.Rows.Add(p, isPass);
            }
            return dtResult;
        }
        public int IsInProcess(string proc,string sSN, ref DataTable dtResult)
        {
            string[] proceslist = ClsUnitManagercs.cls_Units[iIndex].verifyFlags;
            if(proceslist == null || proceslist.Length == 0)
            {
                return 1;
            }
            // Tìm vị trí process trong danh sách
            int procIndex = Array.IndexOf(proceslist, proc);
            if (procIndex == -1)
                return 2; // Không có trong danh sách verifyFlags
            // Lấy các process đứng trước
            string[] processesBefore = GetProcessBefore(proceslist, proc);
            // Kiểm tra kết quả PASS/OK của các process trước đó
            bool[] resultBefore = GetResultprocessBefore(processesBefore);
            //get table tested
            dtResult = GetProcessResultsTable(proceslist, sSN);
            // Nếu có bất kỳ kết quả nào là false => return false
            if (resultBefore.Any(r => !r))
                return 3;
            //Check passed this station
            bool[] resultcurrent = GetResultprocessBefore(new string[1] { proc });
            if (resultcurrent[0])
                return 4;
            return 0; // Tất cả các process trước đó đều PASS/OK
        }
        private string GetProcesCodeError(int iCode)
        {
            switch (iCode)
            {
                case 0:
                    return "Passed";
                case 1:
                    return "Process list is null, call technical to support!!!";
                case 2:
                    return "Process don't include this station!!!";
                case 3:
                    return "Some station before is missing!!!";
                case 4:
                    return "This product already passed in this staion!!!";
                default:
                    return $"Error is not define {iCode}";
            }
        }
        public bool IsSNValidFormat(string sn)
        {
            sn = sn.ToUpper().Trim();
            if (string.IsNullOrWhiteSpace(sn))
            {
                return false;
            }
            if (sn.Length != ClsUnitManagercs.cls_Units[iIndex].SN_Length)
            {
                return false;
            }
            string text = sn.Substring(ClsUnitManagercs.cls_Units[iIndex].Ctl_Str_Offset, ClsUnitManagercs.cls_Units[iIndex].Ctl_Str_Length);
            if (!text.Equals(ClsUnitManagercs.cls_Units[iIndex].PN_Ctl_String, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }

        private async Task<bool> Setting_Pump2Test(int rery = 5)
        {
            bool bResult = false;
            if (_InverterType == StateCommon.InverterType.ASP)
            {
                bool result = false;
                for (int i = 0; i < rery; i++)
                {
                    result = await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayPump, 0);
                    if (result)
                    {
                        log.Info((object)$"Relay Pump {iRelayPump} turned off successfully.");
                        break;
                    }
                    log.Warn((object)$"Retry {i + 1}/{rery} to turn off Relay Pump {iRelayPump} failed.");
                }
                if (!result) return false;
            }
            if (_InverterType == StateCommon.InverterType.ASP)
            {
                bool result =false;
                for (int i = 0; i < rery; i++)
                {
                    result = await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelay3Way_Air_Water, 0);
                    if (result)
                    {
                        log.Info((object)$"Relay Relay3Way_Air_Water {iRelay3Way_Air_Water} turned off successfully.");
                        break;
                    }
                    log.Warn((object)$"Retry {i + 1}/{rery} to turn off Relay3Way_Air_Water {iRelay3Way_Air_Water} failed.");
                }
                if (!result) return false;
            }
            if (_InverterType == StateCommon.InverterType.ASP)
            {
                bool result =false ;
                for (int i = 0; i < rery; i++)
                {
                    result = await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelay3Way_Reverse, 0);
                    if (result)
                    {
                        log.Info((object)$"Relay iRelay3Way_Reverse {iRelay3Way_Reverse} turned off successfully.");
                        break;
                    }
                    log.Warn((object)$"Retry {i + 1}/{rery} to turn off iRelay3Way_Reverse {iRelay3Way_Reverse} failed.");
                }
                if (!result) return false;
            }
            if (_InverterType == StateCommon.InverterType.ASP)
            {
                bool result = false ;
                for (int i = 0; i < rery; i++)
                {
                    result = await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayPump, 1);
                    if (result)
                    {
                        log.Info((object)$"Relay Pump {iRelayPump} turned on successfully.");
                        break;
                    }
                    log.Warn((object)$"Retry {i + 1}/{rery} to turn on Relay Pump {iRelayPump} failed.");
                }
                if (!result) return false;
            }
            if (_InverterType == StateCommon.InverterType.ASP)
            {
                for (int i = 0; i < rery; i++)
                {
                    int flowRateInt = (int)(ClsUnitManagercs.cls_Units[iIndex].iDi_Flow_Rate * 100.0);
                    if (!(await cls_ASPcontrol.SetFlowRateCheckResult(iADAFlowRate, flowRateInt)))
                    {
                        log.Error((object)$"Failed to set flow rate to {ClsUnitManagercs.cls_Units[iIndex].iDi_Flow_Rate.ToString("0.00")} L/min.");
                        //MessageBox.Show($"Failed to set flow rate to {ClsUnitManagercs.cls_Units[iIndex].iDi_Flow_Rate.ToString("0.00")} L/min.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                    else
                    {
                        log.Info((object)$"Flow rate for ADA {iADAFlowRate} set to {ClsUnitManagercs.cls_Units[iIndex].iDi_Flow_Rate.ToString("0.00")} L/min");
                        bResult = true;
                        break;
                    }
                }
                if (!bResult) return false;
                else
                {
                    log.Info((object)$"Setting-Up flow test successfully.");
                    bResult = true;
                }
            }
            return bResult;
        }
        private async Task<bool> Setting_FlushingWash(int rery =5)
        {
            bool bResult = false;
            if (_InverterType == StateCommon.InverterType.ASP)
            {
                for (int i = 0; i < rery; i++)
                {
                    bool result = await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelay3Way_Reverse, 0);
                    if (result)
                    {
                        bResult = true;
                        log.Info((object)$"Relay iRelay3Way_Reverse {iRelay3Way_Reverse} turned off successfully.");
                        break;
                    }
                    log.Warn((object)$"Retry {i + 1}/{rery} to turn off iRelay3Way_Reverse {iRelay3Way_Reverse} failed.");
                }
                if (!bResult) return false;
                else
                {
                    log.Info((object)$"Setting-Up flushing successfully.");
                }
            }
            return bResult;
        }
        private async Task<bool> Setting_FlushingReverse(int rery = 5)
        {
            bool bResult = false;
            if (_InverterType == StateCommon.InverterType.ASP)
            {
                for (int i = 0; i < rery; i++)
                {
                    bool result = await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelay3Way_Reverse, 1);
                    if (result)
                    {
                        bResult = true;
                        log.Info((object)$"Relay iRelay3Way_Reverse {iRelay3Way_Reverse} turned off successfully.");
                        break;
                    }
                    log.Warn((object)$"Retry {i + 1}/{rery} to turn off iRelay3Way_Reverse {iRelay3Way_Reverse} failed.");
                }
                if (!bResult) return false;
                else
                {
                    log.Info((object)$"Setting-Up flushing successfully.");
                }
            }
            return bResult;
        }
        private void SaveLocalLog()
        {
            string pathlog = "C:\\Aavid_Test\\Datalog";
        }
        private DateTime GetDateTimefromDB()
        {
            DateTime serverTime;
            try
            {
                string query = "SELECT CURRENT_TIMESTAMP";
                object result = ProductionDB.ExecuteScalar(query);
                if (result != null && result is DateTime)
                {
                    serverTime = (DateTime)result;
                }
                else
                {
                    serverTime = DateTime.Now;
                }
            }
            catch (Exception)
            {
                serverTime = DateTime.Now;
            }
            return serverTime;
        }
        public bool SavetoDBMonths_Log(string SN, string PN, string StationID, string FailCode,string proc,string step)
        {
            DateTime dt = GetDateTimefromDB();
            bool b = ProductionDB.SaveToLogMonths_LogTable(SN, PN,dt,StationID, FailCode, proc, step);   
            return b;
        }
        public bool SaveToDBDIWaterWashLog(string SN, string PN,string wo, string StationID, string FailCode, string step)
        {
            DateTime dt = GetDateTimefromDB();
            bool b = ProductionDB.SaveToDIWaterWashLog(SN, PN, StationID, FailCode,"",_TestType,wo, step);
            return b;
        }
        private async Task<bool> Setting_Drying(int rery = 5)
        {
            bool bResult = false;
            if (_InverterType == StateCommon.InverterType.ASP)
            {
                bool result = false;
                for (int i = 0; i < rery; i++)
                {
                    result = await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayPump, 0);
                    if (result)
                    {
                        log.Info((object)$"Relay Pump {iRelayPump} turned off successfully.");
                        break;
                    }
                    log.Warn((object)$"Retry {i + 1}/{rery} to turn off Relay Pump {iRelayPump} failed.");
                }
                if (!result) return false;
            }
            if (_InverterType == StateCommon.InverterType.ASP)
            {
                bool result = false;
                for (int i = 0; i < rery; i++)
                {
                    result = await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelay3Way_Reverse, 0);
                    if (result)
                    {
                        log.Info((object)$"Relay iRelay3Way_Reverse {iRelay3Way_Reverse} turned off successfully.");
                        break;
                    }
                    log.Warn((object)$"Retry {i + 1}/{rery} to turn off Relay iRelay3Way_Reverse {iRelay3Way_Reverse} failed.");
                }
                if (!result) return false;
            }
            if (_InverterType == StateCommon.InverterType.ASP)
            {
                bool result = false;
                for (int i = 0; i < rery; i++)
                {
                    result = await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelay3Way_Air_Water, 1);
                    if (result)
                    {
                        log.Info((object)$"Relay iRelay3Way_Air_Water {iRelay3Way_Reverse} turned on successfully.");
                        break;
                    }
                    log.Warn((object)$"Retry {i + 1}/{rery} to turn off iRelay3Way_Air_Water {iRelay3Way_Reverse} failed.");
                }
                if (!result) return false;
                else
                {
                    log.Info((object)$"Setting-Up drying successfully.");
                    bResult = true;
                }
            }
            return bResult;
        }
        private async Task<bool> Setting_ReverseDrying(int rery = 5)
        {
            bool bResult = false;
            if (_InverterType == StateCommon.InverterType.ASP)
            {
                bool result = false;
                for (int i = 0; i < rery; i++)
                {
                    result = await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayPump, 0);
                    if (result)
                    {
                        log.Info((object)$"Relay Pump {iRelayPump} turned off successfully.");
                        break;
                    }
                    log.Warn((object)$"Retry {i + 1}/{rery} to turn off Relay Pump {iRelayPump} failed.");
                }
                if (!result) return false;
            }
            if (_InverterType == StateCommon.InverterType.ASP)
            {
                bool result = false;
                for (int i = 0; i < rery; i++)
                {
                    result = await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelay3Way_Reverse, 1);
                    if (result)
                    {
                        log.Info((object)$"Relay iRelay3Way_Reverse {iRelay3Way_Reverse} turned on successfully.");
                        break;
                    }
                    log.Warn((object)$"Retry {i + 1}/{rery} to turn on Relay iRelay3Way_Reverse {iRelay3Way_Reverse} failed.");
                }
                if (!result) return false;
            }
            if (_InverterType == StateCommon.InverterType.ASP)
            {
                bool result = false;
                for (int i = 0; i < rery; i++)
                {
                    result = await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelay3Way_Air_Water, 1);
                    if (result)
                    {
                        log.Info((object)$"Relay iRelay3Way_Air_Water {iRelay3Way_Reverse} turned on successfully.");
                        break;
                    }
                    log.Warn((object)$"Retry {i + 1}/{rery} to turn off iRelay3Way_Air_Water {iRelay3Way_Reverse} failed.");
                }
                if (!result) return false;
                else
                {
                    log.Info((object)$"Setting-Up drying successfully.");
                    bResult = true;
                }
            }
            return bResult;
        }
        public async void LoopTest()
        {
            
            if (!_AutoMode)
            {
                return;
            }
            if(_testSeq == TestSeq.ERROR)
            {
                log.Error((object)"Test sequence is in ERROR state. Cannot proceed.");
                return;
            }
            if (_testSeq == TestSeq.WAIT)
            {
                _SN = "";
                return;
            }
            switch (_testSeq)
            {
                case TestSeq.SN_INSERT:
                    {
                        int num = 5000;
                        int num2 = 0;
                        int num3 = 100;
                        AccessInsertSN = true;
                        while (string.IsNullOrWhiteSpace(_SN) && num2 < num)
                        {
                            Thread.Sleep(num3);
                            num2 += num3;
                        }
                        if (string.IsNullOrWhiteSpace(_SN))
                        {
                            log.Warn((object)"Timeout: No SN entered within 5 seconds.");
                            _testSeq = TestSeq.WAIT;
                        }
                        else
                        {
                            log.Info((object)("SN received: " + _SN));
                            _testSeq = TestSeq.SN_VERIFY;
                        }
                        AccessInsertSN = false;
                        break;
                    }
                case TestSeq.SN_VERIFY:
                    if (TestType != "Engineering Test")
                    {
                        if (IsSNValidFormat(_SN))
                        {
                            log.Info((object)("SN " + _SN + " is valid."));
                            _testSeq = TestSeq.SN_CHECK_ROUTER;
                        }
                        else
                        {
                            MessageBox.Show(_SN + " is invalid. Please re-enter.", "Invalid SN", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            log.Warn((object)("SN " + _SN + " is invalid. Please re-enter."));
                            _testSeq = TestSeq.WAIT;
                        }
                    }
                    else
                        _testSeq = TestSeq.SN_CHECK_ROUTER;
                    break;
                case TestSeq.SN_CHECK_ROUTER:
                    if(TestType != "Engineering Test")
                    {
                        DataTable dtResult = new DataTable();
                        int iCheckRouter = IsInProcess("DI_Water_Wash", _SN, ref dtResult);
                        AddProcessLogTest(dtResult);
                        if (iCheckRouter == 0) _testSeq = TestSeq.SN_CHECK_ROUTER_OK;
                        else
                        {
                            MessageBox.Show(GetProcesCodeError(iCheckRouter), "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            log.Warn((object)GetProcesCodeError(iCheckRouter) + _SN);
                            _testSeq = TestSeq.WAIT;
                        }
                    }
                    else
                        _testSeq = TestSeq.SN_CHECK_ROUTER_OK;
                    break;
                case TestSeq.SN_CHECK_ROUTER_OK:
                    ClsUnitManagercs.cls_Units[iIndex].cls_SequencyCommon.process = StateCommon.ProcessState.Running;
                    _StepTesting = 0;
                    _testSeq = TestSeq.SETTING_PUMP;
                    break;
                case TestSeq.SETTING_PUMP:
                    bool bResult = await Setting_Pump2Test(5);
                    if (bResult)
                    {
                        log.Info((object)$"Pump and relays set successfully for SN {_SN}.");
                        _testSeq = TestSeq.TURN_FLUSHING_WASHING;
                    }
                    else
                    {
                        log.Error((object)$"Failed to set pump and relays for SN {_SN}.");
                        MessageBox.Show($"Failed to set pump and relays for SN {_SN}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        await ShowLighTowerError();
                        _testSeq = TestSeq.ERROR;
                        ClsUnitManagercs.cls_Units[iIndex].cls_SequencyCommon.process = StateCommon.ProcessState.Error;
                    }
                    break;
                case TestSeq.TURN_FLUSHING_WASHING:
                    bool bFlushing = await Setting_FlushingWash(5);
                    if (bFlushing)
                    {
                        log.Info((object)$"flushing relay set successfully for SN {_SN}.");
                        _testSeq = TestSeq.PRE_WASHING;
                    }
                    else
                    {
                        log.Error((object)$"Failed to flushing relay set for SN {_SN}.");
                        MessageBox.Show($"Failed to flushing relay set for SN {_SN}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        await ShowLighTowerError();
                        _testSeq = TestSeq.ERROR;
                        ClsUnitManagercs.cls_Units[iIndex].cls_SequencyCommon.process = StateCommon.ProcessState.Error;
                    }
                    break;  
                case TestSeq.PRE_WASHING:
                    sw_prewash = new Stopwatch();
                    sw_prewash.Start();
                    while (sw_prewash.ElapsedMilliseconds < ClsUnitManagercs.cls_Units[iIndex].iPre_Washing_Time * 1000)
                    {
                        if (ClsUnitManagercs.cls_Units[iIndex].cls_SequencyCommon.process == StateCommon.ProcessState.Error)
                        {
                            log.Error((object)$"Process error during testing for SN {_SN}.");
                            await ShowLighTowerError();
                            _testSeq = TestSeq.ERROR;
                            return;
                        }
                        Thread.Sleep(100);
                    }
                    sw_prewash.Stop();
                    _testSeq = TestSeq.FLUSHING_WASHING;
                    break;
                case TestSeq.FLUSHING_WASHING:
                    sw_flush = new Stopwatch();
                    sw_flush.Start();
                    while (sw_flush.ElapsedMilliseconds < ClsUnitManagercs.cls_Units[iIndex].iWashing_Time * 1000)
                    {
                        if (ClsUnitManagercs.cls_Units[iIndex].cls_SequencyCommon.process == StateCommon.ProcessState.Error)
                        {
                            log.Error((object)$"Process error during testing for SN {_SN}.");
                            await ShowLighTowerError();
                            _testSeq = TestSeq.ERROR;
                            return;
                        }
                        Thread.Sleep(100);
                    }
                    sw_flush.Stop();
                    _testSeq = TestSeq.TURN_REVERSE_WASHING;
                    break;
                case TestSeq.TURN_REVERSE_WASHING:
                    bool bReverse = await Setting_FlushingReverse(5);
                    if (bReverse)
                    {
                        log.Info((object)$"Reverse relay set successfully for SN {_SN}.");
                        _testSeq = TestSeq.REVERSE_WASHING;
                    }
                    else
                    {
                        log.Error((object)$"Failed to Reverse relay set for SN {_SN}.");
                        MessageBox.Show($"Failed to Reverse relay set for SN {_SN}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        await ShowLighTowerError();
                        _testSeq = TestSeq.ERROR;
                        ClsUnitManagercs.cls_Units[iIndex].cls_SequencyCommon.process = StateCommon.ProcessState.Error;
                    }
                    break;
                case TestSeq.REVERSE_WASHING:
                    _StepTesting++;
                    if (ClsUnitManagercs.cls_Units[iIndex].bReverse_Washing_Flow)
                    {
                        sw_reverse = new Stopwatch();
                        sw_reverse.Start();
                        while (sw_reverse.ElapsedMilliseconds < ClsUnitManagercs.cls_Units[iIndex].iDI_Reverse_Washing_Time * 1000)
                        {
                            if (ClsUnitManagercs.cls_Units[iIndex].cls_SequencyCommon.process == StateCommon.ProcessState.Error)
                            {
                                log.Error((object)$"Process error during testing for SN {_SN}.");
                                await ShowLighTowerError();
                                _testSeq = TestSeq.ERROR;
                                return;
                            }
                            Thread.Sleep(100);
                        }
                        sw_reverse.Stop();
                    }    
                    if(_StepTesting >= ClsUnitManagercs.cls_Units[iIndex].iWash_Cycle)
                    {
                        _testSeq = TestSeq.TURN_DRYING;
                    }
                    else
                    {
                        _testSeq = TestSeq.TURN_FLUSHING_WASHING;
                    }
                    break;
                case TestSeq.TURN_DRYING:
                    bool bdry = await Setting_Drying(5);
                    if (bdry)
                    {
                        log.Info((object)$"Drying relay set successfully for SN {_SN}.");
                        _testSeq = TestSeq.DRYING;
                    }
                    else
                    {
                        log.Error((object)$"Failed to Drying relay set for SN {_SN}.");
                        MessageBox.Show($"Failed to Drying relay set for SN {_SN}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        await ShowLighTowerError();
                        _testSeq = TestSeq.ERROR;
                        ClsUnitManagercs.cls_Units[iIndex].cls_SequencyCommon.process = StateCommon.ProcessState.Error;
                    }
                    break;
                case TestSeq.DRYING:
                    sw_drying = new Stopwatch();
                    sw_drying.Start();
                    while (sw_drying.ElapsedMilliseconds < ClsUnitManagercs.cls_Units[iIndex].iDI_Drying_Time * 1000)
                    {
                        if (ClsUnitManagercs.cls_Units[iIndex].cls_SequencyCommon.process == StateCommon.ProcessState.Error)
                        {
                            log.Error((object)$"Process error during testing for SN {_SN}.");
                            await ShowLighTowerError();
                            _testSeq = TestSeq.ERROR;
                            return;
                        }
                        Thread.Sleep(100);
                    }
                    sw_drying.Stop();
                    _testSeq = TestSeq.TURN_REVERSE_DRYING;
                    break;
                case TestSeq.TURN_REVERSE_DRYING:
                    bool breversedry = await Setting_ReverseDrying(5);
                    if (breversedry)
                    {
                        log.Info((object)$"Reverse Drying relay set successfully for SN {_SN}.");
                        _testSeq = TestSeq.REVERSE_DRYING;
                    }
                    else
                    {
                        log.Error((object)$"Failed to Reverse Drying relay set for SN {_SN}.");
                        MessageBox.Show($"Failed to Reverse Drying relay set for SN {_SN}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        await ShowLighTowerError();
                        _testSeq = TestSeq.ERROR;
                        ClsUnitManagercs.cls_Units[iIndex].cls_SequencyCommon.process = StateCommon.ProcessState.Error;
                    }
                    break;
                case TestSeq.REVERSE_DRYING:
                    if (ClsUnitManagercs.cls_Units[iIndex].bReverse_DI_Flushing_Flow)
                    {
                        sw_reverse_drying = new Stopwatch();
                        sw_reverse_drying.Start();
                        while (sw_reverse_drying.ElapsedMilliseconds < ClsUnitManagercs.cls_Units[iIndex].iReverse_DI_Drying_Time * 1000)
                        {
                            if (ClsUnitManagercs.cls_Units[iIndex].cls_SequencyCommon.process == StateCommon.ProcessState.Error)
                            {
                                log.Error((object)$"Process error during testing for SN {_SN}.");
                                await ShowLighTowerError();
                                _testSeq = TestSeq.ERROR;
                                return;
                            }
                            Thread.Sleep(100);
                        }
                        sw_reverse_drying.Stop();
                    }
                    _testSeq = TestSeq.CHECKING_RESULT;
                    break;
                case TestSeq.CHECKING_RESULT:
                    if(ClsUnitManagercs.cls_Units[iIndex].bCheck_DI_Huminity)
                    {
                        //Check huminity
                    }
                    _testSeq = TestSeq.TEST_PASS;
                    break;
                case TestSeq.TEST_PASS:
                    ClsUnitManagercs.cls_Units[iIndex].cls_SequencyCommon.process = StateCommon.ProcessState.CompletedPass;
                    TestResult = "PASS";
                    _testSeq = TestSeq.CREATE_LOCAL_LOG;
                    break;
                case TestSeq.TEST_FAIL:
                    ClsUnitManagercs.cls_Units[iIndex].cls_SequencyCommon.process = StateCommon.ProcessState.CompletedFail;
                    TestResult = "FAIL";
                    _testSeq = TestSeq.CREATE_LOCAL_LOG;
                    break;
                case TestSeq.CREATE_LOCAL_LOG:
                    _testSeq = TestSeq.SAVE_DB;
                    break;
                case TestSeq.SAVE_DB:
                    string PN = ClsUnitManagercs.cls_Units[iIndex].AssyPN;
                    string WO = ClsUnitManagercs.cls_Units[iIndex].WO;
                    string Station = ClsUnitManagercs.cls_Units[iIndex].StaionID;
                    if (TestType == "Engineering Test") TestResult = "FAIL";
                    if (!SavetoDBMonths_Log(_SN, PN ,Station, TestResult, "DI_Water_Wash" , "1"))
                    {
                        MessageBox.Show("Failed to save DB months log failed.");
                        await ShowLighTowerError();
                        _testSeq = TestSeq.ERROR;
                        break;
                    }
                    if (!SaveToDBDIWaterWashLog(_SN, PN,WO, Station, TestResult,"1"))
                    {
                        MessageBox.Show("Failed to save DB DI_Water log failed.");
                        await ShowLighTowerError();
                        _testSeq = TestSeq.ERROR;
                        break;
                    }
                    _testSeq = TestSeq.WAIT;
                    break;
            }
        }
        public void AddProcessLogTest(DataTable dtResult)
        {
            OnRequestAddProcessLogTest?.Invoke(this, new ProcessLogEventArgs
            {
                UnitIndex = this.iIndex,
                LogData = dtResult
            });
        }
    }
}
