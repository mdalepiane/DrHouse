# OnDependencyException

```OnDependencyException``` is an event from ```HealthChecker```
that is invoked whenever an exception is thrown while a dependency
check is being executed. It is recommended to add a handler to it
to log the exceptions to help throubleshooting dependency failures.

The following code is an example of how to use ```OnDependencyException```
to capture exceptions during checker execution and log it to the console.

```
HealthChecker checker = new HealthChecker("TestApp");

checker.OnDependencyException += (sender, eventArgs) =>
{
    Console.WriteLine(eventArgs.Exception.Message);
};
```