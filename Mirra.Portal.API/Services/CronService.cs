using Cronos;
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
    }
}
