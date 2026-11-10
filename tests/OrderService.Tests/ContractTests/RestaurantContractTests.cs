using System.Text.Json;
using FluentAssertions;
using OrderService.Application.DTOs;

namespace OrderService.Tests.ContractTests;

// Restaurant service answers with this JSON. Order must be able to read it,
// otherwise a field name mismatch silently leaves a property at its default value.
public class RestaurantContractTests
{
    private const string RestaurantJson = """
    {
      "id": "11111111-1111-1111-1111-111111111111",
      "ownerId": "22222222-2222-2222-2222-222222222222",
      "name": "Test Restaurant",
      "description": "d",
      "address": "1 Main St",
      "isActive": true,
      "status": "Open",
      "menuItems": [
        {
          "id": "33333333-3333-3333-3333-333333333333",
          "name": "Pizza",
          "description": "x",
          "price": 9.5,
          "available": true
        }
      ]
    }
    """;

    [Fact]
    public void RestaurantResponse_Json_MapsToRestaurantDto()
    {
        // Same options as HttpClient.GetFromJsonAsync (web defaults).
        var restaurant = JsonSerializer.Deserialize<RestaurantDto>(
            RestaurantJson, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        restaurant.Should().NotBeNull();
        restaurant!.Id.Should().Be(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        restaurant.OwnerId.Should().Be(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        restaurant.Name.Should().Be("Test Restaurant");
        restaurant.Status.Should().Be("Open");
        restaurant.IsActive.Should().BeTrue();

        var item = restaurant.MenuItems.Should().ContainSingle().Subject;
        item.Id.Should().Be(Guid.Parse("33333333-3333-3333-3333-333333333333"));
        item.Name.Should().Be("Pizza");
        item.Price.Should().Be(9.5m);
        item.Available.Should().BeTrue();
    }
}
