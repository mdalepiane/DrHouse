using System;
using System.Threading.Tasks;
using DrHouse.Events;

namespace DrHouse.Core
{
    public interface IHealthDependency
    {
        Task<HealthData> CheckHealthAsync();

        HealthData CheckHealth(Action check);

        event EventHandler<DependencyExceptionEvent> OnDependencyException;
    }
}
