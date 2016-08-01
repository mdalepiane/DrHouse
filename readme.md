# Dr House

The objective of Dr House is to provide a  set of application Health Checkers.

It can be used within a health check endpoint to perform all verifications needed
in order to determine whether the application is healthy or not.

## Health Checkers

Current implementation includes the following Health Checkers:
- [SqlServer](docs/health-checkers/SqlServer.md) - checks the connection with
  SQL Server database and the specified permissions
