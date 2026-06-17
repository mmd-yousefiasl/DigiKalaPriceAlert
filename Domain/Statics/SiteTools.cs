namespace Domain.Statics
{
    public class SiteTools
    {
        public static string SiteUrl { get; set; }

        public static string SiteName { get; set; }

        public static string BlogFile { get; set; }

        public static string DefaultImage { get; set; }

        public static string SocialMediaFile { get; set; }

        public static string PaymentLogo { get; set; }

        #region Payment

        public static string PaymentRequestURL { get; set; }
        public static string CallBackURL { get; set; }
        public static string StartPaymentURL { get; set; }
        public static string VerifyPaymentURL { get; set; }

        #endregion
    }
}
