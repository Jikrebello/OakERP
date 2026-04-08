namespace OakERP.Tests.Unit;

public static class TestDates
{
    public static DateOnly DaysFromToday(int daysOffset) =>
        DateOnly.FromDateTime(DateTime.Today.AddDays(daysOffset));

    public static DateTimeOffset UtcAtHourDaysFromToday(int daysOffset, int hour = 12) =>
        new(
            DateOnly
                .FromDateTime(DateTime.Today.AddDays(daysOffset))
                .ToDateTime(new TimeOnly(hour, 0), DateTimeKind.Utc)
        );

    public static DateOnly StartOfMonth(DateOnly date) => new(date.Year, date.Month, 1);

    public static DateOnly EndOfMonth(DateOnly date) => StartOfMonth(date).AddMonths(1).AddDays(-1);
}
