using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Repositories.Interfaces
{
    public interface ISchedulingRepository
    {
        Task<List<Scheduling>> GetAllByConfigurationId(int configurationId);
        Task<Scheduling> GetById(int schedulingId);
    }
}
