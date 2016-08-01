using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrHouse.SqlServer
{
    public enum Permission
    {
        Undefined = 0,
        SELECT = 1,
        INSERT = 2,
        DELETE = 4,
        UPDATE = 8,
    }
}
