using Mirra_Portal_API.Enums;
using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Repositories.Interfaces
{
    public interface ISchedulingRepository
    {
        Task<List<Scheduling>> GetAllByConfigurationId(int configurationId);
        Task<Scheduling> GetById(int schedulingId);
        Task<Scheduling> Create(Scheduling scheduling);
        Task<Scheduling> Update(Scheduling scheduling);
        Task Delete(int schedulingId);
        Task<bool> HasAnyByCustomerIdAndStatus(int customerId, ESchedulingStatus status);
    }
}
