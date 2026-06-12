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

        #endregion

        #region Success Messages

        public const string SuccessfullyDone = "عملیات با موفقیت به پایان رسید";
        public const string LoginCodeSentSuccessfully = "کد ورود با موفقیت ارسال شد";
        public const string LogedInSuccessfully = "با موفقیت وارد حساب کاربری خود شدید";
        public const string LogedoutSuccessfully = "با موفقیت از حساب کاربری خود خارج شدید";

        #endregion

        #region Message

        public const string ProductPriceChangedMessage = "کاربر گرامی، قیمت حداقل یکی از محصولات مورد رصد شما تغییر یافته است. جهت مشاهده جزئیات به ربات مراجعه کنید.";

        #endregion
    }
}
