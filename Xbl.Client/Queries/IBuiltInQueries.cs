namespace Xbl.Client.Queries;

public interface IBuiltInQueries
{
    Task Count();
    Task RarestAchievements();
    Task MostComplete();
    Task SpentMostTimeWith();
    Task WeightedRarity();
}