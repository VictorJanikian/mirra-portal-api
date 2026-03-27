using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Services.Interfaces
{
    public interface IConfigurationService
    {
        public Task<CustomerPlatformConfiguration> CreateConfiguration(CustomerPlatformConfiguration configuration);
        public Task<CustomerPlatformConfiguration> GetConfiguration(int configurationId);
        public Task<List<CustomerPlatformConfiguration>> GetAllConfigurations();
        public Task<bool> HasSuspendedSchedulingsDueToLackOfPayment();
        public Task<bool> HasSuspendedSchedulingsDueToPlanDowngrade();
        public Task<bool> HasSuspendedSchedulingsDueToLackOfPayment(int configurationId);
        public Task<bool> HasSuspendedSchedulingsDueToPlanDowngrade(int configurationId);


    }
}
