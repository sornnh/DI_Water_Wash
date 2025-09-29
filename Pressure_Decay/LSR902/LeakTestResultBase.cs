using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class LeakTestResultBase
{
    public string Result { get; set; }
    public int Channel { get; set; }
    public DateTime Timestamp { get; set; }
}
