using System;

namespace Chat_Group_System.Helpers
{
    public static class TimeHelper
    {
        public static DateTime NowVN 
        {
            get 
            {
                try
                {
                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                }
                catch (TimeZoneNotFoundException)
                {
                    // Fallback using fixed offset if exact timezone isn't found
                    return DateTime.UtcNow.AddHours(7);
                }
            }
        }
    }
}
