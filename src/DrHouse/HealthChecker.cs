using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DrHouse.Events;

namespace DrHouse.Core
{
    public class HealthChecker
    {
        private readonly string _appName;
        private readonly ICollection<IHealthDependency> _healthDependencyCollection;

        public event EventHandler<DependencyExceptionEvent> OnDependencyException;

        public HealthChecker(string appName)
        {
            _appName = appName;
            _healthDependencyCollection = new List<IHealthDependency>();
        }

        public void AddDependency(IHealthDependency dependency)
        {
            _healthDependencyCollection.Add(dependency);
        }

        public HealthData CheckHealth()
        {
            HealthData healthData = new HealthData(_appName);

            try
            {
                ConcurrentBag<HealthData> healthDataCollection = new ConcurrentBag<HealthData>();

                _healthDependencyCollection.AsParallel().ForAll(dep =>
                {
                    healthDataCollection.Add(CheckDependency(dep));
                });

                healthData.DependenciesStatus.AddRange(healthDataCollection);
                healthData.IsOK = true;
            }
            catch (Exception ex)
            {
                OnDependencyException?.Invoke(this, new DependencyExceptionEvent(ex));

                healthData.IsOK = false;
                healthData.ErrorMessage = "HealthChecker crashed.";
            }

            return healthData;
        }

        private HealthData CheckDependency(IHealthDependency dependency)
        {
            try
            {
                dependency.OnDependencyException += (o, e) =>
                {
                    OnDependencyException?.Invoke(this, e);
                };

                return dependency.CheckHealth();
            }
            catch (Exception ex)
            {
                OnDependencyException?.Invoke(this, new DependencyExceptionEvent(ex));
                throw;
            }
        }
    }
}
