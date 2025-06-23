using AutoMapper;
using Xbl.Client.Models.Xbl.Achievements;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Achievement, Xbl.Web.Models.Achievement>()
            .ForMember(d => d.IsUnlocked, o => o.MapFrom(s => s.Unlocked))
            .ForMember(d => d.IsRare, o => o.MapFrom(s => s.Rarity.CurrentCategory == "Rare"))
            .ForMember(d => d.RarityPercentage, o => o.MapFrom(s => s.Rarity.CurrentPercentage));
    }
}