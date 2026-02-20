using Cronos;
using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Exceptions;
using Mirra_Portal_API.Helper;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Enums;
using Mirra_Portal_API.Services.Interfaces;

namespace Mirra_Portal_API.Services
{
    public class ConfigurationService : IConfigurationService
    {
        ICustomerPlatformConfigurationRepository _configurationRepository;
        ISchedulingRepository _schedulingRepository;
        ICustomerRepository _customerRepository;
        IdentityHelper _identityHelper;
        SymmetricEncryptionHelper _symmetricEncryptionHelper;
        ISubscriptionPlanEvaluator _subscriptionPlanEvaluator;

        public ConfigurationService(ICustomerPlatformConfigurationRepository configurationRepository,
                                    IdentityHelper identityHelper,
                                    SymmetricEncryptionHelper symmetricEncryptionHelper,
                                    ISchedulingRepository schedulingRepository,
                                    ICustomerRepository customerRepository,
                                    ISubscriptionPlanEvaluator subscriptionPlanEvaluator)
        {
            _configurationRepository = configurationRepository;
            _identityHelper = identityHelper;
            _symmetricEncryptionHelper = symmetricEncryptionHelper;
            _schedulingRepository = schedulingRepository;
            _customerRepository = customerRepository;
            _subscriptionPlanEvaluator = subscriptionPlanEvaluator;
        }

        public async Task<CustomerPlatformConfiguration> CreateConfiguration(CustomerPlatformConfiguration configuration)
        {
            validateIntervals(configuration);
            configuration.Customer = new Customer { Id = _identityHelper.UserId() };
            configuration.Password = _symmetricEncryptionHelper.Encrypt(configuration.Password);
            return await _configurationRepository.Create(configuration);
        }


        public async Task<Scheduling> CreateScheduling(int configurationId, Scheduling scheduling)
        {
            validateInterval(scheduling);
            await checkIfConfigurationBelongsToCustomer(configurationId);
            scheduling.RunsPerWeek = CalculateMaxRunsPerWeek(scheduling.Interval);
            var customer = await getCustomerByConfigurationId(configurationId);
            var schedulingAllowed = _subscriptionPlanEvaluator.checkIfRunsPerWeekAreAllowedInCustomerCurrentPlan(customer, scheduling.RunsPerWeek);
            if (!schedulingAllowed)
                throw new SubscriptionException("The number of scheduling runs per week exceeds the limit of your current subscription plan.");
            scheduling.CustomerPlatformConfiguration = new CustomerPlatformConfiguration { Id = configurationId };
            return await _schedulingRepository.Create(scheduling);
        }

        private void validateIntervals(CustomerPlatformConfiguration configuration)
        {
            if (configuration.Schedulings == null) return;

            foreach (var schedule in configuration.Schedulings)
            {
                if (!CronExpression.TryParse(schedule.Interval, CronFormat.Standard, out _))
                    throw new BadRequestException("Interval must be a 5 fields valid cron expression (ex.: \"0 2 * * 1\").");
            }
        }

        private void validateInterval(Scheduling scheduling)
        {
            if (!CronExpression.TryParse(scheduling.Interval, CronFormat.Standard, out _))
                throw new BadRequestException("Interval must be a 5 fields valid cron expression (ex.: \"0 2 * * 1\").");
        }

        public async Task<List<Scheduling>> GetConfigurationSchedulings(int configurationId)
        {
            await checkIfConfigurationBelongsToCustomer(configurationId);
            return await _schedulingRepository.GetAllByConfigurationId(configurationId);
        }



        private int CalculateMaxRunsPerWeek(string cronExpression)
        {
            var fields = cronExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length != 5)
                throw new ArgumentException("Expressão cron deve ter 5 campos");

            // Campos: 0=minuto, 1=hora, 2=dia do mês, 3=mês, 4=dia da semana
            string minute = fields[0];
            string hour = fields[1];
            string dayOfMonth = fields[2];
            string dayOfWeek = fields[4];

            // Calcula execuções por dia baseado em minuto e hora
            int runsPerDay = CalculateRunsPerDay(minute, hour);

            // Calcula quantos dias por semana podem executar
            int daysPerWeek = CalculateDaysPerWeek(dayOfMonth, dayOfWeek);

            return runsPerDay * daysPerWeek;
        }

        private int CalculateRunsPerDay(string minute, string hour)
        {
            // Cria uma expressão cron apenas com minuto e hora, executando todo dia
            string dailyCron = $"{minute} {hour} * * *";
            var cron = CronExpression.Parse(dailyCron);
            var timeZone = TimeZoneInfo.Utc;

            // Conta execuções em um único dia
            var startDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddDays(1);

            int count = 0;
            DateTime? nextRun = startDate.AddSeconds(-1);

            while ((nextRun = cron.GetNextOccurrence(nextRun.Value, timeZone, inclusive: false)) != null
                   && nextRun.Value < endDate)
            {
                count++;
            }

            return count;
        }

        private int CalculateDaysPerWeek(string dayOfMonth, string dayOfWeek)
        {
            bool dayOfMonthIsWildcard = dayOfMonth == "*" || dayOfMonth == "?";
            bool dayOfWeekIsWildcard = dayOfWeek == "*" || dayOfWeek == "?";

            // Ambos são wildcard = 7 dias por semana
            if (dayOfMonthIsWildcard && dayOfWeekIsWildcard)
            {
                return 7;
            }

            // Apenas dia da semana é específico
            if (dayOfMonthIsWildcard && !dayOfWeekIsWildcard)
            {
                return CountSpecificValues(dayOfWeek, 0, 6);
            }

            // Apenas dia do mês é específico
            if (!dayOfMonthIsWildcard && dayOfWeekIsWildcard)
            {
                int specificDays = CountSpecificValues(dayOfMonth, 1, 31);
                return Math.Min(specificDays, 7);
            }

            // AMBOS são específicos = UNIÃO (OR) dos dois critérios
            // No melhor cenário, os dias não se sobrepõem
            int daysFromDayOfMonth = Math.Min(CountSpecificValues(dayOfMonth, 1, 31), 7);
            int daysFromDayOfWeek = CountSpecificValues(dayOfWeek, 0, 6);

            // Máximo possível: soma dos dois, limitado a 7
            return Math.Min(daysFromDayOfMonth + daysFromDayOfWeek, 7);
        }

        private int CountSpecificValues(string field, int min, int max)
        {
            var values = new HashSet<int>();

            // Divide por vírgula para múltiplos valores
            var parts = field.Split(',');

            foreach (var part in parts)
            {
                // Verifica se é um range (ex: 1-5)
                if (part.Contains('-'))
                {
                    var rangeParts = part.Split('-');
                    int start = int.Parse(rangeParts[0]);
                    int end = int.Parse(rangeParts[1]);

                    for (int i = start; i <= end; i++)
                    {
                        values.Add(i);
                    }
                }
                // Verifica se é um step (ex: */2)
                else if (part.Contains('/'))
                {
                    var stepParts = part.Split('/');
                    int step = int.Parse(stepParts[1]);
                    int start = stepParts[0] == "*" ? min : int.Parse(stepParts[0]);

                    for (int i = start; i <= max; i += step)
                    {
                        values.Add(i);
                    }
                }
                // Valor único
                else if (int.TryParse(part, out int value))
                {
                    values.Add(value);
                }
            }

            return values.Count;
        }

        public async Task<Scheduling> GetScheduling(int configurationId, int schedulingId)
        {
            await checkIfConfigurationBelongsToCustomer(configurationId);
            await checkIfSchedulingBelongsToConfiguration(configurationId, schedulingId);
            var scheduling = await _schedulingRepository.GetById(schedulingId);
            if (scheduling == null || scheduling.CustomerPlatformConfiguration.Id != configurationId)
                throw new BadRequestException("Scheduling not found.");
            return scheduling;
        }


        public async Task<Scheduling> UpdateScheduling(int configurationId, int schedulingId, Scheduling scheduling)
        {
            await checkIfConfigurationBelongsToCustomer(configurationId);
            await checkIfSchedulingBelongsToConfiguration(configurationId, schedulingId);
            scheduling.Id = schedulingId;
            return await _schedulingRepository.Update(scheduling);
        }

        public async Task DeleteScheduling(int configurationId, int schedulingId)
        {
            await checkIfConfigurationBelongsToCustomer(configurationId);
            await checkIfSchedulingBelongsToConfiguration(configurationId, schedulingId);
            await _schedulingRepository.Delete(schedulingId);
        }

        private async Task checkIfConfigurationBelongsToCustomer(int configurationId)
        {
            var configuration = await _configurationRepository.GetById(configurationId);
            if (configuration == null || configuration.Customer.Id != _identityHelper.UserId())
                throw new NotFoundException("Configuration not found.");
        }

        private async Task checkIfSchedulingBelongsToConfiguration(int configurationId, int schedulingId)
        {
            var scheduling = await _schedulingRepository.GetById(schedulingId);
            if (scheduling == null || scheduling.CustomerPlatformConfiguration.Id != configurationId)
                throw new NotFoundException("Scheduling not found.");
        }

        private async Task<Customer> getCustomerByConfigurationId(int configurationId)
        {
            return await _customerRepository.GetByConfigurationId(configurationId);
        }

        public async Task<List<CustomerPlatformConfiguration>> GetAllConfigurations()
        {
            return await _configurationRepository.GetAllForCustomer(_identityHelper.UserId());
        }

        public async Task<bool> HasSuspendedSchedulingsDueToLackOfPayment()
        {
            return await _schedulingRepository.HasAnyByCustomerIdAndStatus(
                _identityHelper.UserId(),
                ESchedulingStatus.SUSPENDED_DUE_TO_LACK_PAYMENT);
        }
    }
}
