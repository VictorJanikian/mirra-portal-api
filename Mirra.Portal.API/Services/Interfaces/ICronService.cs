namespace Mirra_Portal_API.Services.Interfaces
{
    public interface ICronService
    {
        public int CalculateMaxRunsPerWeek(string cronExpression);
    }
}
