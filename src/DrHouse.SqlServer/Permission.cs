using System;

namespace DrHouse.SqlServer
{
    [FlagsAttribute]
    public enum Permission
    {
        Undefined = 0,
        SELECT = 1,
        INSERT = 2,
        DELETE = 4,
        UPDATE = 8,
        ALTER = 16,
    }
}
