using Xunit;
using Xunit.Abstractions;

namespace DrHouse.Core.UnitTest
{
    public class HealthDataTest
    {
        private readonly ITestOutputHelper _output;

        public HealthDataTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void HealthData_with_dependencies_ok_is_ok()
        {
            HealthData healthData = new HealthData("test");
            healthData.IsOK = true;

            healthData.DependenciesStatus.Add(new HealthData("dep1") { IsOK = true });
            healthData.DependenciesStatus.Add(new HealthData("dep2") { IsOK = true });

            Assert.True(healthData.IsOK);
        }

        [Fact]
        public void HealthData_without_dependencies_is_ok()
        {
            HealthData healthData = new HealthData("test");
            healthData.IsOK = true;

            Assert.True(healthData.IsOK);
        }

        [Fact]
        public void HealthData_without_dependencies_is_not_ok()
        {
            HealthData healthData = new HealthData("test");
            healthData.IsOK = false;

            Assert.False(healthData.IsOK);
        }

        [Fact]
        public void HealthData_with_dependencies_not_ok_is_not_ok()
        {
            HealthData healthData = new HealthData("test");
            healthData.IsOK = true;

            healthData.DependenciesStatus.Add(new HealthData("dep1") { IsOK = false });
            healthData.DependenciesStatus.Add(new HealthData("dep2") { IsOK = true });

            Assert.False(healthData.IsOK);
        }

        [Fact]
        public void HealthData_with_dependencies_ok_is_not_ok()
        {
            HealthData healthData = new HealthData("test");
            healthData.IsOK = false;

            healthData.DependenciesStatus.Add(new HealthData("dep1") { IsOK = true });
            healthData.DependenciesStatus.Add(new HealthData("dep2") { IsOK = true });

            Assert.False(healthData.IsOK);
        }

        [Fact]
        public void HealthData_with_sub_dependencies_not_ok_is_not_ok()
        {
            HealthData healthData = new HealthData("test");
            healthData.IsOK = true;

            healthData.DependenciesStatus.Add(new HealthData("dep1") { IsOK = true });
            HealthData dataWithSubDependency = new HealthData("dep2") { IsOK = true };
            dataWithSubDependency.DependenciesStatus.Add(new HealthData("subdep1") { IsOK = false });
            healthData.DependenciesStatus.Add(dataWithSubDependency);

            Assert.False(healthData.IsOK);
        }
    }
}
