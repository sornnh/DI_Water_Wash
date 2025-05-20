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
        public static void Initialize(string[] assyPN)
        {
            cls_Units = new Cls_Unit[assyPN.Length];
            for (int i = 0; i < assyPN.Length; i++)
            {
                cls_Units[i] = new Cls_Unit(i, assyPN[i]);
            }
        }
    }
}
