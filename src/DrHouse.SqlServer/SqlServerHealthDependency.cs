using DrHouse.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrHouse.SqlServer
{
    public class SqlServerHealthDependency : IHealthDependency
    {
        private readonly SqlConnection _sqlConnection;
        private readonly IDictionary<string, ICollection<TablePermission>> _permissions;
        private readonly string _dbName;

        public SqlServerHealthDependency(string nameOrConnectionString)
        {
            string connectionString = (ConfigurationManager.ConnectionStrings[nameOrConnectionString] != null) ?
                                ConfigurationManager.ConnectionStrings[nameOrConnectionString].ConnectionString :
                                nameOrConnectionString;

            _sqlConnection = new SqlConnection(connectionString);

            if (_sqlConnection.State != System.Data.ConnectionState.Open)
            {
                _sqlConnection.Open();
            }

            _dbName = _sqlConnection.Database;

            _permissions = new Dictionary<string, ICollection<TablePermission>>();
        }

        public void AddTableDependency(string tableName, Permission permissionSet)
        {
            if(_permissions.ContainsKey(tableName) == false)
            {
                _permissions.Add(tableName, new List<TablePermission>());
            }

            ICollection<TablePermission> tablePermisionCollection = _permissions[tableName];

            foreach(Permission permission in Enum.GetValues(typeof(Permission)))
            {
                if((permission & permissionSet) != 0)
                {
                    tablePermisionCollection.Add(new TablePermission()
                    {
                        TableName = tableName,
                        Permission = permission,
                    });
                }
            }
        }

        public HealthData CheckHealth()
        {
            HealthData sqlHealthData = new HealthData(_dbName);
            sqlHealthData.Type = "SqlServer";

            try
            {
                foreach (string tableName in _permissions.Keys)
                {
                    HealthData tableHealth = CheckTablePermissions(tableName, _permissions[tableName]);
                    sqlHealthData.DependenciesStatus.Add(tableHealth);
                }

                sqlHealthData.IsOK = true;
            }
            catch(Exception ex)
            {
                sqlHealthData.ErrorMessage = ex.Message;
                sqlHealthData.IsOK = false;
            }

            return sqlHealthData;
        }

        private HealthData CheckTablePermissions(string tableName, ICollection<TablePermission> permissions)
        {
            HealthData tableHealth = new HealthData(tableName);

            try
            {
                foreach (TablePermission permission in permissions)
                {
                    HealthData tablePermissionHealth = new HealthData(permission.Permission.ToString());

                    tablePermissionHealth.IsOK = CheckPermission(permission);
                    if(tablePermissionHealth.IsOK == false)
                    {
                        tablePermissionHealth.ErrorMessage = "Does not have permission.";
                    }

                    tableHealth.DependenciesStatus.Add(tablePermissionHealth);
                }

                tableHealth.IsOK = true;
            }
            catch (Exception ex)
            {
                tableHealth.ErrorMessage = ex.Message;
                tableHealth.IsOK = false;
            }

            return tableHealth;
        }

        private bool CheckPermission(TablePermission permission)
        {
            string query = @"SELECT HAS_PERMS_BY_NAME (@tableName, 'OBJECT', @permission)";
            var permissionCmd = new SqlCommand(query);
            permissionCmd.Parameters.Add(new SqlParameter() { ParameterName = "@tableName", Value = permission.TableName });
            permissionCmd.Parameters.Add(new SqlParameter() { ParameterName = "@permission", Value = permission.Permission.ToString() });

            permissionCmd.Connection = _sqlConnection;

            var reader = permissionCmd.ExecuteReader();
            reader.Read();

            bool result = (int)reader[0] == 1;
            reader.Close();

            return result;
        }

        public HealthData CheckHealth(Action check)
        {
            throw new NotImplementedException();
        }
    }
}
