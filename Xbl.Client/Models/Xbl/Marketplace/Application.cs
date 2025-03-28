﻿using System.Diagnostics.CodeAnalysis;

namespace Xbl.Client.Models.Xbl.Marketplace;

[ExcludeFromCodeCoverage]
public class Application
{
    public string ApplicationId { get; set; }
    public int DeclarationOrder { get; set; }
    public string[] Extensions { get; set; }
}