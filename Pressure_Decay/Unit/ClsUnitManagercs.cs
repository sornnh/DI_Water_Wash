using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public static class ClsUnitManagercs
{
    public static Cls_Unit cls_Units;
    
    public static void Initialize(string assyPN,string WorkOder,string StationID, Cls_ASPcontrol cls_AS, Cls_LS_R902 clsLSR902)
    {
        cls_Units = new Cls_Unit(assyPN, WorkOder,StationID, cls_AS, clsLSR902);
        
    }
    
}
