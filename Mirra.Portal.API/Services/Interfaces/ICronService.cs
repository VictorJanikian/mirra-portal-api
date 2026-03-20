using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Services.Interfaces
{
    public interface ICronService
    {
        public int CalculateMaxRunsPerWeek(string cronExpression);
        public void ValidateIntervalsFormat(CustomerPlatformConfiguration configuration);
        public Task<int> CalculateTotalRunsPerWeekForConfiguration(CustomerPlatformConfiguration configuration);
        public void ValidateIntervalFormat(Scheduling scheduling);
    }
}
