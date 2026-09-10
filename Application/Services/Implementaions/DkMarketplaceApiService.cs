using Application.Dtos.DkMarketplace.Common;
using Application.Dtos.DkMarketplace.Main;
using Application.Services.Interfaces;
using Domain.Shared;
using Domain.Statics;
using Microsoft.Extensions.Logging;

namespace Application.Services.Implementaions
{
    public sealed class DkMarketplaceApiService : DkApiServiceBase, IDkMarketplaceApiService
    {
        #region Ctor
        public DkMarketplaceApiService(HttpClient httpClient, ILogger<DkMarketplaceApiService> logger)
            : base(httpClient, logger) { }
        #endregion

        #region Products
        public Task<Result<SellerProductsData>> GetSellerProductsAsync(SellerProductsQuery q, CancellationToken ct = default)
            => GetAsync<SellerProductsData>($"{SiteTools.DkApiServiceBase}/products/seller{BuildSellerProductsQuery(q)}", ct);
        #endregion

        #region Variants
        public Task<Result<VariantListData>> GetVariantsAsync(VariantsQuery q, CancellationToken ct = default)
            => GetAsync<VariantListData>($"{SiteTools.DkApiServiceBase}/variants{BuildVariantsQuery(q)}", ct);

        public Task<Result<VariantItem>> GetVariantAsync(int variantId, CancellationToken ct = default)
            => GetAsync<VariantItem>($"{SiteTools.DkApiServiceBase}/variants/{variantId}", ct);

        public Task<Result<VariantItem>> UpdateVariantAsync(int variantId, UpdateVariantRequest request, CancellationToken ct = default)
            => PutAsync<VariantItem>($"{SiteTools.DkApiServiceBase}/variants/{variantId}", request, ct);

        public Task<Result<VariantStockData>> GetVariantStockAsync(int variantId, CancellationToken ct = default)
            => GetAsync<VariantStockData>($"{SiteTools.DkApiServiceBase}/variants/{variantId}/seller-stock", ct);

        public Task<Result<UpdateVariantStockData>> UpdateVariantStockAsync(int variantId, UpdateVariantStockRequest request, CancellationToken ct = default)
            => PatchAsync<UpdateVariantStockData>($"{SiteTools.DkApiServiceBase}/variants/{variantId}/seller-stock", request, ct);

        public Task<Result<UpdateSellingPriceData>> UpdateSellingPriceAsync(UpdateSellingPriceRequest request, CancellationToken ct = default)
            => PatchAsync<UpdateSellingPriceData>($"{SiteTools.DkApiServiceBase}/variants/selling-price", request, ct);
        #endregion

        #region Pricing
        public Task<Result<WinningPriceData>> GetWinningPriceAsync(int productVariantId, CancellationToken ct = default)
            => GetAsync<WinningPriceData>(
                $"{SiteTools.DkApiServiceBase}/pricing/buybox/price-suggestion/winning-price?product_variant_id={productVariantId}", ct);

        public Task<Result<SellingPriceBoundaryData>> GetSellingPriceBoundaryAsync(int variantId, int? promotionId = null, CancellationToken ct = default)
        {
            var url = $"{SiteTools.DkApiServiceBase}/pricing/price-stats/{variantId}/boundary";
            if (promotionId.HasValue) url += $"?promotion_id={promotionId.Value}";
            return GetAsync<SellingPriceBoundaryData>(url, ct);
        }

        public Task<Result<VariantCurrentPromotionsData>> GetVariantCurrentPromotionsAsync(
            int productVariantId, VariantPromotionsQuery q, CancellationToken ct = default)
        {
            var p = new List<string>();
            if (q.Page.HasValue) p.Add($"page={q.Page.Value}");
            if (q.Size.HasValue) p.Add($"size={q.Size.Value}");
            if (!string.IsNullOrWhiteSpace(q.Sort)) p.Add($"sort={Uri.EscapeDataString(q.Sort!)}");
            if (!string.IsNullOrWhiteSpace(q.Order)) p.Add($"order={Uri.EscapeDataString(q.Order!)}");
            var qs = p.Count == 0 ? string.Empty : "?" + string.Join("&", p);

            return GetAsync<VariantCurrentPromotionsData>(
                $"{SiteTools.DkApiServiceBase}/pricing/promotions/variant-current-promotions/{productVariantId}{qs}", ct);
        }
        #endregion

        #region Batch
        public Task<Result<BatchVariantUpdateData>> BatchUpdateVariantsAsync(BatchVariantUpdateRequest request, CancellationToken ct = default)
            => PostAsync<BatchVariantUpdateData>($"{SiteTools.DkApiServiceBase}/batch/variant/update", request, ct);

        public Task<Result<BatchInquiryData>> BatchInquiryAsync(BatchInquiryRequest request, CancellationToken ct = default)
            => PostAsync<BatchInquiryData>($"{SiteTools.DkApiServiceBase}/batch/inquiry", request, ct);
        #endregion

        #region Commission
        public Task<Result<CommissionListData>> GetCommissionsAsync(CommissionQuery q, CancellationToken ct = default)
            => GetAsync<CommissionListData>($"{SiteTools.DkApiServiceBase}/commissions/{BuildCommissionQuery(q)}", ct);
        #endregion

        #region Profile
        public Task<Result<ProfileData>> GetProfileAsync(CancellationToken ct = default)
            => GetAsync<ProfileData>($"{SiteTools.DkApiServiceBase}/profile", ct);
        #endregion

        #region Query builders
        private static string BuildSellerProductsQuery(SellerProductsQuery q)
        {
            var p = new List<string>();
            if (q.Page.HasValue) p.Add($"page={q.Page.Value}");
            if (q.Size.HasValue) p.Add($"size={q.Size.Value}");
            if (!string.IsNullOrWhiteSpace(q.Sort)) p.Add($"sort={Uri.EscapeDataString(q.Sort!)}");
            if (!string.IsNullOrWhiteSpace(q.Order)) p.Add($"order={Uri.EscapeDataString(q.Order!)}");
            if (!string.IsNullOrWhiteSpace(q.MultiSearch)) p.Add($"search[multi_search]={Uri.EscapeDataString(q.MultiSearch!)}");
            if (!string.IsNullOrWhiteSpace(q.CategoryId)) p.Add($"search[category_id]={Uri.EscapeDataString(q.CategoryId!)}");
            if (!string.IsNullOrWhiteSpace(q.BrandId)) p.Add($"search[brand_id]={Uri.EscapeDataString(q.BrandId!)}");
            if (!string.IsNullOrWhiteSpace(q.ModerationStatus)) p.Add($"search[moderation_status]={Uri.EscapeDataString(q.ModerationStatus!)}");
            if (!string.IsNullOrWhiteSpace(q.FakeStatus)) p.Add($"search[fake_status]={Uri.EscapeDataString(q.FakeStatus!)}");
            return p.Count == 0 ? string.Empty : "?" + string.Join("&", p);
        }

        private static string BuildVariantsQuery(VariantsQuery q)
        {
            var p = new List<string>();
            if (q.Page.HasValue) p.Add($"page={q.Page.Value}");
            if (q.Size.HasValue) p.Add($"size={q.Size.Value}");
            if (!string.IsNullOrWhiteSpace(q.Sort)) p.Add($"sort={Uri.EscapeDataString(q.Sort!)}");
            if (!string.IsNullOrWhiteSpace(q.Order)) p.Add($"order={Uri.EscapeDataString(q.Order!)}");
            if (q.Id.HasValue) p.Add($"search[id]={q.Id.Value}");
            if (!string.IsNullOrWhiteSpace(q.Ids)) p.Add($"search[ids]={Uri.EscapeDataString(q.Ids!)}");
            if (!string.IsNullOrWhiteSpace(q.ShippingType)) p.Add($"search[shipping_type]={Uri.EscapeDataString(q.ShippingType!)}");
            if (q.Active.HasValue) p.Add($"search[active]={(q.Active.Value ? "true" : "false")}");
            if (!string.IsNullOrWhiteSpace(q.ModerationStatus)) p.Add($"search[moderation_status]={Uri.EscapeDataString(q.ModerationStatus!)}");
            if (!string.IsNullOrWhiteSpace(q.CategoryIds)) p.Add($"search[category_ids]={Uri.EscapeDataString(q.CategoryIds!)}");
            if (!string.IsNullOrWhiteSpace(q.BuyBoxWinner)) p.Add($"search[buy_box_winner]={Uri.EscapeDataString(q.BuyBoxWinner!)}");
            if (q.InCompetition.HasValue) p.Add($"search[in_competition]={(q.InCompetition.Value ? "true" : "false")}");
            if (!string.IsNullOrWhiteSpace(q.SearchTerm)) p.Add($"search[search_term]={Uri.EscapeDataString(q.SearchTerm!)}");
            if (!string.IsNullOrWhiteSpace(q.PriceTerms)) p.Add($"search[price_terms]={Uri.EscapeDataString(q.PriceTerms!)}");
            if (q.OutOfStock.HasValue) p.Add($"search[out_of_stock]={(q.OutOfStock.Value ? "true" : "false")}");
            if (q.Archived.HasValue) p.Add($"search[archived]={(q.Archived.Value ? "true" : "false")}");
            if (!string.IsNullOrWhiteSpace(q.SellingChannel)) p.Add($"search[selling_channel]={Uri.EscapeDataString(q.SellingChannel!)}");
            if (!string.IsNullOrWhiteSpace(q.CreationTimeFrom)) p.Add($"search[creation_time_from]={Uri.EscapeDataString(q.CreationTimeFrom!)}");
            if (!string.IsNullOrWhiteSpace(q.CreationTimeTo)) p.Add($"search[creation_time_to]={Uri.EscapeDataString(q.CreationTimeTo!)}");
            if (!string.IsNullOrWhiteSpace(q.SellerProductTags)) p.Add($"search[seller_product_tags]={Uri.EscapeDataString(q.SellerProductTags!)}");
            if (q.NearbySellerShipment.HasValue) p.Add($"search[nearby_seller_shipment]={(q.NearbySellerShipment.Value ? "true" : "false")}");
            return p.Count == 0 ? string.Empty : "?" + string.Join("&", p);
        }

        private static string BuildCommissionQuery(CommissionQuery q)
        {
            var p = new List<string>();
            if (q.Page.HasValue) p.Add($"page={q.Page.Value}");
            if (q.Size.HasValue) p.Add($"size={q.Size.Value}");
            if (!string.IsNullOrWhiteSpace(q.Sort)) p.Add($"sort={Uri.EscapeDataString(q.Sort!)}");
            if (!string.IsNullOrWhiteSpace(q.Order)) p.Add($"order={Uri.EscapeDataString(q.Order!)}");
            if (q.MyCommissions.HasValue) p.Add($"search[my_commissions]={(q.MyCommissions.Value ? "true" : "false")}");
            if (q.MainCategoryId.HasValue) p.Add($"search[main_category_id]={q.MainCategoryId.Value}");
            if (q.CategoryId.HasValue) p.Add($"search[category_id]={q.CategoryId.Value}");
            if (q.BrandId.HasValue) p.Add($"search[brand_id]={q.BrandId.Value}");
            return p.Count == 0 ? string.Empty : "?" + string.Join("&", p);
        }
        #endregion
    }
}
