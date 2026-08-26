using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Enums.Products;
using Domain.Events.Products;
using Domain.Statics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services.Implementaions
{
    /// <summary>
    /// Sends one Telegram market report per variant change event:
    /// resolves recipients for the COLOR GROUP, resets their unread flags,
    /// and composes a full market-status message.
    /// </summary>
    public class MarketAlertService : IMarketAlertService
    {
        private readonly DigiKalaPriceAlertDbContext _context;
        private readonly ITelegramService _telegramService;
        private readonly ILogger<MarketAlertService> _logger;

        public MarketAlertService(
            DigiKalaPriceAlertDbContext context,
            ITelegramService telegramService,
            ILogger<MarketAlertService> logger)
        {
            _context = context;
            _telegramService = telegramService;
            _logger = logger;
        }

        public async Task DispatchMarketStatusAlertAsync(
            ProductVariantMarketStatusUpdatedEvent notification,
            CancellationToken cancellationToken = default)
        {
            // 1. Recipients: every tracker of THIS COLOR GROUP with a linked
            //    telegram account. (DigiKala's variant id is per seller combo,
            //    so mappings must be matched via their seller's ColorName.)
            //    The mapping Id is kept so each user gets a link to THEIR
            //    own product-details page.
            var recipients = await _context.UserProductMappings
                .AsNoTracking()
                .Where(m =>
                    m.VariantSeller.ProductId == notification.ProductId &&
                    m.VariantSeller.ColorName == notification.VariantName &&
                    m.User.TelegramChatId != null &&
                    !m.User.IsUserBanned)
                .Select(m => new { m.UserId, MappingId = m.Id, ChatId = m.User.TelegramChatId!.Value })
                .Distinct()
                .ToListAsync(cancellationToken);

            if (recipients.Count == 0)
                return;

            // 2. Mark these mappings unread so the site list shows the change
            await _context.UserProductMappings
                .Where(m =>
                    m.VariantSeller.ProductId == notification.ProductId &&
                    m.VariantSeller.ColorName == notification.VariantName)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(m => m.IsRead, false),
                    cancellationToken);

            // 3. ONE message per user per event — with a deep link to THEIR
            //    product-details page on the site.
            var baseText = ComposeAlertMessage(notification);

            foreach (var recipient in recipients)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var detailsUrl =
                    $"{SiteTools.SiteUrl.TrimEnd('/')}/user/products/{recipient.MappingId}";

                var text =
                    baseText +
                    $"\n\n🔗 <a href=\"{EscapeHtml(detailsUrl)}\">مشاهده جزئیات و تاریخچه در سایت</a>";

                try
                {
                    var result = await _telegramService.SendTelegramMessage(recipient.ChatId, text);
                    if (result.IsFailure)
                        _logger.LogWarning(
                            "Telegram send failed for user {UserId} (chat {ChatId})",
                            recipient.UserId, recipient.ChatId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Telegram send threw for user {UserId} (chat {ChatId})",
                        recipient.UserId, recipient.ChatId);
                }
            }
        }

        #region Message Composition

        private static string ComposeAlertMessage(ProductVariantMarketStatusUpdatedEvent n)
        {
            var lines = new List<string>
            {
                "<b>🔔 تغییر در محصول تحت نظر شما</b>",
                $"<a href=\"{EscapeHtml(n.ProductUrl)}\">{EscapeHtml(n.ProductTitle)}</a>",
            };

            if (!string.IsNullOrWhiteSpace(n.VariantName) && n.VariantName != "نامشخص")
                lines.Add($"🎨 رنگ: {EscapeHtml(n.VariantName)}");

            // ---- Changes detected this cycle ----
            lines.Add(string.Empty);
            lines.Add("<b>📋 تغییرات:</b>");

            foreach (var change in n.Changes)
                lines.Add(DescribeChange(change));

            // ---- Current market report ----
            lines.Add(string.Empty);
            lines.Add("<b>📊 وضعیت فعلی بازار:</b>");

            var rankedSellers = n.Sellers
                .Where(s => s.CurrentPrice > 0)
                .OrderBy(s => s.CurrentPrice)
                .ToList();

            if (rankedSellers.Count == 0)
            {
                lines.Add("❌ هیچ فروشنده فعالی در این رنگ وجود ندارد.");
            }
            else
            {
                foreach (var (seller, index) in rankedSellers.Take(5).Select((s, i) => (s, i)))
                {
                    var medal = index switch
                    {
                        0 => "🥇",
                        1 => "🥈",
                        2 => "🥉",
                        _ => "▫️"
                    };

                    var crown = seller.IsBuyBoxWinner ? " 👑" : string.Empty;

                    lines.Add(
                        $"{medal} {EscapeHtml(seller.SellerName)} — " +
                        $"{PriceFormatHelper.FormatPrice(seller.CurrentPrice)}{crown}");
                }

                if (rankedSellers.Count > 5)
                    lines.Add($"… و {(rankedSellers.Count - 5):N0} فروشنده دیگر");

                lines.Add($"🏪 مجموع فروشندگان فعال: {n.Sellers.Count:N0}");
            }

            lines.Add(n.IsAvailable ? "📦 وضعیت: موجود" : "📦 وضعیت: ناموجود");

            return string.Join('\n', lines);
        }

        private static string DescribeChange(VariantChangeItemInfo item) => item.Type switch
        {
            ChangeItemType.PriceChanged => PriceChangeLine(item),
            ChangeItemType.BuyBoxGained => $"🏆 <b>{EscapeHtml(item.SellerName)}</b> برنده بای‌باکس شد",
            ChangeItemType.BuyBoxLost => $"❌ <b>{EscapeHtml(item.SellerName)}</b> بای‌باکس را از دست داد",
            ChangeItemType.SellerAdded => $"➕ فروشنده جدید: <b>{EscapeHtml(item.SellerName)}</b>" +
                (item.NewPrice > 0 ? $" با قیمت {PriceFormatHelper.FormatPrice(item.NewPrice)}" : string.Empty),
            ChangeItemType.SellerRemoved => $"➖ <b>{EscapeHtml(item.SellerName)}</b> ناموجود شد یا از لیست فروشندگان حذف شد",
            ChangeItemType.SellerBackInStock => $"✅ <b>{EscapeHtml(item.SellerName)}</b> مجدداً موجود شد",
            ChangeItemType.StockOut => "📦 محصول در این رنگ کاملاً ناموجود شد",
            _ => $"تغییر: {EscapeHtml(item.SellerName)}"
        };

        private static string PriceChangeLine(VariantChangeItemInfo item)
        {
            var directionIcon = item.NewPrice > item.OldPrice ? "📈" : "📉";
            var percentPart = item.ChangePercent.HasValue
                ? $" ({item.ChangePercent.Value:+0.0;-0.0}%)"
                : string.Empty;

            return $"{directionIcon} <b>{EscapeHtml(item.SellerName)}</b>: " +
                   $"{PriceFormatHelper.FormatPrice(item.OldPrice)} ← {PriceFormatHelper.FormatPrice(item.NewPrice)}{percentPart}";
        }

        private static string EscapeHtml(string? value) =>
            value?
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;") ?? string.Empty;

        #endregion
    }
}
