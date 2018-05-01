# Directory Checker

Directory checker can be used to verify if a Windows user has permissions
on a given directory.

A user must be provided when the checker is created and a set of directories
and expected permissions might be added to it to be checked.

## Usage
```
// Create a new Health Checker for application TestApp
HealthChecker healthChecker = new HealthChecker("TestApp");

// Retrieve the WindowsIdentity of the user that is running the application
WindowsIdentity currentUser = WindowsIdentity.GetCurrent();

// Create a new Directory Checker for this user
DirectoryHealthDependency directoryDep = new DirectoryHealthDependency(currentUser);

// Add some directory accesss dependencies for this user
directoryDep.AddDirectoryDependency(@"C:\sample_input_dir", new [] { FileSystemRights.Read });
directoryDep.AddDirectoryDependency(@"C:\sample_output_dir", new [] { FileSystemRights.Read, FileSystemRights.Write });
			
healthChecker.AddDependency(directoryDep);

HealthData health = healthChecker.CheckHealth();
```
