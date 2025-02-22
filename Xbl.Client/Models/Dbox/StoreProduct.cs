using System.Text.Json.Serialization;

namespace Xbl.Client.Models.Dbox
{
    public class StoreProduct
    {
        [JsonPropertyName("neutral_title")] 
        public string Title { get; set; }
        [JsonPropertyName("product_id")] 
        public string ProductId { get; set; }
        [JsonPropertyName("title_id")] 
        public string TitleId { get; set; }
        [JsonPropertyName("legacy_windows_store_product_id")]
        public string LegacyWindowsStoreProductId { get; set; }
        [JsonPropertyName("legacy_windows_store_parent_product_id")]
        public string LegacyWindowsStoreParentProductId { get; set; }
        [JsonPropertyName("legacy_windows_phone_product_id")]
        public string LegacyWindowsPhoneProductId { get; set; }
        [JsonPropertyName("legacy_windows_phone_parent_product_id")]
        public string LegacyWindowsPhoneParentProductId { get; set; }
        [JsonPropertyName("legacy_xbox_product_id")]
        public string LegacyXboxProductId { get; set; }
        [JsonPropertyName("in_app_offer_token")]
        public string InAppOfferToken { get; set; }
        [JsonPropertyName("avatar_asset_id")] 
        public string AvatarAssetId { get; set; }
        [JsonPropertyName("product_kind")] 
        public string ProductKind { get; set; }
        [JsonPropertyName("developer_name")] 
        public string DeveloperName { get; set; }
        [JsonPropertyName("publisher_name")] 
        public string PublisherName { get; set; }
        [JsonPropertyName("product_group_id")] 
        public string ProductGroupId { get; set; }
        [JsonPropertyName("product_group_name")]
        public string ProductGroupName { get; set; }
        [JsonPropertyName("package_family_name")]
        public string PackageFamilyName { get; set; }
        [JsonPropertyName("package_identity_name")]
        public string PackageIdentityName { get; set; }
        [JsonPropertyName("xbox_console_gen_optimized")]
        public string[] XboxConsoleGenOptimized { get; set; }
        [JsonPropertyName("xbox_console_gen_compatible")]
        public string[] XboxConsoleGenCompatible { get; set; }
        [JsonPropertyName("category")] 
        public string Category { get; set; }
        [JsonPropertyName("is_demo")] 
        public bool? IsDemo { get; set; }
        [JsonPropertyName("xbox_live_gold_required")]
        public bool? XboxLiveGoldRequired { get; set; }
        [JsonPropertyName("product_type")] 
        public string ProductType { get; set; }
        [JsonPropertyName("product_family")] 
        public string ProductFamily { get; set; }
        [JsonPropertyName("preferred_sku_id")] 
        public string PreferredSkuId { get; set; }
        [JsonPropertyName("revision_id")] 
        public DateTime? RevisionId { get; set; }
    }

}
