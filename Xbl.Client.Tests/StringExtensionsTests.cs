using FluentAssertions;
using Xbl.Client.Extensions;
using Xunit;

namespace Xbl.Client.Tests;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("1792227428", "6AD33864")]
    public void ToHexId_ShouldConvertToHexId(string titleId, string expected)
    {
        // Act
        var result = titleId.ToHexId();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("6AD33864", 1792227428)]
    public void FromHexId_ShouldConvertFromHexId(string hexId, int expected)
    {
        // Act
        var result = hexId.FromHexId();

        // Assert
        result.Should().Be(expected);
    }
}