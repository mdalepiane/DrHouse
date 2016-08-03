using DrHouse.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace DrHouse.SqlServer
{
    public class SqlServerHealthDependency : IHealthDependency
    {
        private readonly string _connectionStringName;
        private readonly string _connectionString;
        private readonly IDictionary<string, ICollection<TablePermission>> _permissions;

        public SqlServerHealthDependency(string connectionStringName)
        {
            _connectionStringName = connectionStringName;
            _connectionString = (ConfigurationManager.ConnectionStrings[connectionStringName] != null) ?
                                ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString :
                                "";

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
            HealthData sqlHealthData = new HealthData(_connectionStringName);
            sqlHealthData.Type = "SqlServer";

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(_connectionString))
                {

                    if (sqlConnection.State != System.Data.ConnectionState.Open)
                    {
                        sqlConnection.Open();
                    }

                    sqlHealthData = new HealthData(sqlConnection.Database);
                    sqlHealthData.Type = "SqlServer";

                    foreach (string tableName in _permissions.Keys)
                    {
                        HealthData tableHealth = CheckTablePermissions(tableName, _permissions[tableName], sqlConnection);
                        sqlHealthData.DependenciesStatus.Add(tableHealth);
                    }

                    sqlHealthData.IsOK = true;
                }
            }
            catch (Exception ex)
            {
                sqlHealthData.IsOK = false;
                sqlHealthData.ErrorMessage = ex.Message;
            }

            return sqlHealthData;
        }

        private HealthData CheckTablePermissions(string tableName, ICollection<TablePermission> permissions, SqlConnection sqlConnection)
        {
            HealthData tableHealth = new HealthData(tableName);

            try
            {
                foreach (TablePermission permission in permissions)
                {
                    HealthData tablePermissionHealth = new HealthData(permission.Permission.ToString());

                    tablePermissionHealth.IsOK = CheckPermission(permission, sqlConnection);
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

        private bool CheckPermission(TablePermission permission, SqlConnection sqlConnection)
        {
            string query = @"SELECT HAS_PERMS_BY_NAME (@tableName, 'OBJECT', @permission)";
            var permissionCmd = new SqlCommand(query);
            permissionCmd.Parameters.Add(new SqlParameter() { ParameterName = "@tableName", Value = permission.TableName });
            permissionCmd.Parameters.Add(new SqlParameter() { ParameterName = "@permission", Value = permission.Permission.ToString() });

            permissionCmd.Connection = sqlConnection;

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
