using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Services.Interfaces
{
    public interface IConfigurationService
    {
        public Task<CustomerContentPlatformConfiguration> CreateConfiguration(CustomerContentPlatformConfiguration configuration);
        public Task<List<Scheduling>> GetConfigurationSchedulings(int configurationId);
        public Task<Scheduling> GetScheduling(int configurationId, int schedulingId);
        public Task<Scheduling> UpdateScheduling(int configurationId, int schedulingId, Scheduling scheduling);


    }
}
