using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            return CheckHealthAsync().Result;
        }

        public async Task<HealthData> CheckHealthAsync()
        {
            HealthData healthData = new HealthData(_appName);

            try
            {
                List<Task<HealthData>> checkTasks = new List<Task<HealthData>>();
                foreach (IHealthDependency dependency in _healthDependencyCollection)
                {
                    checkTasks.Add(CheckDependency(dependency));
                }

                HealthData[] results = await Task.WhenAll(checkTasks.ToList());

                healthData.DependenciesStatus.AddRange(results);
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

        private async Task<HealthData> CheckDependency(IHealthDependency dependency)
        {
            try
            {
                dependency.OnDependencyException += (o, e) =>
                {
                    OnDependencyException?.Invoke(this, e);
                };

                return await dependency.CheckHealthAsync();
            }
            catch (Exception ex)
            {
                OnDependencyException?.Invoke(this, new DependencyExceptionEvent(ex));
                throw;
            }
        }
    }
}
