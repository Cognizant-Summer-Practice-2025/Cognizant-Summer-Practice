using backend_AI.DTO.Ai.Response;
using FluentAssertions;

namespace backend_AI.tests.DTO;

public class GenerateTextResponseTests
{
    [Fact]
    public void FromString_ShouldCreate_WithEmptyWhenNull()
    {
        var dto = GenerateTextResponse.FromString(null);
        dto.Response.Should().BeEmpty();
        dto.ToString().Should().Be("");
    }

    [Fact]
    public void ToString_ReturnsResponse()
    {
        var dto = new GenerateTextResponse("abc");
        dto.ToString().Should().Be("abc");
    }
}


