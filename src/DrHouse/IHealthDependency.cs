using System;
using DrHouse.Events;

namespace DrHouse.Core
{
    public interface IHealthDependency
    {
        HealthData CheckHealth();

        HealthData CheckHealth(Action check);

        event EventHandler<DependencyExceptionEvent> OnDependencyException;
    }
}
