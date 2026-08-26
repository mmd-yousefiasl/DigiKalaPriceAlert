namespace Domain.Statics
{
    public class SiteTools
    {
        #region GeneralInfo
        public static string SiteUrl { get; set; }

        public static string ENemadUrl { get; set; }

        public static string ENemadLogo { get; set; }

        public static string SupportEmailAddress { get; set; }

        public static string SiteName { get; set; }

        public static string DefaultImage { get; set; }

        public static string PaymentLogo { get; set; }

        public static int LoginExpirationInDays { get; set; }
        #endregion

        #region Blog
        public static string BlogFile { get; set; }
        #endregion

        #region Social Media
        public static string SocialMediaFile { get; set; }
        #endregion

        #region  Price Monitor
        public static int MaximumProductCount { get; set; }
        public static int PriceMonitorBatchSize { get; set; }
        #endregion

        #region Tickets
        public static int MaxTicketAttachmentLength { get; set; }
        public static string TicketAttachmentFile { get; set; }

        #endregion

        #region Payment

        public static string PaymentRequestURL { get; set; }
        public static string CallBackURL { get; set; }
        public static string StartPaymentURL { get; set; }
        public static string VerifyPaymentURL { get; set; }

        #endregion

        #region CacheManager
        public static int CachesDefaultExpirationTimeInMinutes { get; set; }
        public static int UserPermissionCacheExpirationTimeInMinutes { get; set; }
        #endregion

        #region Mediator
        public static string MediatorLicenceKey { get; set; }
        #endregion

        #region Medium
        public static string MediumFile { get; set; }
        public static int MaxMediumFileLength { get; set; }
        #endregion

        #region Visior
        public static string VisitorKeyName { get; set; }
        public static int VisitorExpirationDays { get; set; }
        #endregion

        #region Telegram

        public static string TelegramWorkerKey { get; set; }
        public static string TelegramWorkerSendUrl { get; set; }
        public static string TelegramBotUsername { get; set; }

        public static string AdminTelegramWorkerKey { get; set; }
        public static string AdminTelegramWorkerSendUrl { get; set; }

        // Private channels (ids are negative, e.g. -100xxxxxxxxxx; bot must be admin).
        public static long TelegramNotificationChannelId { get; set; }
        public static long TelegramErrorChannelId { get; set; }

        public static bool TelegramErrorAlertingEnabled { get; set; }

        #endregion
    }
}