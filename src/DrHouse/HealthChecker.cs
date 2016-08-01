using System;
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
                _healthDependencyCollection.AsParallel().ForAll(dep =>
                {
                    healthData.DependenciesStatus.Add(dep.CheckHealth());
                });
                healthData.IsOK = true;
            }
            catch (Exception ex)
            {
                healthData.IsOK = false;
            }
            return healthData;
        }
    }
}
