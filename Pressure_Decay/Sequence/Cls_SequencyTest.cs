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
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

public class Cls_SequencyTest
{
    public delegate void DelShow_UpdateHumidityTemp_Request(Cls_SequencyTest sender);

    public enum TestSeq
    {
        NONE = -2, // Trạng thái không có quá trình nào đang chạy
        ERROR = -1,
        WAIT = 9999,
        FIXTURE_CLOSE = 0,
        SN_INSERT_1 = 1,
        SN_INSERT_2 = 2,
        SN_INSERT_3 = 3,
        SN_INSERT_4 = 4,
        SN_VERIFY = 5,
        SN_CHECK_ROUTER = 6,
        SN_CHECK_ROUTER_OK = 7,
        START_TEST = 8,
        CHECK_START = 9,
        CHECKING_RESULT = 10,
        FIXTURE_OPEN = 11,
        TEST_PASS = 12,
        SET_LIGHT_TOWER_PASS = 13,
        TEST_FAIL = 14,
        SET_LIGHT_TOWER_FAIL = 15,
        CREATE_LOCAL_LOG = 16,
        SAVE_DB = 17,
        END_TEST = 18,
    }

    //public delegate void DelShow_AddProcessLogTest_Request(DataTable sender);
    public event EventHandler<ProcessLogEventArgs> OnRequestAddProcessLogTest;
    public class ProcessLogEventArgs : EventArgs
    {
        public int UnitIndex { get; set; }
        public System.Data.DataTable LogData { get; set; }
    }
    private bool _isTestResultShown = false;
    private static readonly ILog log = LogManager.GetLogger(typeof(Cls_ASPcontrol));
    private string TestResult;
    private Cls_ASPcontrol cls_ASPcontrol;
    private Cls_LS_R902 cls_LSR902;
    Cls_DBMsSQL ProductionDB = new Cls_DBMsSQL();
    private bool _AutoMode = true;
    private bool _AutoScanner = true;
    private int _ScannerNo = 1;
    private bool _AutoFixture = false;
    private bool _AutoDecayTest = true;
    public bool AutoDecayTest
    {
        get { return _AutoDecayTest; }
        set { _AutoDecayTest = value; }
    }
    public bool AutoScanner
    {
        get { return _AutoScanner; }
        set { _AutoScanner = value; }
    }
    public int ScannerNo
    {
        get { return _ScannerNo; }
        set { _ScannerNo = value; }
    }
    public bool AutoFixture
    {
        get { return _AutoFixture; }
        set { _AutoFixture = value; }
    }
    private bool _isRequestCloseFixture = false;
    private bool _isCheckingStartTester = false;
    private bool _isWaitingResult = false;
    private bool _isWaitingSN = false;
    private bool _isShow2RequestStartDecay = false;
    private string _SN1 = "";
    private string _SN2 = "";
    private string _SN3 = "";
    private string _SN4 = "";
    private string[] _SNs = new string[4];
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
            if (_testSeq == TestSeq.WAIT || _testSeq == TestSeq.ERROR)
            {
                _testSeq = value;
            }
        }
    }
    public string sErrorCode { get; private set; } = "";
    public int iRelayStart1 { get; private set; }
    public int iRelayStop1 { get; private set; }
    public int iRelayStart2 { get; private set; }
    public int iRelayStop2 { get; private set; }
    public int iRelayFixtureDoor1 { get; private set; }

    public int iRelayFixtureCylinder1 { get; private set; }
    public int iRelayFixtureDoor2 { get; private set; }
    public int iRelayFixtureCylinder2 { get; private set; }
    public int iIndex { get; private set; }
    public int iInputQD1 { get; private set; }
    public int iInputStart { get; private set; }
    public int iInputEMG { get; private set; }
    public int iInputDoorUp1 { get; private set; }
    public int iInputDoorDown1 { get; private set; }
    public int iInputIn1 { get; private set; }
    public int iInputOut1 { get; private set; }
    public int iInputQD2 { get; private set; }
    public int iInputDoorUp2 { get; private set; }
    public int iInputDoorDown2 { get; private set; }
    public int iInputIn2 { get; private set; }
    public int iInputOut2 { get; private set; }
    public int iInputBusy1 { get; private set; }
    public int iInputBusy2 { get; private set; }
    public int iFixtureNo { get; private set; } 
    public bool bQD1 = false;
    public bool bQD2 = false;
    public bool bStart = false;
    public bool bEMG = false;
    public bool bDoorUp1 = false;
    public bool bDoorDown1 = false;
    public bool bIn1 = false;
    public bool bOut1 = false;
    public bool bBusy1 = false;
    public bool bBusy2 = false;
    public bool bDoorUp2 = false;
    public bool bDoorDown2 = false;
    public bool bIn2 = false;
    public bool bOut2 = false;
    private string _TestType = "Production Test";
    public Stopwatch sw_decayTest { get; private set; } = new Stopwatch();
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
    public string SN1
    {
        get
        {
            return _SN1;
        }
        set
        {
            if (AccessInsertSN)
            {
                _SN1 = value;
            }
            else
            {
                log.Warn((object)"Attempt to set SN when access is not allowed.");
            }
        }
    }
    public string SN2
    {
        get
        {
            return _SN2;
        }
        set
        {
            if (AccessInsertSN)
            {
                _SN2 = value;
            }
            else
            {
                log.Warn((object)"Attempt to set SN when access is not allowed.");
            }
        }
    }
    public string SN3
    {
        get
        {
            return _SN3;
        }
        set
        {
            if (AccessInsertSN)
            {
                _SN3 = value;
            }
            else
            {
                log.Warn((object)"Attempt to set SN when access is not allowed.");
            }
        }
    }
    public string SN4
    {
        get
        {
            return _SN4;
        }
        set
        {
            if (AccessInsertSN)
            {
                _SN4 = value;
            }
            else
            {
                log.Warn((object)"Attempt to set SN when access is not allowed.");
            }
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

    public event DelShow_UpdateHumidityTemp_Request OnRequestUpdateHumidityTemp;

    public Cls_SequencyTest(Cls_ASPcontrol cls_ASP, Cls_LS_R902 clsLSR902, bool isAutoMode = true, int ifixtureNo = 2)
    {
        cls_ASPcontrol = cls_ASP;
        cls_LSR902 = clsLSR902;
        _AutoMode = isAutoMode;
        iFixtureNo = ifixtureNo;
        iRelayFixtureCylinder1 = 1;
        iRelayFixtureDoor1 = 2;
        iInputQD1 = 11;
        iInputStart = 1;
        iInputEMG = 2;
        iInputIn1 = -1; // ASP board don't support Input 12-20
        iInputOut1 = 8;
        iInputDoorUp1 = 5;
        iInputDoorDown1 = 13;
        iInputBusy1 = 7; // ASP board don't support Input 12-20
        iRelayFixtureCylinder2 = 3;
        iRelayFixtureDoor2 = 4;
        iInputQD2 = 12;
        iInputIn2 = -1;// ASP board don't support Input 12-20
        iInputOut2 = 10;
        iInputDoorUp2 = 6;
        iInputDoorDown2 = 14;
        iInputBusy2 = 8;// ASP board don't support Input 12-20
        iRelayStart1 = 5;
        iRelayStart2 = 6;
        iRelayStop1 = 7;
        iRelayStop2 = 8;
        Thread thrGetInput = new Thread(() => { LoopGetInput(); });
        thrGetInput.Name = "Get Input from ASP interface";
        thrGetInput.IsBackground = true;
        thrGetInput.Start();
        _testSeq = TestSeq.WAIT;
        _SNs = new string[4] {_SN1,_SN2,_SN3,_SN4};
        ProductionDB.Initialize("10.102.4.20", "Production_SZ", "sa", "nuventixleo");
        ProductionDB.Open();
    }
    public async void LoopGetInput()
    {
        while (true)
        {
            if(_AutoMode)
            {
                await cls_ASPcontrol.GetAllInput();
                for (int i=0;i< cls_ASPcontrol.inputStates.Length; i++)
                {
                    int iInput = i + 1;
                    if (iInput == iInputQD1)
                    {
                        bQD1 = cls_ASPcontrol.inputStates[i];
                    }
                    else if (iInput == iInputQD2)
                    {
                        bQD2 = cls_ASPcontrol.inputStates[i];
                    }
                    else if (iInput == iInputStart)
                    {
                        bStart = cls_ASPcontrol.inputStates[i];
                    }
                    else if (iInput == iInputEMG)
                    {
                        bEMG = cls_ASPcontrol.inputStates[i];
                    }
                    else if (iInput == iInputDoorUp1)
                    {
                        bDoorUp1 = cls_ASPcontrol.inputStates[i];
                    }
                    else if (iInput == iInputDoorDown1)
                    {
                        bDoorDown1 = cls_ASPcontrol.inputStates[i];
                    }
                    else if (iInput == iInputIn1)
                    {
                        bIn1 = cls_ASPcontrol.inputStates[i];
                    }
                    else if (iInput == iInputOut1)
                    {
                        bOut1 = cls_ASPcontrol.inputStates[i];
                    }
                    else if (iInput == iInputDoorUp2)
                    {
                        bDoorUp2 = cls_ASPcontrol.inputStates[i];
                    }
                    else if (iInput == iInputDoorDown2)
                    {
                        bDoorDown2 = cls_ASPcontrol.inputStates[i];
                    }
                    else if (iInput == iInputIn2)
                    {
                        bIn2 = cls_ASPcontrol.inputStates[i];
                    }
                    else if (iInput == iInputOut2)
                    {
                        bOut2 = cls_ASPcontrol.inputStates[i];
                    }
                    else if (iInput == iInputBusy1)
                    {
                        bBusy1 = cls_ASPcontrol.inputStates[i];
                    }
                    else if (iInput == iInputBusy2)
                    {
                        bBusy2 = cls_ASPcontrol.inputStates[i];
                    }
                }
            }
            Thread.Sleep(1000);
        }
    }
    public async Task<bool> OpenFixture_1()
    {  
        if(!await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayFixtureDoor1, 0))
            return false;
            Thread.Sleep(2000);
        if(!await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayFixtureCylinder1, 0))
            return false;
            Thread.Sleep(2000);
        return true;
    }
    public async Task<bool> OpenFixture_2()
    {
        if (!await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayFixtureDoor2, 0))
            return false;
        Thread.Sleep(2000);
        if (!await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayFixtureCylinder2, 0))
            return false;
        Thread.Sleep(2000);
        return true;
    }
    public async Task<bool> CloseFixture_1()
    {
        if (!await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayFixtureDoor1, 0))
            return false;
        Thread.Sleep(2000);
        if (!await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayFixtureCylinder1, 1))
            return false;
        Thread.Sleep(2000);
        if (!await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayFixtureDoor1, 1))
            return false;
        Thread.Sleep(2000);
        return true;
    }
    public async Task<bool> CloseFixture_2()
    {
        if (!await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayFixtureDoor2, 0))
            return false;
        Thread.Sleep(2000);
        if (!await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayFixtureCylinder2, 1))
            return false;
        Thread.Sleep(2000);
        if (!await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayFixtureDoor2, 1))
            return false;
        Thread.Sleep(2000);
        return true;
    }
    public string[] GetProcessBefore(string[] proclist,string proc )
    {
        int index = Array.IndexOf(proclist, proc);
        if (index <= 0)
            return new string[0]; // Không tìm thấy hoặc là phần tử đầu tiên

        // Lấy tất cả phần tử từ 0 đến index - 1
        return proclist.Take(index).ToArray();
    }
    public bool[] GetResultprocessBefore(string[]proccheck,string SN)
    {
        bool[] result = new bool[proccheck.Length];
        for (int i = 0; i < proccheck.Length; i++)
        {
            string TableName = "";
            string SummaryTableName = "";
            StateCommon.GetTableName(proccheck[i], out TableName, out SummaryTableName);
            string query = $"SELECT [Serial],[Assy_PN],[Date_Time],[Station],[FailCode] " +
                $"FROM [Production_SZ].[dbo].[{TableName}] where [Serial] ='{SN}' order by Date_Time";
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
        string[] proceslist = ClsUnitManagercs.cls_Units.verifyFlags;
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
        bool[] resultBefore = GetResultprocessBefore(processesBefore, sSN);
        //get table tested
        dtResult = GetProcessResultsTable(proceslist, sSN);
        // Nếu có bất kỳ kết quả nào là false => return false
        if (resultBefore.Any(r => !r))
            return 3;
        //Check passed this station
        bool[] resultcurrent = GetResultprocessBefore(new string[1] { proc }, sSN);
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
        if (sn.Length != ClsUnitManagercs.cls_Units.SN_Length)
        {
            return false;
        }
        string text = sn.Substring(ClsUnitManagercs.cls_Units.Ctl_Str_Offset, ClsUnitManagercs.cls_Units.Ctl_Str_Length);
        if (!text.Equals(ClsUnitManagercs.cls_Units.PN_Ctl_String, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        return true;
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
    public bool SaveToDBPressureDecayLog(string SN, string PN, string wo, string StationID, string FailCode,string Param_Value, string pressuredrop)
    {
        DateTime dt = GetDateTimefromDB();
        bool b = ProductionDB.SaveToPressureDecayLog(SN, PN, StationID, FailCode,Param_Value, "", _TestType, wo, pressuredrop);
        return b;
    }
    public async void LoopTest()
    {
            
        if (!_AutoMode)
        {
            return;
        }
        if (_testSeq == TestSeq.ERROR)
        {
            _SN1 = "";
            _SN2 = "";
            _SN3 = "";
            _SN4 = "";
            _testSeq = TestSeq.WAIT;
            return;
        }
        if (_testSeq == TestSeq.WAIT)
        {
            ClsUnitManagercs.cls_Units.cls_SequencyCommon.process = StateCommon.ProcessState.Idle;
            TestResult = "FAIL";
            if (bQD1 & bQD2 & bStart && !bEMG && AutoFixture)
            {
                _testSeq = TestSeq.FIXTURE_CLOSE;
            }
            else if(!AutoFixture && bStart)
                _testSeq = TestSeq.SN_INSERT_1;
            _SN1 = "";
            _SN2 = "";
            _SN3 = "";
            _SN4 = "";
            return;
        }

        switch (_testSeq)
        {
            case TestSeq.FIXTURE_CLOSE:
                if (_isRequestCloseFixture) break;
                _isRequestCloseFixture = true;
                if (!await CloseFixture_1())
                {
                    MessageBox.Show("Fixture 1 can not close.", "Fixture Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _testSeq = TestSeq.ERROR;
                }
                if (!await CloseFixture_2())
                {
                    MessageBox.Show("Fixture 2 can not close.", "Fixture Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _testSeq = TestSeq.ERROR;
                }
                Thread.Sleep(2000);
                _testSeq = TestSeq.SN_INSERT_1;
                _isRequestCloseFixture = false;
                break;
            case TestSeq.SN_INSERT_1:
                if (_isWaitingSN) break;
                _isWaitingSN = true;
                if(_AutoScanner)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        bool bscaned = await cls_ASPcontrol.RequestTriggerScanner(1);
                        if (bscaned) break;
                        if (i == 2)
                        {
                            MessageBox.Show("Scanner 1 can not trigger. Please check connection.", "Scanner Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            _testSeq = TestSeq.ERROR;
                            _isWaitingSN = false;
                            return;
                        }
                    }
                    _SN1 = cls_ASPcontrol.SerialNumber1.Trim().ToUpper();
                }
                else 
                {
                    AccessInsertSN = true;
                    while (string.IsNullOrWhiteSpace(_SN1))
                        Thread.Sleep(100);
                }
                _SNs[0] = _SN1;
                _isWaitingSN = false;
                if (_ScannerNo == 1)
                {
                    _testSeq = TestSeq.SN_VERIFY;
                }
                else
                    _testSeq = TestSeq.SN_INSERT_2;
                break;
            case TestSeq.SN_INSERT_2:
                if (_isWaitingSN) break;
                _isWaitingSN = true;
                if (_AutoScanner)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (await cls_ASPcontrol.RequestTriggerScanner(2)) break;
                        if (i == 3)
                        {
                            MessageBox.Show("Scanner 2 can not trigger. Please check connection.", "Scanner Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            _testSeq = TestSeq.ERROR;
                            return;
                        }
                    }
                    _SN2 = cls_ASPcontrol.SerialNumber2.Trim().ToUpper();
                }
                else
                {
                    int num = 10000;
                    int num2 = 0;
                    int num3 = 100;
                    AccessInsertSN = true;
                    while (string.IsNullOrWhiteSpace(_SN2) && num2 < num)
                    {
                        Thread.Sleep(num3);
                        num2 += num3;
                    }
                    if (string.IsNullOrWhiteSpace(_SN2))
                    {
                        log.Warn((object)"Timeout: No SN entered within 5 seconds.");
                        _testSeq = TestSeq.WAIT;
                    }
                    else
                    {
                        log.Info((object)("SN received: " + _SN2));
                    }
                    AccessInsertSN = false;
                }
                _SNs[1] = _SN2;
                _isWaitingSN = false;
                if (_ScannerNo == 2)
                {
                    _testSeq = TestSeq.SN_VERIFY;
                }
                else if (_ScannerNo == 4)
                {
                    _testSeq = TestSeq.SN_INSERT_3;
                }
                break;
            case TestSeq.SN_INSERT_3:
                if (_isWaitingSN) break;
                _isWaitingSN = true;
                if (_AutoScanner)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (await cls_ASPcontrol.RequestTriggerScanner(3)) break;
                        if (i == 3)
                        {
                            MessageBox.Show("Scanner 3 can not trigger. Please check connection.", "Scanner Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            _testSeq = TestSeq.ERROR;
                            return;
                        }
                    }

                    _SN3 = cls_ASPcontrol.SerialNumber3.Trim().ToUpper();
                }
                else
                {
                    int num = 10000;
                    int num2 = 0;
                    int num3 = 100;
                    AccessInsertSN = true;
                    while (string.IsNullOrWhiteSpace(_SN3) && num2 < num)
                    {
                        Thread.Sleep(num3);
                        num2 += num3;
                    }
                    if (string.IsNullOrWhiteSpace(_SN3))
                    {
                        log.Warn((object)"Timeout: No SN entered within 5 seconds.");
                        _testSeq = TestSeq.WAIT;
                    }
                    else
                    {
                        log.Info((object)("SN received: " + _SN3));
                    }
                    AccessInsertSN = false;
                }
                _SNs[2] = _SN3;
                _isWaitingSN = false;
                _testSeq = TestSeq.SN_INSERT_4;
                break;
            case TestSeq.SN_INSERT_4:
                if (_isWaitingSN) break;
                _isWaitingSN = true;
                if (_AutoScanner)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (await cls_ASPcontrol.RequestTriggerScanner(4)) break;
                        if (i == 3)
                        {
                            MessageBox.Show("Scanner 4 can not trigger. Please check connection.", "Scanner Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            _testSeq = TestSeq.ERROR;
                            return;
                        }
                    }
                }
                else
                {
                    int num = 10000;
                    int num2 = 0;
                    int num3 = 100;
                    AccessInsertSN = true;
                    while (string.IsNullOrWhiteSpace(_SN4) && num2 < num)
                    {
                        Thread.Sleep(num3);
                        num2 += num3;
                    }
                    if (string.IsNullOrWhiteSpace(_SN4))
                    {
                        log.Warn((object)"Timeout: No SN entered within 5 seconds.");
                        _testSeq = TestSeq.WAIT;
                    }
                    else
                    {
                        log.Info((object)("SN received: " + _SN4));
                    }
                    AccessInsertSN = false;
                }
                _SN4 = cls_ASPcontrol.SerialNumber4.Trim().ToUpper();
                _SNs[4] = _SN4;
                _isWaitingSN = false;
                _testSeq = TestSeq.SN_VERIFY;
                break;
            case TestSeq.SN_VERIFY:
                sErrorCode = "";
                if (TestType != "Engineering Test")
                {
                    if (_ScannerNo == 1)
                    {
                        for (int i = 0; i < 1; i++)
                        {
                            if (IsSNValidFormat(_SNs[i]))
                            {
                                log.Info((object)("SN " + _SNs[i] + " is valid."));
                                _testSeq = TestSeq.SN_CHECK_ROUTER;
                            }
                            else
                            {
                                sErrorCode = $"SN {_SNs[i]} is invalid.";
                                _testSeq = TestSeq.ERROR;
                                log.Warn((object)("SN " + _SNs[i] + " is invalid. Please re-enter."));
                                break;
                            }
                        }
                    }
                    else if (_ScannerNo == 2)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if (IsSNValidFormat(_SNs[i]))
                            {
                                log.Info((object)("SN " + _SNs[i] + " is valid."));
                                _testSeq = TestSeq.SN_CHECK_ROUTER;
                            }
                            else
                            {
                                sErrorCode = $"SN {_SNs[i]} is invalid.";
                                _testSeq = TestSeq.ERROR;
                                log.Warn((object)("SN " + _SNs[i] + " is invalid. Please re-enter."));
                                break;
                            }
                        }
                    }
                    else if (_ScannerNo == 4)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (IsSNValidFormat(_SNs[i]))
                            {
                                log.Info((object)("SN " + _SNs[i] + " is valid."));
                                _testSeq = TestSeq.SN_CHECK_ROUTER;
                            }
                            else
                            {
                                sErrorCode = $"SN {_SNs[i]} is invalid.";
                                _testSeq = TestSeq.ERROR;
                                log.Warn((object)("SN " + _SNs[i] + " is invalid. Please re-enter."));
                                break;
                            }
                        }
                    }
                }
                else
                    _testSeq = TestSeq.SN_CHECK_ROUTER;
                break;
            case TestSeq.SN_CHECK_ROUTER:
                if(TestType != "Engineering Test")
                {
                    if (_ScannerNo == 1)
                    {
                        for (int i = 0; i < 1; i++)
                        {
                            DataTable dtResult = new DataTable();
                            int iCheckRouter = IsInProcess("Pressure_Decay", _SNs[i], ref dtResult);
                            AddProcessLogTest(i, dtResult);
                            if(iCheckRouter == 0)
                                _testSeq = TestSeq.SN_CHECK_ROUTER_OK;
                            else
                            {
                                sErrorCode = $"SN {_SNs[i]} already passed";
                                _testSeq = TestSeq.ERROR;
                                break;
                            }    
                        }
                    }
                    if (_ScannerNo == 2)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            DataTable dtResult = new DataTable();
                            int iCheckRouter = IsInProcess("Pressure_Decay", _SNs[i], ref dtResult);
                            AddProcessLogTest(i,dtResult);
                            if (iCheckRouter == 0)
                                _testSeq = TestSeq.SN_CHECK_ROUTER_OK;
                            else
                            {
                                sErrorCode = $"SN {_SNs[i]} already passed";
                                _testSeq = TestSeq.ERROR;
                                break;
                            }
                            _testSeq = TestSeq.SN_CHECK_ROUTER_OK;
                        }
                    }
                    else if (_ScannerNo == 4)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            DataTable dtResult = new DataTable();
                            int iCheckRouter = IsInProcess("Pressure_Decay", _SNs[i], ref dtResult);
                            AddProcessLogTest(i, dtResult);
                            if (iCheckRouter == 0)
                                _testSeq = TestSeq.SN_CHECK_ROUTER_OK;
                            else
                            {
                                sErrorCode = $"SN {_SNs[i]} already passed";
                                _testSeq = TestSeq.ERROR;
                                break;
                            }
                            _testSeq = TestSeq.SN_CHECK_ROUTER_OK;
                        }
                    }
                }
                else
                    _testSeq = TestSeq.SN_CHECK_ROUTER_OK;
                break;
            case TestSeq.SN_CHECK_ROUTER_OK:
                ClsUnitManagercs.cls_Units.cls_SequencyCommon.process = StateCommon.ProcessState.Running;
                _testSeq = TestSeq.START_TEST;
                break;
            case TestSeq.START_TEST:
                if(!_AutoDecayTest)
                {
                    if (_isShow2RequestStartDecay) break;
                    _isShow2RequestStartDecay = true;
                    DialogResult dr = MessageBox.Show("Nhấn nút Start sau đó nhấn OK trên màn hình hiển thị.", "Test Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    if (dr == DialogResult.OK)
                    {
                        _isShow2RequestStartDecay = false;
                        _testSeq = TestSeq.CHECKING_RESULT;
                    }
                    else
                        _isShow2RequestStartDecay = false;
                }
                else
                {
                    if(bBusy1)
                    {
                        MessageBox.Show("Decay Tester is busy. Please call technical to support!!!!", "Tester Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _testSeq = TestSeq.ERROR;
                        return;
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        if (await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayStart1, 1)) break;
                        if (i == 3)
                        {
                            MessageBox.Show("Can not turn on Decay tester. Please check connection.", "Tester Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            _testSeq = TestSeq.ERROR;
                            return;
                        }
                       
                    }
                    _testSeq = TestSeq.CHECKING_RESULT;
                }    
                break;
            case TestSeq.CHECK_START:
                if (_isCheckingStartTester) break;
                _isCheckingStartTester = true;
                for (int i = 0;i <5; i++)
                {
                    if(i==4 && !bBusy1)
                    {
                        MessageBox.Show("Can not turn on Decay tester. Please check connection.", "Tester Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayStart1, 0);
                        _testSeq = TestSeq.ERROR;
                        return;
                    }    
                    if (bBusy1)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayStart1, 0)) break;
                            if (j == 3)
                            {
                                MessageBox.Show("Please check connection.", "Tester Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                _testSeq = TestSeq.ERROR;
                                return;
                            }
                            _testSeq = TestSeq.CHECK_START;
                        }
                    } 
                    Thread.Sleep(1000);
                } 
                _testSeq = TestSeq.CHECKING_RESULT;
                _isCheckingStartTester = false;
                break;
            case TestSeq.CHECKING_RESULT:
                if (_isWaitingResult) break;
                _isWaitingResult = true;
                cls_LSR902._IsgetResultSuccess = false;
                sw_decayTest = new Stopwatch();
                sw_decayTest.Start();
                //while (sw_decayTest.ElapsedMilliseconds < (ClsUnitManagercs.cls_Units.iDecayStabilize_Time + ClsUnitManagercs.cls_Units.iDecayTest_Time + 20) * 1000)
                while (sw_decayTest.ElapsedMilliseconds < 250 * 1000)
                {
                    if(bEMG)
                    {
                        sErrorCode = "Abort";
                        _testSeq = TestSeq.END_TEST;
                        await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayStop1, 1);
                        sw_decayTest.Stop();
                        _isWaitingResult = false;
                        return;
                    }    
                    if(cls_LSR902._IsgetResultSuccess)
                    {
                        if (cls_LSR902.result is LeakTestResultM mResult)
                        {
                            double PressureTime = mResult.Pressure_Time;
                            double HoldingTime = mResult.Balance1_Time;
                            double BalanceTime = mResult.Balance2_Time;
                            double TestTime = mResult.Detection_Time;
                            double TestPressure = mResult.TestPressure;
                            double Leak = mResult.Leak;
                            int iLeakUnitType = mResult.LeakUnit;
                            //if (TestPressure > (ClsUnitManagercs.cls_Units.iDecayPressure + ClsUnitManagercs.cls_Units.iDecayPressureTolerance))
                            if (TestPressure > (58.02 + 5.802))
                            {
                                sErrorCode ="Pressure is too high.";
                                _testSeq = TestSeq.TEST_FAIL;
                                break;
                            }
                            //else if (TestPressure < (ClsUnitManagercs.cls_Units.iDecayPressure - ClsUnitManagercs.cls_Units.iDecayPressureTolerance)
                            else if (TestPressure < (58.02 - 5.802))
                            {
                                sErrorCode = "Pressure is too low.";
                                _testSeq = TestSeq.TEST_FAIL;
                                break;
                            }
                            else if( Leak > 0.35)
                            {
                                sErrorCode = "Leak is too high.";
                                _testSeq = TestSeq.TEST_FAIL;
                                break;
                            }
                            else if (Leak < 0)
                            {
                                sErrorCode = "Leak is too low.";
                                _testSeq = TestSeq.TEST_FAIL;
                                break;
                            }
                            else if (PressureTime < 30)
                            {
                                sErrorCode = "Pressure time is too short.";
                                _testSeq = TestSeq.TEST_FAIL;
                                break;
                            }
                            else if (HoldingTime < 30)
                            {
                                sErrorCode = "Holding time is too short.";
                                _testSeq = TestSeq.TEST_FAIL;
                                break;
                            }
                            else if (BalanceTime < 60)
                            {
                                sErrorCode = "Balance time is too short.";
                                _testSeq = TestSeq.TEST_FAIL;
                                break;
                            }
                            else if (TestTime < 30)
                            {
                                sErrorCode = "Detect time is too short.";
                                _testSeq = TestSeq.TEST_FAIL;
                                break;
                            }
                            else if(iLeakUnitType !=12)
                            {
                                sErrorCode = "Leak type is not E-3 Pa·m³/s. Please call technical!";
                                _testSeq = TestSeq.TEST_FAIL;
                                break;
                            }    
                            else
                            {
                                _testSeq = TestSeq.TEST_PASS;
                                cls_LSR902.result = mResult;
                                _isWaitingResult = false;
                                break;
                            }
                        }
                    }    
                    Thread.Sleep(100);
                }
                sw_decayTest.Stop();
                if (!cls_LSR902._IsgetResultSuccess)
                {
                    sErrorCode ="Get result from LSR902 timeout. Please check connection.";
                    _testSeq = TestSeq.ERROR;
                }
                _isWaitingResult = false;
                break;
            case TestSeq.TEST_PASS:
                ClsUnitManagercs.cls_Units.cls_SequencyCommon.process = StateCommon.ProcessState.CompletedPass;
                TestResult = "PASS";
                _testSeq = TestSeq.CREATE_LOCAL_LOG;
                break;
            case TestSeq.TEST_FAIL:
                ClsUnitManagercs.cls_Units.cls_SequencyCommon.process = StateCommon.ProcessState.CompletedFail;
                TestResult = "FAIL";
                _testSeq = TestSeq.CREATE_LOCAL_LOG;
                break;
            case TestSeq.FIXTURE_OPEN:
                if(!_AutoFixture)
                {
                    _testSeq = TestSeq.CREATE_LOCAL_LOG;
                    break;
                }
                if (_isRequestCloseFixture) break;
                _isRequestCloseFixture = true;
                if (!await OpenFixture_1())
                {
                    MessageBox.Show("Fixture 1 can not open.", "Fixture Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _testSeq = TestSeq.ERROR;
                }
                if (!await OpenFixture_2())
                {
                    MessageBox.Show("Fixture 2 can not open.", "Fixture Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _testSeq = TestSeq.ERROR;
                }
                Thread.Sleep(2000);
                _testSeq = TestSeq.CREATE_LOCAL_LOG;
                _isRequestCloseFixture = false;
                break;
            case TestSeq.CREATE_LOCAL_LOG:
                DateTime dt = DateTime.Now;
                var logger = new Logger($"C:\\Aavid_Test\\Datalog\\Pressure_Decay\\{dt.ToString("yyyyMMdd")}.csv");
                if (cls_LSR902.result is LeakTestResultM mresult)
                {
                    for (int i = 0; i < _ScannerNo; i++)
                    {
                        var logData = new PressureDecayLog
                        {
                            Time = dt,
                            SerialNumber = _SNs[i],
                            TestResult = TestResult, // PASS / FAIL
                            PressureUSL = mresult.TestPressureUL,
                            PressureLSL = mresult.TestPressureLL,
                            PressureValue = mresult.TestPressure,
                            PressureType = mresult.PresureTestType,
                            LeakageUSL = mresult.DET_LL, // Upper Specification Limit for Leakage
                            LeakageLSL = mresult.DET_UL, // Lower Specification Limit for Leakage
                            Leakagevalue = mresult.Leak,
                            LeakageType = mresult.LeakTestType,
                            PressureTime = mresult.Pressure_Time, // Time taken for the test in seconds
                            Balance1Time = mresult.Balance1_Time, // Balance time for the first measurement
                            Balance2Time = mresult.Balance2_Time, // Balance time for the second measurement
                            DetectTime = mresult.Detection_Time, // Detection time for the test
                            KVe = mresult.KUnit // K value for the test, if applicable
                        };
                        logger.PressureDecay(logData);
                    }
                }
                else
                {
                    for (int i = 0; i < _ScannerNo; i++)
                    {
                        var logData = new PressureDecayLog
                        {
                            Time = dt,
                            SerialNumber = _SNs[i],
                            TestResult = TestResult, // PASS / FAIL
                            PressureUSL = 0,
                            PressureLSL = 0,
                            PressureValue = 0,
                            PressureType = "Null",
                            LeakageUSL = 0, // Upper Specification Limit for Leakage
                            LeakageLSL =0, // Lower Specification Limit for Leakage
                            Leakagevalue = 0,
                            LeakageType ="Null",
                            PressureTime = 0, // Time taken for the test in seconds
                            Balance1Time = 0, // Balance time for the first measurement
                            Balance2Time =0, // Balance time for the second measurement
                            DetectTime = 0, // Detection time for the test
                            KVe = 0 // K value for the test, if applicable
                        };
                        logger.PressureDecay(logData);
                    }
                }   
                _testSeq = TestSeq.SAVE_DB;
                break;
            case TestSeq.SAVE_DB:
                string PN = ClsUnitManagercs.cls_Units.AssyPN;
                string WO = ClsUnitManagercs.cls_Units.WO;
                string Station = ClsUnitManagercs.cls_Units.StaionID;
                if (TestType == "Engineering Test") TestResult = "FAIL";
                if(cls_LSR902.result is LeakTestResultM m)
                {
                    if (_ScannerNo == 1)
                    {
                        for (int i = 0; i < 1; i++)
                        {
                            if (!SavetoDBMonths_Log(_SNs[i], PN, Station, TestResult, "Pressure_Decay", ""))
                            {
                                MessageBox.Show("Failed to save DB months log failed.");
                                ClsUnitManagercs.cls_Units.cls_SequencyCommon.process = StateCommon.ProcessState.Error;
                                _testSeq = TestSeq.ERROR;
                                break;
                            }
                            if (!SaveToDBPressureDecayLog(_SNs[i], PN, WO, Station, TestResult, m.TestPressure.ToString("0.000"), m.Leak.ToString("0.000")))
                            {
                                MessageBox.Show("Failed to save DB Decay test log failed.");
                                ClsUnitManagercs.cls_Units.cls_SequencyCommon.process = StateCommon.ProcessState.Error;
                                _testSeq = TestSeq.ERROR;
                                break;
                            }
                            _testSeq = TestSeq.END_TEST;
                        }
                    }
                    else if (_ScannerNo == 2)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if (!SavetoDBMonths_Log(_SNs[i], PN, Station, TestResult, "Pressure_Decay", ""))
                            {
                                MessageBox.Show("Failed to save DB months log failed.");
                                ClsUnitManagercs.cls_Units.cls_SequencyCommon.process = StateCommon.ProcessState.Error;
                                _testSeq = TestSeq.ERROR;
                                break;
                            }
                            if (!SaveToDBPressureDecayLog(_SNs[i], PN, WO, Station, TestResult, m.TestPressure.ToString("0.000"), m.Leak.ToString("0.000")))
                            {
                                MessageBox.Show("Failed to save DB DI_Water log failed.");
                                ClsUnitManagercs.cls_Units.cls_SequencyCommon.process = StateCommon.ProcessState.Error;
                                _testSeq = TestSeq.ERROR;
                                break;
                            }
                            _testSeq = TestSeq.END_TEST;
                        }
                    }
                    else if (_ScannerNo == 4)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (!SavetoDBMonths_Log(_SNs[i], PN, Station, TestResult, "Pressure_Decay", ""))
                            {
                                MessageBox.Show("Failed to save DB months log failed.");
                                ClsUnitManagercs.cls_Units.cls_SequencyCommon.process = StateCommon.ProcessState.Error;
                                _testSeq = TestSeq.ERROR;
                                break;
                            }
                            if (!SaveToDBPressureDecayLog(_SNs[i], PN, WO, Station, TestResult, m.TestPressure.ToString("0.000"), m.Leak.ToString("0.000")))
                            {
                                MessageBox.Show("Failed to save DB DI_Water log failed.");
                                ClsUnitManagercs.cls_Units.cls_SequencyCommon.process = StateCommon.ProcessState.Error;
                                _testSeq = TestSeq.ERROR;
                                break;
                            }
                            _testSeq = TestSeq.END_TEST;
                        }
                    }
                }
                else
                {
                    _testSeq = TestSeq.ERROR;
                    break;
                }    
                break;
            case TestSeq.END_TEST:
                _SN1 = "";
                _SN2 = "";
                _SN3 = "";
                _SN4 = "";
                await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayStop1, 0);
                await cls_ASPcontrol.SetRelayONOFFAsyncCheckResult(iRelayStart1, 0);
                cls_LSR902._IsgetResultSuccess = false;
                cls_LSR902.result = null;
                _testSeq = TestSeq.WAIT;
                break;
        }
    }
    public void AddProcessLogTest(int UCMainIndex ,DataTable dtResult)
    {
        OnRequestAddProcessLogTest?.Invoke(this, new ProcessLogEventArgs
        {
            UnitIndex = UCMainIndex,
            LogData = dtResult
        });
    }
}
