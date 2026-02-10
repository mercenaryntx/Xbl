using Kusto.Language.Parsing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using System.Diagnostics;
using System.Text.Json;
using AutoMapper;
using Xbl.Admin.Io;
using Xbl.Client;
using Xbl.Client.Extensions;
using Xbl.Client.Models.Dbox;
using Xbl.Client.Models.Xbl.Achievements;
using Xbl.Client.Models.Xbl.Marketplace;
using Xbl.Data;

namespace Xbl.Admin;

public sealed class App : AsyncCommand<Settings>
{
    private readonly IDboxClient _dbox;
    private readonly IXblClient _xbl;
    private readonly IMapper _mapper;
    private readonly IDatabaseContext _dboxDb;
    private readonly IDatabaseContext _liveDb;

    //[FromKeyedServices(DataSource.Dbox)] IDatabaseContext dboxDb, 
    public App(IDboxClient dbox, IXblClient xbl, IMapper mapper, [FromKeyedServices(DataSource.Live)] IDatabaseContext liveDb)
    {
        _dbox = dbox;
        _xbl = xbl;
        _mapper = mapper;
        //_dboxDb = dboxDb.Mandatory();
        _liveDb = liveDb.Mandatory(SqliteOpenMode.ReadWrite);
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (!Directory.Exists(DataSource.DataFolder)) Directory.CreateDirectory(DataSource.DataFolder);

        //var tr = await _liveDb.GetRepository<Title>();
        //var titles = await tr.GetAll();

        var tr = await _liveDb.GetRepository<Title>();
        var titles = await tr.GetAll();
        titles = titles.Where(t => t.Products == null || t.Products.Count == 0).ToArray();

        foreach (var title in titles.Where(t => !t.CompatibleDevices.Contains("Mobile")))
        {
            Console.Write($"Updating {title.Name}...");
            var products = await _xbl.GetGameDetailsByTitleId(title.IntId);
            EnrichTitle(title, products);
            await tr.Update(title);
            Console.WriteLine("OK");
        }
        

        //var ar = await _liveDb.GetRepository<Achievement>();
        //var ag = await ar.GetAll();
        //var achievements = ag.GroupBy(a => a.TitleId).ToDictionary(g => g.Key, g => g.ToArray());

        //foreach (var (key, value) in achievements)
        //{
        //    foreach (var g in value.GroupBy(v => v.Name).Where(g => g.Count() > 1))
        //    {
        //        //Console.WriteLine(key);
        //        Console.WriteLine($"[{value.First().TitleName}] {g.Key} {string.Join(", ", g.Select(z => z.Id))} {string.Join(", ", g.Select(z => z.OriginalId))}");
        //    }
        //}



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

        //var xblProducts = await _xbl.GetGameDetailsByProductId(newProducts.SelectMany(p => p.Versions.Select(v => v.Value.ProductId)));

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

    private Title EnrichTitle(Title title, Dictionary<string, XblProduct> store)
    {
        title.Source = DataSource.Live;
        title.HexId = title.IntId.ToHexId();

        var d = new Dictionary<string, XblProductVersion>();
        if (store.Count > 1) Debugger.Break();

        foreach (var sp in store.Values.OrderBy(x => x.Properties.XboxConsoleGenCompatible?.Length))
        {
            var v = new XblProductVersion
            {
                Title = sp.LocalizedProperties.First().ProductTitle,
                ProductId = sp.ProductId,
                ReleaseDate = sp.MarketProperties.Length > 0 ? sp.MarketProperties[0].OriginalReleaseDate : null,
                PackageIdentityName = sp.Properties.PackageIdentityName,
                XboxConsoleGenOptimized = sp.Properties.XboxConsoleGenOptimized,
                XboxConsoleGenCompatible = sp.Properties.XboxConsoleGenCompatible,
                RevisionId = sp.Properties.RevisionId,
                OriginalReleaseDate = sp.MarketProperties.First().OriginalReleaseDate
            };
            if (sp.Properties.XboxConsoleGenCompatible == null)
            {
                if (d.ContainsKey(Device.PC) && sp.Properties.RevisionId < d[Device.PC].RevisionId) continue;
                d[Device.PC] = v;
                continue;
            }

            if (sp.Properties.PackageIdentityName.StartsWith("Xbox360BackwardCompatibil."))
            {
                d.Add(Device.Xbox360, v);
                continue;
            }
            foreach (var gen in sp.Properties.XboxConsoleGenCompatible)
            {
                if (gen == "ConsoleGen8")
                {
                    if (d.ContainsKey(Device.XboxOne) && sp.Properties.RevisionId < d[Device.XboxOne].RevisionId) continue;
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

        title.Category = store.Values.First().Properties.Category;
        title.Products = d.ToDictionary(kvp => kvp.Key, kvp => new TitleProduct
        {
            TitleId = title.HexId,
            ProductId = kvp.Value.ProductId,
            ReleaseDate = kvp.Value.OriginalReleaseDate
        });
        title.OriginalConsole = title.Products.Keys.OrderBy(k => k).FirstOrDefault(k => k.StartsWith("Xbox"));
        return title;
    }
}