using System;

namespace DrHouse.Core
{
    public interface IHealthDependency
    {
        HealthData CheckHealth();

        HealthData CheckHealth(Action check);

        event EventHandler OnDependencyException;
    }
}
