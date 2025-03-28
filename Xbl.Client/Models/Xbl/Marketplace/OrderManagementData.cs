﻿using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class OrderManagementData
{
    public object[] GrantedEntitlementKeys { get; set; }
    public PiFilter PIFilter { get; set; }
    public Price Price { get; set; }
    public string OrderManagementPolicyIdOverride { get; set; }
    public string GeofencingPolicyId { get; set; }
}