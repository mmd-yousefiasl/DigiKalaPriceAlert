using Application.Dtos.DkMarketplace.Main;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IDkMarketplaceApiService
    {
        // Products
        Task<Result<SellerProductsData>> GetSellerProductsAsync(SellerProductsQuery query, CancellationToken ct = default);

        // Variants
        Task<Result<VariantListData>> GetVariantsAsync(VariantsQuery query, CancellationToken ct = default);
        Task<Result<VariantItem>> GetVariantAsync(int variantId, CancellationToken ct = default);
        Task<Result<VariantItem>> UpdateVariantAsync(int variantId, UpdateVariantRequest request, CancellationToken ct = default);
        Task<Result<VariantStockData>> GetVariantStockAsync(int variantId, CancellationToken ct = default);
        Task<Result<UpdateVariantStockData>> UpdateVariantStockAsync(int variantId, UpdateVariantStockRequest request, CancellationToken ct = default);
        Task<Result<UpdateSellingPriceData>> UpdateSellingPriceAsync(UpdateSellingPriceRequest request, CancellationToken ct = default);

        // Pricing
        Task<Result<WinningPriceData>> GetWinningPriceAsync(int productVariantId, CancellationToken ct = default);
        Task<Result<SellingPriceBoundaryData>> GetSellingPriceBoundaryAsync(int variantId, int? promotionId = null, CancellationToken ct = default);
        Task<Result<VariantCurrentPromotionsData>> GetVariantCurrentPromotionsAsync(int productVariantId, VariantPromotionsQuery query, CancellationToken ct = default);

        // Batch
        Task<Result<BatchVariantUpdateData>> BatchUpdateVariantsAsync(BatchVariantUpdateRequest request, CancellationToken ct = default);
        Task<Result<BatchInquiryData>> BatchInquiryAsync(BatchInquiryRequest request, CancellationToken ct = default);

        // Commission
        Task<Result<CommissionListData>> GetCommissionsAsync(CommissionQuery query, CancellationToken ct = default);

        // Profile
        Task<Result<ProfileData>> GetProfileAsync(CancellationToken ct = default);
    }
}
