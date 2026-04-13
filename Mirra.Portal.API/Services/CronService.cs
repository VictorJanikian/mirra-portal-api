using Cronos;
using Mirra_Portal_API.Exceptions;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Services.Interfaces;

namespace Mirra_Portal_API.Services
{
    public class CronService : ICronService
    {
        public int CalculateMaxRunsPerWeek(string cronExpression)
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

        public void ValidateIntervalsFormat(CustomerPlatformConfiguration configuration)
        {
            if (configuration.Schedulings == null) return;

            foreach (var schedule in configuration.Schedulings)
            {
                ValidateIntervalFormat(schedule);
            }
        }

        public async Task<int> CalculateTotalRunsPerWeekForConfiguration(CustomerPlatformConfiguration configuration)
        {

            int totalRunsPerWeek = 0;

            foreach (var schedule in configuration.Schedulings)
                totalRunsPerWeek += CalculateMaxRunsPerWeek(schedule.Interval);

            return totalRunsPerWeek;
        }

        public void ValidateIntervalFormat(Scheduling scheduling)
        {
            if (!CronExpression.TryParse(scheduling.Interval, CronFormat.Standard, out _))
                throw new BadRequestException("Interval must be a 5 fields valid cron expression (ex.: \"0 2 * * 1\").");

            if (!scheduling.Interval.StartsWith("0 ")
                && !scheduling.Interval.StartsWith("15 ")
                && !scheduling.Interval.StartsWith("30 ")
                && !scheduling.Interval.StartsWith("45 "))

                throw new BadRequestException("Minute must be either 0, 15, 30 or 45 (ex.: \"0 2 * * 1\").");
        }

        public string ConvertCronToUtc(string cronExpression, string timeZoneId)
        {
            if (string.IsNullOrWhiteSpace(timeZoneId))
                throw new BadRequestException("Timezone is required.");

            TimeZoneInfo tz;
            try
            {
                tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                throw new BadRequestException($"Invalid timezone: '{timeZoneId}'.");
            }

            var offset = tz.BaseUtcOffset;
            if (offset == TimeSpan.Zero) return cronExpression;

            var fields = cronExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length != 5)
                throw new BadRequestException("Cron expression must have 5 fields.");

            int localMinute = int.Parse(fields[0]);
            int offsetTotalMinutes = (int)offset.TotalMinutes;

            var localHours = ExpandField(fields[1], 0, 23);

            var utcHours = new SortedSet<int>();
            int? consistentDayShift = null;
            int utcMinute = 0;

            foreach (int localHour in localHours)
            {
                utcMinute = calculateUtcMinutesAndHours(localMinute, offsetTotalMinutes, utcHours, ref consistentDayShift, localHour);
            }

            string utcHourField = CompressField(utcHours.ToList());

            string dayOfWeekField = fields[4];
            string dayOfMonthField = fields[2];

            if (consistentDayShift.HasValue && consistentDayShift.Value != 0)
            {
                dayOfWeekField = calculateUtcDayOfWeek(consistentDayShift, dayOfWeekField);

                dayOfMonthField = calculateUtcDayOfMonth(consistentDayShift, dayOfMonthField);
            }

            return $"{utcMinute} {utcHourField} {dayOfMonthField} {fields[3]} {dayOfWeekField}";
        }

        private string calculateUtcDayOfMonth(int? consistentDayShift, string dayOfMonthField)
        {
            if (dayOfMonthField != "*" && dayOfMonthField != "?")
            {
                var localDays = ExpandField(dayOfMonthField, 1, 31);
                var utcDays = new SortedSet<int>();
                foreach (int day in localDays)
                {
                    int shifted = day + consistentDayShift.Value;
                    if (shifted < 1) shifted += 31;
                    if (shifted > 31) shifted -= 31;
                    utcDays.Add(shifted);
                }
                dayOfMonthField = CompressField(utcDays.ToList());
            }

            return dayOfMonthField;
        }

        private string calculateUtcDayOfWeek(int? consistentDayShift, string dayOfWeekField)
        {
            if (dayOfWeekField != "*" && dayOfWeekField != "?")
            {
                var localDays = ExpandField(dayOfWeekField, 0, 6);
                var utcDays = new SortedSet<int>();
                foreach (int day in localDays)
                {
                    int shifted = (day + consistentDayShift.Value % 7 + 7) % 7;
                    utcDays.Add(shifted);
                }
                dayOfWeekField = CompressField(utcDays.ToList());
            }

            return dayOfWeekField;
        }

        private static int calculateUtcMinutesAndHours(int localMinute, int offsetTotalMinutes, SortedSet<int> utcHours, ref int? consistentDayShift, int localHour)
        {
            int utcMinute;
            int totalUtcMinutes = localHour * 60 + localMinute - offsetTotalMinutes;

            int dayShift = 0;
            while (totalUtcMinutes < 0) { totalUtcMinutes += 1440; dayShift--; }
            while (totalUtcMinutes >= 1440) { totalUtcMinutes -= 1440; dayShift++; }

            utcMinute = totalUtcMinutes % 60;
            utcHours.Add(totalUtcMinutes / 60);

            if (consistentDayShift == null)
                consistentDayShift = dayShift;
            else if (consistentDayShift != dayShift)
                throw new BadRequestException(
                    "The combination of this cron expression and timezone results in a day boundary split that cannot be represented as a single UTC cron expression. Please simplify the hour field.");
            return utcMinute;
        }

        private SortedSet<int> ExpandField(string field, int min, int max)
        {
            var values = new SortedSet<int>();

            if (field == "*" || field == "?")
            {
                for (int i = min; i <= max; i++)
                    values.Add(i);
                return values;
            }

            foreach (var part in field.Split(','))
            {
                if (part.Contains('/'))
                {
                    var stepParts = part.Split('/');
                    int step = int.Parse(stepParts[1]);
                    int start = stepParts[0] == "*" ? min : int.Parse(stepParts[0]);
                    for (int i = start; i <= max; i += step)
                        values.Add(i);
                }
                else if (part.Contains('-'))
                {
                    var rangeParts = part.Split('-');
                    int start = int.Parse(rangeParts[0]);
                    int end = int.Parse(rangeParts[1]);
                    for (int i = start; i <= end; i++)
                        values.Add(i);
                }
                else if (int.TryParse(part, out int value))
                {
                    values.Add(value);
                }
            }

            return values;
        }

        private string CompressField(List<int> values)
        {
            if (values.Count == 0) return "*";

            var parts = new List<string>();
            int i = 0;

            while (i < values.Count)
            {
                int start = values[i];
                int end = start;

                while (i + 1 < values.Count && values[i + 1] == end + 1)
                {
                    end = values[++i];
                }

                if (start == end)
                    parts.Add(start.ToString());
                else if (end == start + 1)
                {
                    parts.Add(start.ToString());
                    parts.Add(end.ToString());
                }
                else
                    parts.Add($"{start}-{end}");

                i++;
            }

            return string.Join(",", parts);
        }
    }
}
