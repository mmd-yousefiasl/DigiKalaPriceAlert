namespace Domain.Shared
{
    public static class Messages
    {
        #region Error Messages

        public const string RequiredError = "{0} ضروری می باشد";
        public const string OperationFailedError = "عملیات با خطا مواجه شد";
        public const string UserDoesNotExistError = "کاربر پیدا نشد";

        #endregion

        #region Success Messages

        public const string SuccessfullyDone = "عملیات با موفقیت به پایان رسید";

        #endregion

        #region Message

        public const string ProductPriceChangedMessage = "کاربر گرامی، قیمت حداقل یکی از محصولات مورد رصد شما تغییر یافته است. جهت مشاهده جزئیات به ربات مراجعه کنید.";

        #endregion
    }
}
