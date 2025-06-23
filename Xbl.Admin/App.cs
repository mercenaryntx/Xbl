using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Xbl.Admin.Io;
using Xbl.Client;
using Xbl.Client.Extensions;
using Xbl.Client.Models.Dbox;
using Xbl.Client.Models.Xbl.Achievements;
using Xbl.Data;

namespace Xbl.Admin;

public sealed class App : AsyncCommand<Settings>
{
    private readonly IDboxClient _dbox;
    private readonly IXblClient _xbl;
    private readonly IDatabaseContext _dboxDb;
    private readonly IDatabaseContext _liveDb;

    public App(IDboxClient dbox, IXblClient xbl, [FromKeyedServices(DataSource.Dbox)] IDatabaseContext dboxDb, [FromKeyedServices(DataSource.Live)] IDatabaseContext liveDb)
    {
        _dbox = dbox;
        _xbl = xbl;
        //_dboxDb = dboxDb.Mandatory();
        _liveDb = liveDb.Mandatory(SqliteOpenMode.ReadWrite);
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (!Directory.Exists(DataSource.DataFolder)) Directory.CreateDirectory(DataSource.DataFolder);

        //var tr = await _liveDb.GetRepository<Title>();
        //var titles = await tr.GetAll();

        var ar = await _liveDb.GetRepository<Achievement>();
        var ag = await ar.GetAll();
        var achievements = ag.GroupBy(a => a.TitleId).ToDictionary(g => g.Key, g => g.ToArray());

        foreach (var (key, value) in achievements)
        {
            foreach (var g in value.GroupBy(v => v.Name).Where(g => g.Count() > 1))
            {
                //Console.WriteLine(key);
                Console.WriteLine($"[{value.First().TitleName}] {g.Key} {string.Join(", ", g.Select(z => z.Id))} {string.Join(", ", g.Select(z => z.OriginalId))}");
            }
        }



        //foreach (var (key, value) in achievements)
        //{
        //    //Console.Write(value.First().TitleName + " ");
        //    var hexId = key.ToString().ToHexId();
        //    var path = $@"live\{hexId}\achievements.json";
        //    if (!File.Exists(path))
        //    {
        //        Console.WriteLine($"{key} MISSING");
        //        continue;
        //    }
        //    var json = await File.ReadAllTextAsync(path);
        //    try
        //    {
        //        var data = JsonSerializer.Deserialize<TitleDetails<LiveAchievement>>(json).Achievements
        //            .ToDictionary(a => a.Name);
        //        foreach (var achievement in value)
        //        {
        //            if (data.TryGetValue(achievement.Name, out var la))
        //            {
        //                achievement.DisplayImage = la.MediaAssets.First().Url;
        //                achievement.OriginalId = la.Id;
        //            }
        //            else
        //            {
        //                //Console.Write(achievement.Name + " ");
        //            }
        //        }

        //        //Console.WriteLine("OK");
        //    }
        //    catch
        //    {
        //        Console.WriteLine($"{key} FAIL");
        //    }
        //}

        //await ar.BulkUpdate(achievements.Values.SelectMany(a => a));

        //-------*-------//

        //var newProducts = (await _dbox.GetDelta()).ToArray();

        //var xblProducts = await _xbl.GetGameDetails(newProducts.SelectMany(p => p.Versions.Select(v => v.Value.ProductId)));

        //var repo = await _dboxDb.GetRepository<Product>(DataTable.Store);
        //foreach (var product in newProducts)
        //{
        //    foreach (var (_, version) in product.Versions)
        //    {
        //        if (xblProducts.TryGetValue(version.ProductId, out var xblProduct))
        //        {
        //            version.ReleaseDate = xblProduct.MarketProperties[0].OriginalReleaseDate;
        //            if (xblProduct.MarketProperties.Length > 1) Debugger.Break();
        //            product.Category = xblProduct.Properties.Category;
        //        }
        //        else
        //        {
        //            Console.WriteLine($"Missing XBL product: {version.ProductId} {product.Title}");
        //        }
        //    }
        //}

        //await repo.BulkInsert(newProducts);
        return 0;
    }
}