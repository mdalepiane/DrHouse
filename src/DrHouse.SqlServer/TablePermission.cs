using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrHouse.SqlServer
{
    internal class TablePermission
    {
        public string TableName { get; set; }

        public Permission Permission { get; set; }

        public bool HasPermission { get; set; }
    }
}
