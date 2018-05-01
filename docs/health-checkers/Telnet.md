# Telnet Checker

Telnet checker can be used to verify if the application can connect to a
specified network address and port. Useful when the application must access
other services in the network but the in environment has strict firewall policies.

**Disclaimer:** Despite it's name, this checker no longer uses telnet to verify connectivity.

## Usage
```
HealthChecker healthChecker = new HealthChecker("TestApp");

TelnetHealthDependency telnetDep = new TelnetHealthDependency("localhost", 8080, "ExternalWebService");
healthChecker.AddDependency(telnetDep);

HealthData health = healthChecker.CheckHealth();
```
