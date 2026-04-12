using System;

namespace SharedModels;

public static class BrazilDateTime
{
    private static readonly TimeZoneInfo _tz =
        TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public static DateTime Now =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _tz);
}
