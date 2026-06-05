using Application.Dtos.Products.BotSide;
using Application.Dtos.SubscriptionPlans.BotSide;
using System.Net;
using Telegram.Bot.Types.ReplyMarkups;

namespace Application.Tools
{
    public static class BotMessages
    {
        // 🛠 نام و مشخصات اصلی (با رعایت نیم‌فاصله‌ها)
        public const string BotName = "دیده‌بان قیمت";
        public const string BotId = "@DigiPriceAlert_bot";
        private static readonly string BotFooter = $"\n\n🤖 <b>{BotId}</b> | {BotName}";

        // 💬 متن دکمه‌های ثابت منو
        public const string MenuMyProducts = "📦 محصولات در حال رصد";
        public const string MenuAboutUs = "🌐 درباره ما";
        public const string MenuBack = "🔙 بازگشت به منوی اصلی";
        public const string MenuMyAccount = "👤 حساب کاربری من";

        // 🔔 پیام‌های سیستم (یکپارچه و شیک با ایموجی‌های مرتبط)
        public const string UserAlreadyExistMessage = "🌐 حساب کاربری شما پیش از این فعال شده است.";
        public const string SuccessfullyDone = "🟢 <b>عملیات با موفقیت انجام شد.</b>";
        public const string InvalidOrderError = "⚠️ <b>درخواست نامعتبر!</b>\nلطفاً دوباره تلاش کنید.";
        public const string ProductAlreadyInWatchListError = "⚠️ <b>کالای تکراری!</b>\nاین محصول در حال حاضر در لیست رصد شما قرار دارد.";
        public static readonly string OperationFailedError = $"🚨 <b>خطای سیستمی!</b>\nمتأسفانه در پردازش درخواست شما مشکلی رخ داد. لطفاً دقایقی دیگر امتحان کنید.{BotFooter}";
        public static readonly string ProductNotFoundError = $"🔍 <b>کالا یافت نشد!</b>\nمحصولی با این مشخصات در دیجی‌کالا پیدا نشد یا لینک نامعتبر است.{BotFooter}";
        public static readonly string InvalidLinkError = $"❌ <b>فرمت نامعتبر!</b>\nلطفاً یک «لینک معتبر» از دیجی‌کالا و یا فقط «کد عددی DKP» محصول را ارسال کنید.{BotFooter}";
        public const string SearchingDigiKalaMessage = "⚡️ <b>در حال ارتباط با دیجی‌کالا...</b>\nلطفاً چند لحظه شکیبا باشید ⏳";
        public const string OnlyIranPhoneNumberError = "این ربات فقط از شماره‌های موبایل ایران پشتیبانی می‌کند. لطفاً با سیم‌کارت ایرانی خود اقدام کنید";
        public const string ShareYourPhoneNumberToCreateAccountAndConnectToWebsite =
            "⚠️ <b>حساب کاربری شما یافت نشد!</b>\n\n" +
            "برای استفاده از ربات، ثبت محصولات و اطلاع از تغییر قیمت‌ها، ابتدا باید شماره موبایل خود را تایید کنید.\n\n" +
            "👇 لطفاً روی دکمهٔ <b>«📱 اشتراک‌گذاری شماره موبایل»</b> در پایین صفحه کلیک کنید.";
        public const string InvalidContactError =
            "❌ <b>خطا در تأیید هویت!</b>\n\n" +
            "لطفاً فقط از دکمهٔ <b>«📱 اشتراک‌گذاری شماره موبایل»</b> استفاده کنید و شمارهٔ متعلق به خودتان را بفرستید.\n\n" +
            "ارسال شمارهٔ سایر مخاطبین امکان‌پذیر نیست.";

        // 📦 پیام‌های مدیریت لیست رصد
        public const string ProductCheckListEmptyError = "📭 <b>لیست رصد شما خالی است!</b>\n\nبرای شروع، کافیست لینک یا کد DKP یک محصول از دیجی‌کالا را برای من ارسال کنید.";
        public const string ConfirmProductDeleteMessage = "⚠️ تایید حذف\nآیا از حذف این کالا از لیست رصد خود اطمینان دارید؟";
        public const string YouDontCheckThisProductError = "❌ این محصول در لیست رصد شما وجود ندارد یا قبلاً حذف شده است.";
        public const string ProductNotFoundInServerError = "⚠️ <b>خطا!</b>\nمشخصات این کالا در سیستم یافت نشد.";
        public const string ProductDeletedSuccessfullyMessage = "🗑️ کالا با موفقیت از لیست رصد شما حذف شد.";
        public const string OperationCancelledMessage = "🔙 عملیات لغو شد.";
        public static string CantAddMoreProductsError(int maxCount) =>
            $"🛑 <b>محدودیت تعداد کالا!</b>\n\n" +
            $"شما به سقف مجاز <b>{maxCount} کالا</b> در لیست رصد خود رسیده‌اید و امکان افزودن کالای جدید را ندارید.\n\n" +
            $"💳 برای مشاهده مشخصات اشتراک فعلی یا ارتقای آن، لطفاً روی دکمهٔ زیر کلیک کنید👇";

        // 🌟 پیام‌های پویا (متدها)
        public static string WelcomeMessage(string? firstName) =>
            $"✨ <b>سلام {WebUtility.HtmlEncode(firstName ?? "کاربر گرامی")}! به {BotName} خوش آمدید</b> 🌹\n\n" +
            $"من شبانه‌روز نوسانات قیمت کالاهای مورد علاقه‌ات رو زیر نظر می‌گیرم و به محض کوچک‌ترین <b>کاهش یا افزایش قیمت</b>، سریعاً بهت خبر می‌دم! 📉📈\n\n" +
            $"📥 <b>چطور کار می‌کنه؟</b>\n" +
            $"کافیه <b>« لینک کالا »</b> یا <b>« کد عددی DKP »</b> اون رو از دیجی‌کالا برام بفرستی تا بلافاصله کارم رو شروع کنم. 🚀" +
            BotFooter;

        public static string AboutUsMessage =
            $"🌐 <b>درباره دستیار هوشمند {BotName}</b>\n\n" +
            $"این ربات با هدف پایش دقیق و مانیتورینگ ۲۴ ساعته قیمت محصولات دیجی‌کالا برای شما طراحی شده است. " +
            $"به کمک این ابزار، دیگر نیازی به چک کردن دستی و مداوم صفحات نیست؛ سیستم به محض هرگونه <b>کاهش یا افزایش قیمت</b>، فوراً شما را باخبر می‌کند تا در بهترین زمان تصمیم‌گیری کنید. 🎯\n\n" +
            $"✨ <b>امکانات کلیدی:</b>\n" +
            $"• پایش خودکار و لحظه‌ای تغییرات قیمت\n" +
            $"• امکان مدیریت و رصد همزمان تا ۱۰ محصول\n" +
            $"• اطلاع‌رسانی دقیق از وضعیت موجودی کالا\n" +
            $"• رابط کاربری سریع، ساده و شیشه‌ای\n\n" +
            $"🛠 <b>مشخصات فنی سرور:</b>\n" +
            $"توسعه‌یافته با معماری پرسرعت <b>.NET 10</b> و پایگاه‌داده قدرتمند <b>PostgreSQL</b> جهت تضمین پایداری و پردازش بی‌درنگ داده‌ها. 🚀" +
            BotFooter;

        public static string SubscriptionStatusMessage(BotSideSubscriptionPlanDto plan)
        {
            var message = $"💳 <b>وضعیت حساب و اشتراک شما</b>\n\n" +
                          $"📌 <b>نوع اشتراک فعلی:</b> {plan.Name}\n" +
                          $"📦 <b>سقف مجاز رصد کالا:</b> {plan.MaximumProductCount} کالا\n" +
                          $"⏱ <b>وضعیت اعتبار:</b> {plan.StatusDescription}\n";

            // اگر اشتراک ویژه فعال بود و تاریخ پایان داشت
            if (plan.SubscriptionEndDate.HasValue)
            {
                var remainingDays = (plan.SubscriptionEndDate.Value - DateTime.UtcNow).Days;
                var displayDays = remainingDays < 0 ? 0 : remainingDays;

                message += $"📅 <b>تاریخ اتمام اشتراک:</b> {plan.SubscriptionEndDate.Value.ConvertToShamsi()}\n" +
                           $"⏳ <b>زمان باقی‌مانده:</b> {displayDays} روز\n\n" +
                           $"🚀 برای ارتقای ظرفیت و استفاده از امکانات ویژه، می‌توانید اشتراک خود را ارتقا دهید.\n";
            }
            else
            {
                message += "\n🚀 با ارتقای اشتراک خود، کالا‌های بیشتری را به صورت همزمان رصد کنید!\n";
            }

            message += BotFooter;
            return message;
        }

        public static string SearchingMultipleDigiKalaMessage(int count) => $"⚡️ در حال بررسی {count} محصول...\nلطفاً چند لحظه شکیبا باشید ⏳";

        public static string UserProductsWatchListTitle(int count) =>
            $"📋 <b>لیست کالاهای در حال رصد شما ({count} مورد):</b>\n\n" +
            $"جهت مشاهده جزئیات یا حذف هر محصول، روی دکمه مربوطه در منوی زیر کلیک کنید 👇";

        public static string TelegramAlert(string title, decimal? oldPrice, decimal? newPrice)
        {
            // تبدیل مقادیر نال به صفر برای ساده شدن منطق مقایسه
            decimal oldP = oldPrice ?? 0;
            decimal newP = newPrice ?? 0;

            // استفاده از Tuple Switch (Pattern Matching) برای خوانایی فوق‌العاده بالاتر
            var (statusText, priceSection) = (oldP, newP) switch
            {
                // ۱. کالا قبلا موجود بوده، الان ناموجود شده
                ( > 0, 0) => (
                    "🔴 ناموجود شد",
                    $"🔹 <b>آخرین قیمت:</b> {oldP.ToString("N0")} تومان\n" +
                    $"🔸 <b>وضعیت فعلی:</b> ❌ ناموجود"
                ),

                // ۲. کالا ناموجود بوده، الان موجود شده
                (0, > 0) => (
                    "🟢 موجود شد!",
                    $"🔹 <b>قیمت ثبت شده:</b> ❌ ناموجود\n" +
                    $"🔸 <b>قیمت جدید:</b> {newP.ToString("N0")} تومان"
                ),

                // ۳. تغییر قیمت عادی (ارزان یا گران شدن)
                ( > 0, > 0) => HandlePriceChange(oldP, newP),

                // ۴. در هر حالت دیگری (مثلاً صفر بودن هر دو یا مقادیر منفی غیرمجاز)
                _ => (
                    "🔴 ناموجود",
                    "🔸 <b>وضعیت فعلی:</b> ❌ در حال حاضر ناموجود است"
                )
            };

            // خروجی نهایی بدون کوچک‌ترین تغییر در ساختار پیام اصلی شما
            return $"🔔 <b>گزارش تغییرات {BotName}</b> 🔔\n\n" +
                   $"📦 <b>کالا:</b> {WebUtility.HtmlEncode(title)}\n" +
                   $"📊 <b>وضعیت:</b> {statusText}\n\n" +
                   $"--- 💰 اطلاعات قیمت ---\n" +
                   $"{priceSection}\n\n" +
                   BotFooter;
        }

        private static (string statusText, string priceSection) HandlePriceChange(decimal oldP, decimal newP)
        {
            string statusText;

            if (newP < oldP)
            {
                // گرد کردن تا ۲ رقم اعشار
                decimal percent = Math.Round(((oldP - newP) / oldP) * 100, 2);
                // استفاده از F2 برای تضمین نمایش دقیق ۲ رقم اعشار (مثلاً 12.50)
                statusText = $"📉 ارزان شد! ({percent:F2}% کاهش)";
            }
            else if (newP > oldP)
            {
                // گرد کردن تا ۲ رقم اعشار
                decimal percent = Math.Round(((newP - oldP) / oldP) * 100, 2);
                // استفاده از F2 برای تضمین نمایش دقیق ۲ رقم اعشار
                statusText = $"📈 گران شد! ({percent:F2}% افزایش)";
            }
            else
            {
                statusText = "📊 بدون تغییر";
            }

            string priceSection = $"🔹 <b>قیمت قبلی:</b> {oldP.ToString("N0")} تومان\n" +
                                  $"🔸 <b>قیمت جدید:</b> {newP.ToString("N0")} تومان";

            return (statusText, priceSection);
        }

        public static ReplyKeyboardMarkup GetMainMenuMarkup()
        {
            return new ReplyKeyboardMarkup(new[]
                {
        new[] { new KeyboardButton(MenuMyProducts) },
        new[] { new KeyboardButton(MenuMyAccount) },
        new[] { new KeyboardButton(MenuAboutUs) }
    })
            {
                ResizeKeyboard = true
            };
        }

        public static InlineKeyboardMarkup GetProductReplyMarkup(string dkp)
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithUrl("🔗 مشاهده در دیجی‌کالا", $"https://www.digikala.com/product/dkp-{dkp}/") },
                new[] { InlineKeyboardButton.WithCallbackData("🗑 حذف از لیست رصد", $"confirm_delete:{dkp}") }
            });
        }

        public static InlineKeyboardMarkup GetDeleteConfirmationMarkup(string dkp)
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("✅ بله، مطمئنم", $"delete_yes:{dkp}"),
                    InlineKeyboardButton.WithCallbackData("❌ خیر، انصراف", $"delete_no:{dkp}")
                }
            });
        }

        public static InlineKeyboardMarkup GetUserProductsListMarkup(List<BotSideUserWatchListProductsDto> products)
        {
            var rows = new List<List<InlineKeyboardButton>>();

            foreach (var product in products)
            {
                // محدود کردن هوشمند عنوان برای جلوگیری از به هم ریختگی دکمه‌های تلگرام
                string fullTitle = product.Title.Length > 45 ? product.Title[..45] + "..." : product.Title;

                var button = InlineKeyboardButton.WithCallbackData($"📦 {fullTitle}", $"view_product:{product.DigiKalaId}");

                rows.Add(new List<InlineKeyboardButton> { button });
            }

            return new InlineKeyboardMarkup(rows);
        }

        public static ReplyKeyboardMarkup GetUserContactMarkup()
        {
            var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
            {
        new KeyboardButton("📱 اشتراک‌گذاری شماره موبایل") { RequestContact = true }
    })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };

            return replyKeyboardMarkup;
        }

        public static InlineKeyboardMarkup GetSubscriptionLimitReplyMarkup()
        {
            return new InlineKeyboardMarkup(new[]
            {
        new[] { InlineKeyboardButton.WithCallbackData("💎 مشاهده مشخصات اشتراک من", "view_subscription_status") }});
        }

        public static InlineKeyboardMarkup GetSubscriptionUpgradeReplyMarkup(string targetUrl)
        {
            return new InlineKeyboardMarkup(new[]
            {
        new[] { InlineKeyboardButton.WithUrl("💳 خرید یا ارتقای اشتراک", targetUrl) }});
        }
    }
}