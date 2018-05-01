using DrHouse.Core;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using DrHouse.Events;

namespace DrHouse.SqlServer
{
    public class SqlServerHealthDependency : IHealthDependency
    {
        private readonly string _databaseName;
        private readonly string _connectionString;
        private readonly IDictionary<string, ICollection<TablePermission>> _permissions;
        private readonly ICollection<Index> _indexes;

        public event EventHandler OnDependencyException;

        public SqlServerHealthDependency(string databaseName, string connectionString)
        {
            _databaseName = databaseName;
            _connectionString = connectionString;
            _permissions = new Dictionary<string, ICollection<TablePermission>>();
            _indexes = new List<Index>();
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

        public void AddIndexDependency(string tableName, string indexName)
        {
            _indexes.Add(new Index { TableName = tableName, IndexName = indexName });
        }

        public HealthData CheckHealth()
        {
            HealthData sqlHealthData = new HealthData(_databaseName);
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

                    foreach (Index ix in _indexes)
                    {
                        HealthData indexHealth = CheckIndex(ix, sqlConnection);
                        sqlHealthData.DependenciesStatus.Add(indexHealth);
                    }

                    sqlHealthData.IsOK = true;
                }
            }
            catch (Exception ex)
            {
                OnDependencyException?.Invoke(this, new DependencyExceptionEvent(ex));

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
                OnDependencyException?.Invoke(this, new DependencyExceptionEvent(ex));

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

        private HealthData CheckIndex(Index index, SqlConnection sqlConnection)
        {
            HealthData tableHealth = new HealthData(index.IndexName);

            string query = @"SELECT COUNT(1) FROM sys.indexes WHERE name = @indexName AND object_id = OBJECT_ID(@tableName)";

            try
            {
                var permissionCmd = new SqlCommand(query);
                permissionCmd.Parameters.Add(new SqlParameter() { ParameterName = "@indexName", Value = index.IndexName });
                permissionCmd.Parameters.Add(new SqlParameter() { ParameterName = "@tableName", Value = index.TableName });

                permissionCmd.Connection = sqlConnection;

                bool result = false;
                using (var reader = permissionCmd.ExecuteReader())
                {
                    reader.Read();

                    // If there is at lease one, return success
                    result = (int)reader[0] > 0;
                }

                if (tableHealth.IsOK == false)
                {
                    tableHealth.ErrorMessage = $"Index '{index.IndexName}' not found for table '{index.TableName}'.";
                }

                tableHealth.IsOK = result;
            }
            catch (Exception ex)
            {
                OnDependencyException?.Invoke(this, new DependencyExceptionEvent(ex));

                tableHealth.ErrorMessage = ex.Message;
                tableHealth.IsOK = false;
            }

            return tableHealth;
        }

        public HealthData CheckHealth(Action check)
        {
            throw new NotImplementedException();
        }
    }
}
