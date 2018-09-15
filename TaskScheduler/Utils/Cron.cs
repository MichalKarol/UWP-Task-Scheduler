using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskScheduler
{
    class Cron {
        public class CronStructure  {
            public SortedSet<int> Minutes { get; set; }
            public SortedSet<int> Hours { get; set; }
            public SortedSet<int> Days { get; set; }
            public SortedSet<int> Months { get; set; }
            public SortedSet<int> DaysOfWeek { get; set; }


            private IEnumerable<DateTime> OccurenceGeneratorMethod()
            {
                DateTime init = DateTime.Now;
                int year = init.Year;
                DateTime output = init;

                while (true)
                {
                    foreach (int month in Months)
                    {
                        // Initial test
                        if (year == init.Year && month < init.Month)
                            continue;

                        int maxDays = DateTime.DaysInMonth(year, month);
                        foreach (int day in Days)
                        {
                            if (year == init.Year && month == init.Month && day < init.Day)
                                continue;

                            if (day > maxDays)
                                continue;

                            output = new DateTime(year, month, day);
                            if (DaysOfWeek.Any(dow => dow - 1 == (int)output.DayOfWeek))
                            {
                                foreach (int hour in Hours)
                                {
                                    if (year == init.Year && month == init.Month && day == init.Day && hour < init.Hour)
                                        continue;

                                    foreach (int minute in Minutes)
                                    {
                                        if (year == init.Year && month == init.Month && day == init.Day && hour == init.Hour && minute < init.Minute)
                                            continue;

                                        yield return new DateTime(year, month, day, hour, minute, 0); ;
                                    }
                                }
                            }
                        }
                    }
                    year += 1;

                    if (year > init.Year + 10)
                        yield break;
                }
            }
            private IEnumerator<DateTime> OccurenceGenerator;
            public IEnumerable<DateTime> NextOccurence {
                get {
                    while (OccurenceGenerator.MoveNext()) {
                        yield return OccurenceGenerator.Current;
                    }
                    yield break;
                }
            }

            public CronStructure() {
                OccurenceGenerator = OccurenceGeneratorMethod().GetEnumerator();
            }
        }

        public static CronStructure ParseString(string cronString) {
            string[] cronParts = cronString.Split(new char[] { ' ' });

            if (cronParts.Length != 5)
                throw new ArgumentException("Cron string not formatted properly (number of parts)");

            Func<string, int, int, SortedSet<int>> convertToList = (part, min, max) => {
                SortedSet<int> values = new SortedSet<int>();

                // Parse direct list
                if (part.Contains(',')) {
                    string[] singleItems = part.Split(',');
                    try {
                        values = new SortedSet<int>(singleItems.Select(int.Parse));
                        bool test = values.All(v => v <= max && v >= min);
                        if (!test)
                            throw new ArgumentException("Cron part list value outside the values range.");

                        if (values.Count == 0)
                            throw new ArgumentException("Cron part values list is empty.");

                        return values;
                    } catch(FormatException) {
                        throw new ArgumentException("Cron part list not parsable.");
                    }
                }

                // Parse cycles
                int? cycle = null;
                if (part.Contains('/')) {
                    int index = part.IndexOf('/');
                    string cycleString = part.Substring(index + 1, part.Length - index - 1);
                    try {
                        int cycleValue = int.Parse(cycleString);
                        if (cycleValue > max || cycleValue < min)
                            throw new ArgumentException("Cron part cycle value outside the values range.");
                        cycle = cycleValue;

                    } catch(FormatException) {
                        throw new ArgumentException("Cron part cycle not parsable.");
                    }
                    part = part.Substring(0, index);
                }

                // Parts ranges or literal
                if (part == "*") {
                    values = new SortedSet<int>(Enumerable.Range(min, max - min + 1));
                } else if (part.Contains('-')) {
                    string[] ranges = part.Split('-');

                    if (ranges.Length != 2)
                        throw new ArgumentException("Cron part range not formatted properly (number of parts)");

                    try {
                        int start = int.Parse(ranges[0]);
                        int end = int.Parse(ranges[1]);

                        if (start > max || end > max || start < min || end < min || start > end)
                            throw new ArgumentException("Cron part range value outside the values range.");

                        values = new SortedSet<int>(Enumerable.Range(start, end));

                    } catch (FormatException) {
                        throw new ArgumentException("Cron part range not parsable.");
                    }
            } else {
                    try {
                        int literal = int.Parse(part);
                        if (literal > max || literal < min)
                            throw new ArgumentException("Cron part literal outside the values range.");

                        if (cycle.HasValue)
                            throw new ArgumentException("Cron part literal with cycle.");
                        values = new SortedSet<int>{ literal };

                    } catch(FormatException) {
                        throw new ArgumentException("Cron part type not parsable.");
                    }
                }

                if (cycle.HasValue)
                    values = new SortedSet<int>(values.Where(v => v % cycle == 0));

                if (values.Count == 0)
                    throw new ArgumentException("Cron part values list is empty.");

                return values;
            };
            return new CronStructure {
                Minutes = convertToList(cronParts[0], 0, 59),
                Hours = convertToList(cronParts[1], 0, 23),
                Days = convertToList(cronParts[2], 1, 31),
                Months = convertToList(cronParts[3], 1, 12),
                DaysOfWeek = convertToList(cronParts[4], 0, 6),
            };
        }
    }
}
