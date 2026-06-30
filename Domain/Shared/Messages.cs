namespace Domain.Shared
{
    public static class Messages
    {
        #region Error Messages

        public const string RequiredError = "{0} ضروری می باشد";
        public const string OperationFailedError = "عملیات با خطا مواجه شد";
        public const string UserDoesNotExistError = "کاربر پیدا نشد";
        public const string MaxLengthError = "تعداد کاراکتر بیشتر از حد مجاز می باشد ({1})";
        public const string MobileFormatError = "موبایل نامعتبر می باشد";
        public const string ToManyLoginAttemptError = "تعداد تلاش‌های شما برای ورود از حد مجاز فراتر رفته است. لطفاً بعد از دقایقی دوباره تلاش کنید";
        public const string InvalidLoginCodeError = "کد تایید نامعتبر می باشد.";
        public const string LoginFailedError = "ورود ناموفق، لطفا دوباره امتحان کنید";
        public const string ThisPhoneNumberAlreadyRegisteredError = "این شماره موبایل قبلا ثبت شده است";
        public const string RequiredAccessForRoleError = "حداقل یک دسترسی برای نقش انتخاب کنید";
        public const string DuplicateUniqueNameForRoleError = "نام یکتا برای این نقش قبلاً ثبت شده است";
        public const string ThisEmailAlreadyRegisterdError = "این ایمیل قبلا ثبت شده است";
        public const string RangeError = "حداقل مقدار {0} {1} و حداکثر {2} می باشد";
        public const string DuplicateSlugError = "عنوان در مرورگر تکراری می باشد";
        public const string CantDeleteThisRecordError = "نمی‌توان این رکورد را حذف کرد";
        public const string InvalidFileFormatError = "فرمت فایل نامعتبر است";
        public const string FileLengthError = "طول فایل بیش از حد مجاز است. (حداکثر 400KB)";
        public const string UserBalnaceCantBeLessThanZeroError = "موجودی کاربر نمی تواند منفی باشد";
        public const string YouAlreadyUsedFreePlanError = "طرح رایگان قبلاً برای این شماره فعال شده است";
        public const string InvalidPlanError = "پلن انتخابی معتبر نیست یا غیرفعال شده است";
        public const string EmptyInvoiceError = "سبد خرید یا فاکتور انتخاب‌شده خالی است. لطفاً ابتدا پلن مورد نظر خود را انتخاب کنید.";
        public const string PaymentFailedError = "پرداخت با شکست مواجه شد";
        public const string TransactionAlreadyVerified = "تراکنش قبلاً تأیید شده است";
        public const string InSufficentBalanceError = "موجودی کافی برای انجام تراکنش وجود ندارد";
        public const string InvalidDigiKalaProductUrlError = "آدرس وارد شده یک لینک معتبر از محصول دیجی‌کالا نمی‌باشد";
        public const string UserReachedMaximumProductsError = "تعداد محصولات شما به سقف مجاز رسیده است";
        public const string UpdateBlogCategoriesSiteMapError = "خطایی هنگام بروزرسانی دسته بندی مقالات رخ داد";
        public const string UpdateBlogSiteMapError = "خطایی هنگام بروزرسانی مقالات رخ داد";
        public const string FailedToSendSmsError = "ارسال پیامک با خطا مواجه شد. لطفا مجددا تلاش کنید";
        public const string LoginCodeExpiredError = "کد تایید شما منقضی شده است. لطفا مجددا درخواست کد کنید";

        #endregion

        #region Success Messages

        public const string SuccessfullyDone = "عملیات با موفقیت به پایان رسید";
        public const string LoginCodeSentSuccessfully = "کد ورود با موفقیت ارسال شد";
        public const string LogedInSuccessfully = "با موفقیت وارد حساب کاربری خود شدید";
        public const string LogedoutSuccessfully = "با موفقیت از حساب کاربری خود خارج شدید";
        public const string ContactUsFormSentSuccessfully = "پیام شما با موفقیت ارسال شد، در صورت نیاز همکاران ما با شما تماس خواهند گرفت";
        public const string ContactUsFormMarkedAsReadSuccessfully = "وضعیت فرم تماس باما به خوانده شده تغییر کرد";
        public const string SignInNewsLettersSuccessfully = "عضویت شما در خبرنامه با موفقیت انجام شد. از همراهی شما سپاسگزاریم";
        public const string FreePlanActivatedSuccessfully = "طرح رایگان با موفقیت فعال شد";
        public const string PaymentDoneSuccessfully = "پرداخت با موفقیت انجام شد";
        public const string TicketClosedSuccessfully = "تیکت با موفقیت بسته شد";

        #endregion

        #region Message

        public const string ProductPriceChangedMessage = "تغییر قیمت در محصولات درحال پایش.";
        public const string IncreaseUserBalanceMessage = "افزایش موجودی کیف پول از طریق درگاه پرداخت به منظور پردازش سفارش";
        public const string RefundUserBalanceMessage = "بازگشت وجه سبد خرید به کیف پول به دلیل خطا در پردازش سفارش.";
        public const string NewTicketCreatedByUser = "تیکت جدید توسط کاربر ایجاد شد";
        public const string TicketClosedByUser = "تیکت توسط کاربر بسته شد";
        public const string NewMessageSentByUser = "پیام جدید تیکت توسط کاربر ارسال شد";
        public const string TicketClosedByAdmin = "تیکت توسط ادمین بسته شد";
        public const string NewMessageSentByAdmin = "پیام جدید تیکت توسط ادمین ارسال شد";
        public const string TicketStatusUpdatedByAdmin = "وضعیت تیکت توسط ادمین ویرایش شد";

        #endregion
    }
}
