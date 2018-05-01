# Dr House

The objective of Dr House is to provide a  set of application Health Checkers.

It can be used within a health check endpoint to perform all verifications needed
in order to determine whether the application is healthy or not.

## Health Checkers

Current implementation includes the following Health Checkers:
- [Directory](docs/health-checkers/Directory.md) - checks the exitence of a directory
  and the specified user permissions to it (i.e. R/W)
- [SqlServer](docs/health-checkers/SqlServer.md) - checks the connection with
  SQL Server database and the specified permissions
- [Telnet](docs/health-checkers/Telnet.md) - checks the connectivity with a server

## Throubleshooting dependencies

The library is designed to handle all exceptions internally and always return
a HealthData object indicating either success or failure of the dependencies.
This might make it difficult to figure out what is the dependency problem.

To help throubleshooting dependency errors it is recommended to include an
event handler for the event [OnDependencyException](docs/events/OnDependencyException.md).
This will be invoked whenever an exception is thrown while checking dependencies and
contains the exception, so it may be logged and help during throubleshooting.