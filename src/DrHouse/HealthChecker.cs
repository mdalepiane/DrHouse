using Common.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DrHouse.Core
{
    public class HealthChecker
    {
        private readonly ILog _logger = LogManager.GetLogger<HealthChecker>();

        private readonly string _appName;
        private ICollection<IHealthDependency> _healthDependencyCollection;

        public HealthChecker(string appName)
        {
            _appName = appName;
            _healthDependencyCollection = new List<IHealthDependency>();
        }

        public void AddDependency(IHealthDependency dependency)
        {
            _logger.Debug($"Adding health dependency to {_appName}.");
            _healthDependencyCollection.Add(dependency);
            _logger.Debug($"Health dependency added to {_appName}.");
        }

        public HealthData CheckHealth()
        {
            _logger.Debug($"Check health for {_appName} started.");

            HealthData healthData = new HealthData(_appName);

            try
            {
                ConcurrentBag<HealthData> healthDataCollection = new ConcurrentBag<HealthData>();

                _healthDependencyCollection.AsParallel().ForAll(dep =>
                {
                    healthDataCollection.Add(dep.CheckHealth());
                });

                _logger.Debug($"Check health completed for {_appName}.");
                healthData.DependenciesStatus.AddRange(healthDataCollection);
                healthData.IsOK = true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Check health crashed for {_appName}: {ex.Message}", ex);

                healthData.IsOK = false;
                healthData.ErrorMessage = "HealthChecker crashed.";
            }

            _logger.Info($"Health check for {_appName} finished. Success: {healthData.IsOK}.");

            return healthData;
        }
    }
}
