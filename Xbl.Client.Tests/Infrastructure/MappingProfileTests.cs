using AutoMapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Xbl.Client.Infrastructure;
using Xbl.Client.Models.Dbox;
using Xbl.Client.Models.Kql;
using Xbl.Client.Models.Xbl;
using Xbl.Client.Models.Xbl.Achievements;
using Xbl.Client.Models.Xbl.Player;
using Xunit;

namespace Xbl.Client.Tests.Infrastructure;

public class MappingProfileTests
{
    private IMapper _mapper;

    public MappingProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void MappingProfile_ConfigurationIsValid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        config.AssertConfigurationIsValid();
    }

    [Theory]
    [InlineData("WindowsOneCore", Device.XboxSeries)]
    [InlineData("Scarlett", Device.XboxSeries)]
    [InlineData("Durango", Device.XboxOne)]
    [InlineData(Device.XboxOne, Device.XboxOne)]
    public void Map_LiveAchievement_To_Achievement(string platform, string device)
    {
        var timeUnlocked = DateTime.Now;
        var liveAchievement = new LiveAchievement
        {
            Id = "1",
            TitleAssociations = new[] { new TitleAssociation { Id = 1915865634, Name = "Test Title" } },
            Progression = new Progression { TimeUnlocked = timeUnlocked },
            ProgressState = "Achieved",
            Platforms = new[] { platform },
            Rewards = new[] { new Reward { Type = "Gamerscore", Value = "100", ValueType = "Int" } }
        };

        var achievement = _mapper.Map<Achievement>(liveAchievement);

        using (new AssertionScope())
        {
            achievement.TitleId.Should().Be(1915865634);
            achievement.TitleName.Should().Be("Test Title");
            achievement.TimeUnlocked.Should().Be(timeUnlocked);
            achievement.Unlocked.Should().BeTrue();
            achievement.UnlockedOnline.Should().BeTrue();
            achievement.Platform.Should().Be(device);
            achievement.Gamerscore.Should().Be(100);
        }
    }

    [Fact]
    public void Map_StoreProductGroup_To_Product_PC()
    {
        var storeProducts = new[]
        {
            new StoreProduct
            {
                Title = "Test Title",
                TitleId = "123",
                DeveloperName = "Test Developer",
                PublisherName = "Test Publisher",
                ProductGroupId = "Group123",
                Category = "Test Category",
                ProductId = "abc",
                RevisionId = new DateTime(2024, 5, 5)
            }
        }.GroupBy(p => p.TitleId);

        var product = _mapper.Map<Product>(storeProducts.First());

        using (new AssertionScope())
        {
            product.Title.Should().Be("Test Title");
            product.TitleId.Should().Be("123");
            product.DeveloperName.Should().Be("Test Developer");
            product.PublisherName.Should().Be("Test Publisher");
            product.ProductGroupId.Should().Be("Group123");
            product.Category.Should().Be("Test Category");
            product.Versions.Should().BeEquivalentTo([ new KeyValuePair<string,ProductVersion>(Device.PC, new ProductVersion
            {
                Title = "Test Title", 
                ProductId = "abc", 
                RevisionId = new DateTime(2024, 5, 5), 
                XboxConsoleGenCompatible = [], 
                XboxConsoleGenOptimized = []
            })]);
        }
    }

    [Fact]
    public void Map_StoreProductGroup_To_Product_Xbox360()
    {
        var storeProducts = new[]
        {
            new StoreProduct
            {
                Title = "Test Title",
                TitleId = "123",
                DeveloperName = "Test Developer",
                PublisherName = "Test Publisher",
                ProductGroupId = "Group123",
                Category = "Test Category",
                ProductId = "abc",
                RevisionId = new DateTime(2024, 5, 5),
                XboxConsoleGenCompatible = ["ConsoleGen8"],
                PackageIdentityName = "Xbox360BackwardCompatibil.PrimaryBeautifulKatamari_ksqcvrsvwz2jp"
            }
        }.GroupBy(p => p.TitleId);

        var product = _mapper.Map<Product>(storeProducts.First());

        using (new AssertionScope())
        {
            product.Title.Should().Be("Test Title");
            product.TitleId.Should().Be("123");
            product.DeveloperName.Should().Be("Test Developer");
            product.PublisherName.Should().Be("Test Publisher");
            product.ProductGroupId.Should().Be("Group123");
            product.Category.Should().Be("Test Category");
            product.Versions.Should().BeEquivalentTo([ new KeyValuePair<string,ProductVersion>(Device.Xbox360, new ProductVersion
            {
                Title = "Test Title",
                ProductId = "abc",
                RevisionId = new DateTime(2024, 5, 5),
                XboxConsoleGenCompatible = ["ConsoleGen8"],
                XboxConsoleGenOptimized = [],
                PackageIdentityName = "Xbox360BackwardCompatibil.PrimaryBeautifulKatamari_ksqcvrsvwz2jp"
            })]);
        }
    }

    [Fact]
    public void Map_StoreProductGroup_To_Product_XboxOne()
    {
        var storeProducts = new[]
        {
            new StoreProduct
            {
                Title = "Test Title",
                TitleId = "123",
                DeveloperName = "Test Developer",
                PublisherName = "Test Publisher",
                ProductGroupId = "Group123",
                Category = "Test Category",
                ProductId = "abc",
                RevisionId = new DateTime(2024, 5, 5),
                XboxConsoleGenCompatible = ["ConsoleGen8"],
                PackageIdentityName = ""
            }
        }.GroupBy(p => p.TitleId);

        var product = _mapper.Map<Product>(storeProducts.First());

        using (new AssertionScope())
        {
            product.Title.Should().Be("Test Title");
            product.TitleId.Should().Be("123");
            product.DeveloperName.Should().Be("Test Developer");
            product.PublisherName.Should().Be("Test Publisher");
            product.ProductGroupId.Should().Be("Group123");
            product.Category.Should().Be("Test Category");
            product.Versions.Should().BeEquivalentTo([ new KeyValuePair<string,ProductVersion>(Device.XboxOne, new ProductVersion
            {
                Title = "Test Title",
                ProductId = "abc",
                RevisionId = new DateTime(2024, 5, 5),
                XboxConsoleGenCompatible = ["ConsoleGen8"],
                XboxConsoleGenOptimized = [],
                PackageIdentityName = ""
            })]);
        }
    }

    [Fact]
    public void Map_StoreProductGroup_To_Product_XboxSeries()
    {
        var storeProducts = new[]
        {
            new StoreProduct
            {
                Title = "Test Title",
                TitleId = "123",
                DeveloperName = "Test Developer",
                PublisherName = "Test Publisher",
                ProductGroupId = "Group123",
                Category = "Test Category",
                ProductId = "abc",
                RevisionId = new DateTime(2024, 5, 5),
                XboxConsoleGenCompatible = ["ConsoleGen9"],
                PackageIdentityName = ""
            }
        }.GroupBy(p => p.TitleId);

        var product = _mapper.Map<Product>(storeProducts.First());

        using (new AssertionScope())
        {
            product.Title.Should().Be("Test Title");
            product.TitleId.Should().Be("123");
            product.DeveloperName.Should().Be("Test Developer");
            product.PublisherName.Should().Be("Test Publisher");
            product.ProductGroupId.Should().Be("Group123");
            product.Category.Should().Be("Test Category");
            product.Versions.Should().BeEquivalentTo([ new KeyValuePair<string,ProductVersion>(Device.XboxSeries, new ProductVersion
            {
                Title = "Test Title",
                ProductId = "abc",
                RevisionId = new DateTime(2024, 5, 5),
                XboxConsoleGenCompatible = ["ConsoleGen9"],
                XboxConsoleGenOptimized = [],
                PackageIdentityName = ""
            })]);
        }
    }

    [Fact]
    public void Map_StoreProductGroup_To_Product_Unknown()
    {
        var storeProducts = new[]
        {
            new StoreProduct
            {
                XboxConsoleGenCompatible = ["LoremIpsum"],
                PackageIdentityName = ""
            }
        }.GroupBy(p => p.TitleId);

        Action act = () => _mapper.Map<Product>(storeProducts.First());

        act.Should().Throw<AutoMapperMappingException>().WithInnerException<Exception>().WithMessage("Unknown generation: LoremIpsum");
    }

    [Fact]
    public void Map_StoreProduct_To_ProductVersion()
    {
        var storeProduct = new StoreProduct
        {
            Title = "Test Title",
            ProductId = "123",
            RevisionId = DateTime.Now
        };

        var productVersion = _mapper.Map<ProductVersion>(storeProduct);

        using (new AssertionScope())
        {
            productVersion.Title.Should().Be("Test Title");
            productVersion.ProductId.Should().Be("123");
            productVersion.RevisionId.Should().Be(storeProduct.RevisionId);
        }
    }

    [Fact]
    public void Map_MarketplaceProduct_To_Product()
    {
        var marketplaceProduct = new MarketplaceProduct
        {
            DefaultTitle = "Test Title"
        };

        var product = _mapper.Map<Product>(marketplaceProduct);

        using (new AssertionScope())
        {
            product.Title.Should().Be("Test Title");
            product.Versions.Should().BeNull();
            product.Category.Should().BeNull();
        }
    }

    [Fact]
    public void Map_MarketplaceProduct_To_ProductVersion()
    {
        var marketplaceProduct = new MarketplaceProduct
        {
            DefaultTitle = "Test Title",
            GlobalOriginalReleaseDate = DateTime.Now
        };

        var productVersion = _mapper.Map<ProductVersion>(marketplaceProduct);

        using (new AssertionScope())
        {
            productVersion.Title.Should().Be("Test Title");
            productVersion.ReleaseDate.Should().Be(marketplaceProduct.GlobalOriginalReleaseDate);
            productVersion.XboxConsoleGenCompatible.Should().Contain(Device.Xbox360);
        }
    }

    [Fact]
    public void Map_Achievement_To_KqlAchievement()
    {
        var achievement = new Achievement
        {
            Unlocked = true,
            Rarity = new Rarity { CurrentCategory = "Rare", CurrentPercentage = 5.0 }
        };

        var kqlAchievement = _mapper.Map<KqlAchievement>(achievement);

        using (new AssertionScope())
        {
            kqlAchievement.IsUnlocked.Should().BeTrue();
            kqlAchievement.IsRare.Should().BeTrue();
            kqlAchievement.RarityPercentage.Should().Be(5.0);
        }
    }

    [Fact]
    public void Map_Stat_To_KqlMinutesPlayed()
    {
        var stat = new Stat
        {
            Name = "MinutesPlayed",
            Type = "Integer",
            Value = "120"
        };

        var kqlMinutesPlayed = _mapper.Map<KqlMinutesPlayed>(stat);

        kqlMinutesPlayed.Minutes.Should().Be(120);
    }

    [Fact]
    public void Map_Title_To_KqlTitle()
    {
        var title = new Title
        {
            CompatibleDevices = new[] { "Device1", "Device2" },
            Achievement = new AchievementSummary
            {
                CurrentAchievements = 10,
                TotalAchievements = 20,
                CurrentGamerscore = 1000,
                TotalGamerscore = 2000,
                ProgressPercentage = 50
            },
            TitleHistory = new TitleHistory
            {
                LastTimePlayed = DateTime.Now
            }
        };

        var kqlTitle = _mapper.Map<KqlTitle>(title);

        using (new AssertionScope())
        {
            kqlTitle.CompatibleWith.Should().Be("Device1|Device2");
            kqlTitle.CurrentAchievements.Should().Be(10);
            kqlTitle.TotalAchievements.Should().Be(20);
            kqlTitle.CurrentGamerscore.Should().Be(1000);
            kqlTitle.TotalGamerscore.Should().Be(2000);
            kqlTitle.ProgressPercentage.Should().Be(50);
            kqlTitle.LastTimePlayed.Should().Be(title.TitleHistory.LastTimePlayed);
        }
    }
}