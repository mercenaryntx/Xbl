using AutoMapper;
using Moq;
using Xbl.Client.Models.Dbox;
using Xbl.Client.Repositories;

namespace Xbl.Client.Tests.Repositories;

public class DboxRepositoryTests
{
    private Mock<IMapper> _mapperMock;
    private Mock<IRepository> _repositoryMock;
    private DboxRepository _dboxRepository;

    public DboxRepositoryTests()
    {
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IRepository>();
        _dboxRepository = new DboxRepository(_mapperMock.Object);
    }

    //[TestMethod]
    //public async Task GetMarketplaceProducts_ShouldReturnProducts()
    //{
    //    // Arrange
    //    var expectedProducts = new Dictionary<string, Product> { { "1", new Product() } };
    //    _repositoryMock.Setup(r => r.LoadJson<Dictionary<string, Product>>(It.IsAny<string>()))
    //        .ReturnsAsync(expectedProducts);

    //    // Act
    //    var result = await _dboxRepository.GetMarketplaceProducts();

    //    // Assert
    //    Assert.AreEqual(1, result.Count);
    //    CollectionAssert.AreEquivalent(expectedProducts, result);
    //}

    //[TestMethod]
    //public async Task GetStoreProducts_ShouldReturnProducts()
    //{
    //    // Arrange
    //    var expectedProducts = new Dictionary<string, Product> { { "1", new Product() } };
    //    _repositoryMock.Setup(r => r.LoadJson<Dictionary<string, Product>>(It.IsAny<string>()))
    //        .ReturnsAsync(expectedProducts);

    //    // Act
    //    var result = await _dboxRepository.GetStoreProducts();

    //    // Assert
    //    Assert.AreEqual(1, result.Count);
    //    CollectionAssert.AreEquivalent(expectedProducts, result);
    //}

    //[TestMethod]
    //public async Task ConvertMarketplaceProducts_ShouldSaveConvertedProducts()
    //{
    //    // Arrange
    //    var categories = new[] { new Category { Id = 1, Name = "Action" } };
    //    var marketplaceProducts = new[]
    //    {
    //        new MarketplaceProductCollection
    //        {
    //            Products = new []
    //            {
    //                new MarketplaceProduct { TitleId = "1", DefaultTitle = "Full Game - Test", Categories = new [] { 1 } }
    //            }
    //        }
    //    };

    //    _repositoryMock.Setup(r => r.LoadJson<Category[]>(It.IsAny<string>())).ReturnsAsync(categories);
    //    _repositoryMock.Setup(r => r.LoadJson<MarketplaceProductCollection[]>(It.IsAny<string>())).ReturnsAsync(marketplaceProducts);

    //    // Act
    //    await _dboxRepository.ConvertMarketplaceProducts();

    //    // Assert
    //    _repositoryMock.Verify(r => r.SaveJson(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    //}

    //[TestMethod]
    //public async Task ConvertStoreProducts_ShouldSaveConvertedProducts()
    //{
    //    // Arrange
    //    var storeProducts = new[]
    //    {
    //        new StoreProductCollection
    //        {
    //            Products = new []
    //            {
    //                new StoreProduct { TitleId = "1", ProductType = "Game" }
    //            }
    //        }
    //    };

    //    _repositoryMock.Setup(r => r.LoadJson<StoreProductCollection[]>(It.IsAny<string>())).ReturnsAsync(storeProducts);

    //    // Act
    //    await _dboxRepository.ConvertStoreProducts();

    //    // Assert
    //    _repositoryMock.Verify(r => r.SaveJson(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    //}
}