using DrHouse.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace DrHouseDirectory
{
    /// <summary>
    /// Directory Health Dependency uses the current windows user to check the user's permissions on a directory
    /// </summary>
    public class DirectoryHealthDependency : IHealthDependency
    {
        private WindowsPrincipal _windowsPrincipal;
        private WindowsIdentity _currentUser;
        private readonly IDictionary<string, ICollection<FileSystemRights>> _fileSystemRigths;

        public DirectoryHealthDependency(WindowsIdentity currentUser)
        {
            _currentUser = currentUser;
            _windowsPrincipal = new WindowsPrincipal(currentUser);
            _fileSystemRigths = new Dictionary<string, ICollection<FileSystemRights>>();
        }


        public void AddDirectoryDependency(string directoryPath, FileSystemRights[] filseSystemRigthsArray)
        {
            if (_fileSystemRigths.ContainsKey(directoryPath) == false)
            {
                _fileSystemRigths.Add(directoryPath, new List<FileSystemRights>());
            }

            ICollection<FileSystemRights> fileSystemRigthsCollecion = _fileSystemRigths[directoryPath];

            foreach (FileSystemRights filseSystemRigth in filseSystemRigthsArray)
            {
                fileSystemRigthsCollecion.Add(filseSystemRigth);
            }
        }


        public HealthData CheckHealth()
        {
            HealthData directoryHealthData = new HealthData(_windowsPrincipal.Identity.Name);
            directoryHealthData.Type = "DirectoryPermission";
            try
            {
                foreach (string directoryPath in _fileSystemRigths.Keys)
                {
                    HealthData directoryHealth = CheckFileRigthPermissions(directoryPath, _fileSystemRigths[directoryPath]);
                    directoryHealthData.DependenciesStatus.Add(directoryHealth);
                }

                directoryHealthData.IsOK = true;
            }
            catch (Exception ex)
            {
                directoryHealthData.IsOK = false;
                directoryHealthData.ErrorMessage = ex.Message;
            }
            return directoryHealthData;
        }



        private HealthData CheckFileRigthPermissions(string directoryPath, ICollection<FileSystemRights> fileSystemRights)
        {
            HealthData tableHealth = new HealthData(directoryPath);

            try
            {
                foreach (FileSystemRights fileSystemRight in fileSystemRights)
                {
                    HealthData tablePermissionHealth = new HealthData(fileSystemRight.ToString());

                    tablePermissionHealth.IsOK = CheckUserPermission(directoryPath, fileSystemRight);
                    if (tablePermissionHealth.IsOK == false)
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


        private bool CheckUserPermission(string directoryPath, FileSystemRights accessRights)
        {
            var isInRoleWithAccess = false;

            try
            {
                var di = new DirectoryInfo(directoryPath);
                var acl = di.GetAccessControl();
                var rules = acl.GetAccessRules(true, true, typeof(NTAccount));

                var principal = _windowsPrincipal;
                foreach (AuthorizationRule rule in rules)
                {
                    var fsAccessRule = rule as FileSystemAccessRule;
                    if (fsAccessRule == null)
                        continue;

                    if ((fsAccessRule.FileSystemRights & accessRights) > 0)
                    {
                        var ntAccount = rule.IdentityReference as NTAccount;
                        if (ntAccount == null)
                            continue;

                        if (principal.IsInRole(ntAccount.Value))
                        {
                            if (fsAccessRule.AccessControlType == AccessControlType.Deny)
                                return false;
                            isInRoleWithAccess = true;
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            return isInRoleWithAccess;
        }

        public HealthData CheckHealth(Action check)
        {
            throw new NotImplementedException();
        }
    }
}
