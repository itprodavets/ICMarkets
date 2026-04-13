using ICMarkets.Application.Blockchains.Queries;
using FluentAssertions;

namespace ICMarkets.UnitTests.Validators;

public class GetBlockchainHistoryQueryValidatorTests
{
    private readonly GetBlockchainHistoryQueryValidator _validator = new();

    [Theory]
    [InlineData("btc", "main", 1, 20)]
    [InlineData("eth", "main", 1, 100)]
    [InlineData("dash", "main", 5, 10)]
    [InlineData("ltc", "main", 1, 1)]
    public async Task Validate_ValidQueries_ShouldPass(string network, string chain, int page, int pageSize)
    {
        var query = new GetBlockchainHistoryQuery(network, chain, page, pageSize);
        var result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_InvalidNetwork_ShouldFail()
    {
        var query = new GetBlockchainHistoryQuery("invalid", "main");
        var result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Network");
    }

    [Fact]
    public async Task Validate_EmptyChain_ShouldFail()
    {
        var query = new GetBlockchainHistoryQuery("btc", "");
        var result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_InvalidPage_ShouldFail(int page)
    {
        var query = new GetBlockchainHistoryQuery("btc", "main", page);
        var result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task Validate_InvalidPageSize_ShouldFail(int pageSize)
    {
        var query = new GetBlockchainHistoryQuery("btc", "main", 1, pageSize);
        var result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeFalse();
    }
}
