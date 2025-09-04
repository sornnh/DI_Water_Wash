using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DI_Water_Wash
{

    public static class ClsUnitManagercs
    {
        public static Cls_Unit[] cls_Units;

        public static void Initialize(string[] assyPN,string [] WorkOder,Form1 mainForm,string StationID, Cls_ASPcontrol cls_AS, ClsInverterModbus clsInverter)
        {
            cls_Units = new Cls_Unit[assyPN.Length];
            for (int i = 0; i < assyPN.Length; i++)
            {
                cls_Units[i] = new Cls_Unit(i, mainForm,  assyPN[i], WorkOder[i],StationID, cls_AS, clsInverter);
            }
        }
    }
}
