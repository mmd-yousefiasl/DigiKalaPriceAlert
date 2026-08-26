namespace Application.Tools
{
    public static class PriceFormatHelper
    {
        /// <summary>Formats a Toman price exactly, with thousands separators.</summary>
        public static string FormatPrice(decimal price)
            => $"{price:N0} تومان";
    }
}
