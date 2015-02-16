using System;
using System.Globalization;

public static class DateExtensions
{
    // returns true if the current year is Leap Year (February 29th), if not returns false
    public static bool IsLeapYear(this DateTimeOffset dto)
    {
        return dto.Year % 400 == 0 || (dto.Year % 4 == 0 && dto.Year % 100 != 0);
    }

    // returns the number of days in the given month
    public static int GetDaysInMonth(this DateTimeOffset dto)
    {
        int days = new int[12] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 }[dto.Month - 1];

        if (dto.IsLeapYear() && dto.Month == 2)
        {
            days += 1;
        }

        return days;
    }

    // returns the day of the week the given month starts on
    public static DayOfWeek GetFirstDayOfMonth(this DateTimeOffset dto)
    {
        DateTimeOffset newdto = new DateTimeOffset(dto.Year, dto.Month, 1, 0, 0, 0, TimeSpan.Zero);

        return newdto.DayOfWeek;
    }
}