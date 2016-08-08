using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrHouse.Core
{
    public class HealthChecker
    {
        private readonly string _appName;
        private ICollection<IHealthDependency> _healthDependencyCollection;

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
                    healthDataCollection.Add(dep.CheckHealth());
                });

                healthData.DependenciesStatus.AddRange(healthDataCollection);
                healthData.IsOK = true;
            }
            catch (Exception ex)
            {
                healthData.IsOK = false;
                healthData.ErrorMessage = "HealthChecker crashed.";
            }

            return healthData;
        }
    }
}
