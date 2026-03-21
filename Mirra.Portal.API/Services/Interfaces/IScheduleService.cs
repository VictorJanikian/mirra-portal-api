using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Services.Interfaces
{
    public interface IScheduleService
    {
        public Task<Scheduling> CreateSchedule(int configurationId, Scheduling scheduling);
        public Task<Scheduling> GetSchedule(int configurationId, int schedulingId);
        public Task<Scheduling> UpdateSchedule(int configurationId, int schedulingId, Scheduling scheduling);
        public Task DeleteSchedule(int configurationId, int schedulingId);
    }
}
