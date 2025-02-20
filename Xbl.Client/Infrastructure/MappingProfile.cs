using AutoMapper;
using Xbl.Client.Models;

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

        CreateMap<Achievement, KqlAchievement>()
            .ForMember(d => d.IsUnlocked, o => o.MapFrom(s => s.Unlocked))
            .ForMember(d => d.IsRare, o => o.MapFrom(s => s.Rarity.CurrentCategory == "Rare"))
            .ForMember(d => d.RarityPercentage, o => o.MapFrom(s => s.Rarity.CurrentPercentage));

        CreateMap<Stat, KqlMinutesPlayed>()
            .ForMember(d => d.Minutes, o => o.MapFrom(s => s.IntValue));

        CreateMap<Title, KqlTitle>()
            .ForMember(d => d.Devices, o => o.MapFrom(s => string.Join('|', s.Devices)))
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
                return "XboxSeries";
            case "Durango":
                return "XboxOne";
            default:
                return x;
        }
    }

    private static int MapGamerscore(IEnumerable<Reward> rewards)
    {
        return rewards.SingleOrDefault(r => r.Type == nameof(KqlAchievement.Gamerscore))?.IntValue ?? 0;
    }
}