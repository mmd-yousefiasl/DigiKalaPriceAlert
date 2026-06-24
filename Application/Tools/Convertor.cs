using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace Application.Tools
{
    public static class Convertor
    {
        public static long CalculateKB(this int mb)
        {
            return mb * 1024;
        }

        private static readonly string[] PersianMonthNames =
                { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند" };

        public static string GetDate(this DateTime value)
        {
            // تبدیل زمان به وقت محلی ایران با منطق اشتراکی
            DateTime localTime = value.NormalizeToIranTimeZone();

            PersianCalendar pc = new PersianCalendar();
            int day = pc.GetDayOfMonth(localTime);
            int month = pc.GetMonth(localTime);
            int year = pc.GetYear(localTime);

            return $"{day} {PersianMonthNames[month - 1]} {year}";
        }

        public static string GetDateTime(this DateTime value)
        {
            if (value == DateTime.MinValue || value.Year < 622)
            {
                return "ثبت نشده";
            }

            DateTime localTime = value.NormalizeToIranTimeZone();

            if (localTime.Year < 622)
            {
                return "ثبت نشده";
            }

            PersianCalendar pc = new PersianCalendar();
            int day = pc.GetDayOfMonth(localTime);
            int month = pc.GetMonth(localTime);
            int year = pc.GetYear(localTime);
            int hour = pc.GetHour(localTime);
            int minute = pc.GetMinute(localTime);

            return $"{day} {PersianMonthNames[month - 1]} {year} {hour:D2}:{minute:D2}";
        }

        public static string ToPersianRelativeTime(this DateTime utcDateTime)
        {
            var now = DateTime.UtcNow;

            if (utcDateTime > now) return "لحظاتی پیش";

            var timeSpan = now - utcDateTime;

            string result = timeSpan.TotalSeconds switch
            {
                < 60 => "لحظاتی پیش",
                < 3600 => $"{Math.Floor(timeSpan.TotalMinutes)} دقیقه قبل",
                < 86400 => $"{Math.Floor(timeSpan.TotalHours)} ساعت قبل",
                _ => $"{Math.Floor(timeSpan.TotalDays)} روز پیش"
            };

            return result;
        }

        private static DateTime NormalizeToIranTimeZone(this DateTime value)
        {
            if (value.Kind != DateTimeKind.Utc && value.Kind != DateTimeKind.Unspecified)
                return value;

            try
            {
                TimeZoneInfo winZone = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(value, winZone);
            }
            catch (TimeZoneNotFoundException)
            {
                TimeZoneInfo linuxZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");
                return TimeZoneInfo.ConvertTime(value, linuxZone);
            }
        }

        public static string GetDisplayName(this Enum value)
        {
            if (value == null) return "نامشخص";

            var type = value.GetType();

            var name = Enum.GetName(type, value);
            if (name == null) return value.ToString();

            var fieldInfo = type.GetField(name);
            if (fieldInfo == null) return value.ToString();

            var attribute = fieldInfo.GetCustomAttribute<DisplayAttribute>();

            return attribute != null ? attribute.Name : value.ToString();
        }

        public static string ToTomanFormat(this decimal value)
        {
            return value.ToString("#,0");
        }

        public static string ToToman(this decimal value)
        {
            return value.ToString("#,0 تومان");
        }

        public static string GetDigiKalaProductUrl(this string digiKalaId)
        {
            return "https://www.digikala.com/product/dkp-" + digiKalaId + "/";
        }
    }
}
