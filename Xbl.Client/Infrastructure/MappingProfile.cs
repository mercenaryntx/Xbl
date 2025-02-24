﻿using System.Diagnostics;
using AutoMapper;
using Xbl.Client.Models.Dbox;
using Xbl.Client.Models.Kql;
using Xbl.Client.Models.Xbl;

namespace Xbl.Client.Infrastructure;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<LiveAchievement, Achievement>()
            .ForMember(d => d.TitleId, o => o.MapFrom(s => long.Parse(s.Id)))
            .ForMember(d => d.TitleName, o => o.MapFrom(s => s.TitleAssociations[0].Name))
            .ForMember(d => d.TimeUnlocked, o => o.MapFrom(s => s.Progression.TimeUnlocked))
            .ForMember(d => d.Unlocked, o => o.MapFrom(s => s.ProgressState == "Achieved"))
            .ForMember(d => d.UnlockedOnline, o => o.MapFrom(s => s.ProgressState == "Achieved"))
            .ForMember(d => d.Platform, o => o.MapFrom(s => MapPlatform(s.Platforms)))
            .ForMember(d => d.Gamerscore, o => o.MapFrom(s => MapGamerscore(s.Rewards)));

        CreateMap<IGrouping<string, StoreProduct>, Product>()
            .ForMember(d => d.Title, o => o.MapFrom(s => s.First().ProductGroupName ?? s.First().Title))
            .ForMember(d => d.TitleId, o => o.MapFrom(s => s.First().TitleId))
            .ForMember(d => d.DeveloperName, o => o.MapFrom(s => s.First().DeveloperName))
            .ForMember(d => d.PublisherName, o => o.MapFrom(s => s.First().PublisherName))
            .ForMember(d => d.ProductGroupId, o => o.MapFrom(s => s.First().ProductGroupId))
            .ForMember(d => d.Category, o => o.MapFrom(s => s.First().Category))
            .ForMember(d => d.Versions, o => o.MapFrom((s, _, _, c) => MapStoreProductVersions(s, c)));

        CreateMap<StoreProduct, ProductVersion>();

        CreateMap<MarketplaceProduct, Product>()
            .ForMember(d => d.Title, o => o.MapFrom(s => s.DefaultTitle))
            .ForMember(d => d.Versions, o => o.Ignore())
            .ForMember(d => d.Category, o => o.Ignore());

        CreateMap<MarketplaceProduct, ProductVersion>()
            .ForMember(d => d.Title, o => o.MapFrom(s => s.DefaultTitle))
            .ForMember(d => d.ReleaseDate, o => o.MapFrom(s => s.GlobalOriginalReleaseDate))
            .ForMember(d => d.XboxConsoleGenCompatible, o => o.MapFrom(s => new [] { Device.Xbox360 }));

        CreateMap<Achievement, KqlAchievement>()
            .ForMember(d => d.IsUnlocked, o => o.MapFrom(s => s.Unlocked))
            .ForMember(d => d.IsRare, o => o.MapFrom(s => s.Rarity.CurrentCategory == "Rare"))
            .ForMember(d => d.RarityPercentage, o => o.MapFrom(s => s.Rarity.CurrentPercentage));

        CreateMap<Stat, KqlMinutesPlayed>()
            .ForMember(d => d.Minutes, o => o.MapFrom(s => s.IntValue));

        CreateMap<Title, KqlTitle>()
            .ForMember(d => d.CompatibleWith, o => o.MapFrom(s => string.Join('|', s.CompatibleDevices)))
            .ForMember(d => d.CurrentAchievements, o => o.MapFrom(s => s.Achievement.CurrentAchievements))
            .ForMember(d => d.TotalAchievements, o => o.MapFrom(s => s.Achievement.TotalAchievements))
            .ForMember(d => d.CurrentGamerscore, o => o.MapFrom(s => s.Achievement.CurrentGamerscore))
            .ForMember(d => d.TotalGamerscore, o => o.MapFrom(s => s.Achievement.TotalGamerscore))
            .ForMember(d => d.ProgressPercentage, o => o.MapFrom(s => s.Achievement.ProgressPercentage))
            .ForMember(d => d.LastTimePlayed, o => o.MapFrom(s => s.TitleHistory.LastTimePlayed));
    }

    private static string MapPlatform(IEnumerable<string> platforms)
    {
        var x = platforms.First();
        switch (x)
        {
            case "WindowsOneCore":
            case "Scarlett":
                return Device.XboxSeries;
            case "Durango":
                return Device.XboxOne;
            default:
                Debugger.Break();
                return x;
        }
    }

    private static int MapGamerscore(IEnumerable<Reward> rewards)
    {
        return rewards.SingleOrDefault(r => r.Type == nameof(KqlAchievement.Gamerscore))?.IntValue ?? 0;
    }

    private static Dictionary<string, ProductVersion> MapStoreProductVersions(IEnumerable<StoreProduct> source, ResolutionContext context)
    {
        var d = new Dictionary<string, ProductVersion>();
        foreach (var sp in source.OrderBy(x => x.XboxConsoleGenCompatible?.Length))
        {
            var v = context.Mapper.Map<ProductVersion>(sp);
            if (sp.XboxConsoleGenCompatible == null)
            {
                if (d.ContainsKey(Device.PC) && sp.RevisionId < d[Device.PC].RevisionId) continue;
                d[Device.PC] = v;
                continue;
            }

            if (sp.PackageIdentityName.StartsWith("Xbox360BackwardCompatibil."))
            {
                d.Add(Device.Xbox360, v);
                continue;
            }
            foreach (var gen in sp.XboxConsoleGenCompatible)
            {
                if (gen == "ConsoleGen8")
                {
                    if (d.ContainsKey(Device.XboxOne) && sp.RevisionId < d[Device.XboxOne].RevisionId) continue;
                    d[Device.XboxOne] = v;
                    break;
                }

                if (gen == "ConsoleGen9")
                {
                    d.Add(Device.XboxSeries, v);
                    break;
                }

                throw new Exception($"Unknown generation: {gen}");
            }
        }

        return d;
    }
}