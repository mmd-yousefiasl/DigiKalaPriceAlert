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
    /// Sends one Bale market report per variant change event:
    /// resolves recipients for the COLOR GROUP, resets their unread flags,
    /// and composes a full market-status message.
    /// </summary>
    public class MarketAlertService : IMarketAlertService
    {
        #region Fields
        private const int TopSellerCount = 5;

        private readonly DigiKalaPriceAlertDbContext _context;
        private readonly IBaleService _baleService;
        private readonly ILogger<MarketAlertService> _logger;
        #endregion

        #region Ctor
        public MarketAlertService(
            DigiKalaPriceAlertDbContext context,
            IBaleService baleService,
            ILogger<MarketAlertService> logger)
        {
            _context = context;
            _baleService = baleService;
            _logger = logger;
        }
        #endregion

        public async Task DispatchMarketStatusAlertAsync(
            ProductVariantMarketStatusUpdatedEvent notification,
            CancellationToken cancellationToken = default)
        {
            // 1. Recipients: every tracker of THIS COLOR GROUP with a linked
            //    Bale account. (DigiKala's variant id is per seller combo,
            //    so mappings must be matched via their seller's ColorName.)
            //    The mapping Id is kept so each user gets a link to THEIR
            //    own product-details page.
            var recipients = await _context.UserProductMappings
                .AsNoTracking()
                .Where(m =>
                    m.VariantSeller.ProductId == notification.ProductId &&
                    m.VariantSeller.ColorName == notification.VariantName &&
                    m.User.BaleChatId != null &&
                    !m.User.IsUserBanned)
                .Select(m => new { m.UserId, MappingId = m.Id, ChatId = m.User.BaleChatId!.Value })
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
                    $"\n\n🔗 [مشاهده جزئیات و تاریخچه در سایت]({detailsUrl})";

                try
                {
                    var result = await _baleService.SendMessageAsync(
                        recipient.ChatId, text, ct: cancellationToken);

                    if (result.IsFailure)
                        _logger.LogWarning(
                            "Bale send failed for user {UserId} (chat {ChatId})",
                            recipient.UserId, recipient.ChatId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Bale send threw for user {UserId} (chat {ChatId})",
                        recipient.UserId, recipient.ChatId);
                }
            }
        }

        #region Message Composition

        private static string ComposeAlertMessage(ProductVariantMarketStatusUpdatedEvent n)
        {
            var lines = new List<string>
            {
                "🔔 *تغییر در محصول تحت نظر شما*",
                string.Empty,
                $"[{EscapeMarkdown(n.ProductTitle)}]({n.ProductUrl})"
            };

            if (!string.IsNullOrWhiteSpace(n.VariantName) && n.VariantName != "نامشخص")
                lines.Add($"🎨 رنگ: {EscapeMarkdown(n.VariantName)}");

            // ---- Changes detected this cycle ----
            lines.Add(string.Empty);
            lines.Add("📋 *تغییرات این دوره:*");

            foreach (var change in n.Changes)
                lines.Add(DescribeChange(change));

            // ---- Current market report ----
            lines.Add(string.Empty);
            lines.Add("📊 *وضعیت فعلی بازار:*");

            AppendMarketReport(lines, n);

            lines.Add(n.IsAvailable ? "📦 وضعیت: موجود" : "📦 وضعیت: ناموجود");

            return string.Join('\n', lines);
        }

        private static void AppendMarketReport(
            List<string> lines,
            ProductVariantMarketStatusUpdatedEvent n)
        {
            var rankedSellers = n.Sellers
                .Where(s => s.CurrentPrice > 0)
                .OrderBy(s => s.CurrentPrice)
                .ToList();

            if (rankedSellers.Count == 0)
            {
                lines.Add("❌ هیچ فروشنده فعالی در این رنگ وجود ندارد.");
                return;
            }

            foreach (var (seller, index) in rankedSellers
                         .Take(TopSellerCount)
                         .Select((s, i) => (s, i)))
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
                    $"{medal} {EscapeMarkdown(seller.SellerName)} — " +
                    $"{PriceFormatHelper.FormatPrice(seller.CurrentPrice)}{crown}");
            }

            if (rankedSellers.Count > TopSellerCount)
                lines.Add($"… و {(rankedSellers.Count - TopSellerCount):N0} فروشنده دیگر");

            lines.Add($"🏪 مجموع فروشندگان فعال: {n.Sellers.Count:N0}");
        }

        private static string DescribeChange(VariantChangeItemInfo item) => item.Type switch
        {
            ChangeItemType.PriceChanged =>
                PriceChangeLine(item),

            ChangeItemType.BuyBoxGained =>
                $"🏆 *{EscapeMarkdown(item.SellerName)}* برنده بای‌باکس شد",

            ChangeItemType.BuyBoxLost =>
                $"❌ *{EscapeMarkdown(item.SellerName)}* بای‌باکس را از دست داد",

            ChangeItemType.SellerAdded =>
                $"➕ فروشنده جدید: *{EscapeMarkdown(item.SellerName)}*" +
                (item.NewPrice > 0
                    ? $" با قیمت {PriceFormatHelper.FormatPrice(item.NewPrice)}"
                    : string.Empty),

            ChangeItemType.SellerRemoved =>
                $"➖ *{EscapeMarkdown(item.SellerName)}* ناموجود شد یا از لیست فروشندگان حذف شد",

            ChangeItemType.SellerBackInStock =>
                $"✅ *{EscapeMarkdown(item.SellerName)}* مجدداً موجود شد",

            ChangeItemType.StockOut =>
                "📦 محصول در این رنگ کاملاً ناموجود شد",

            _ =>
                $"تغییر: {EscapeMarkdown(item.SellerName)}"
        };

        private static string PriceChangeLine(VariantChangeItemInfo item)
        {
            var directionIcon = item.NewPrice > item.OldPrice ? "📈" : "📉";

            var percentPart = item.ChangePercent.HasValue
                ? $" ({item.ChangePercent.Value:+0.0;-0.0}%)"
                : string.Empty;

            return $"{directionIcon} *{EscapeMarkdown(item.SellerName)}*: " +
                   $"{PriceFormatHelper.FormatPrice(item.OldPrice)} ← " +
                   $"{PriceFormatHelper.FormatPrice(item.NewPrice)}{percentPart}";
        }

        // Escapes Markdown metacharacters so dynamic text (seller names,
        // product titles, …) can't break the surrounding formatting.
        // Mirrors BaleService.EscapeMarkdown — extract to a shared helper
        // if a third caller appears.
        private static string EscapeMarkdown(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var sb = new System.Text.StringBuilder(value.Length);

            foreach (var c in value)
            {
                if (c is '\\' or '*' or '_' or '`' or '[' or ']')
                    sb.Append('\\');

                sb.Append(c);
            }

            return sb.ToString();
        }

        #endregion
    }
}