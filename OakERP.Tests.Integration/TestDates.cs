namespace OakERP.Tests.Integration;

public static class TestDates
{
    public static DateOnly DaysFromToday(int daysOffset) =>
        DateOnly.FromDateTime(DateTime.Today.AddDays(daysOffset));

    public static DateOnly StartOfMonth(DateOnly date) => new(date.Year, date.Month, 1);

    public static DateOnly EndOfMonth(DateOnly date) => StartOfMonth(date).AddMonths(1).AddDays(-1);
}
