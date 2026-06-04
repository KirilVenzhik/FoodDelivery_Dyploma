namespace FoodDelivery.Web.Models;

// ---- Auth ----
public record AuthResponse(string AccessToken, string RefreshToken, Guid UserId, string Role);
public record UserDto(Guid UserId, string Email, string Role);
public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password, string Role);
public record RefreshRequest(string RefreshToken);

// ---- Restaurant ----
public record RestaurantSummaryResponse(Guid Id, string Name, string Description, string Address, bool IsActive, string Status);
public record RestaurantResponse(Guid Id, string Name, string Description, string Address, bool IsActive, string Status, List<MenuItemResponse> MenuItems);
public record MenuItemResponse(Guid Id, string Name, string Description, decimal Price, bool Available);

// ---- Order ----
public record OrderResponse(Guid Id, Guid RestaurantId, decimal TotalAmount, string Status, DateTime CreatedAt, List<OrderItemResponse> Items);
public record OrderItemResponse(Guid Id, Guid MenuItemId, string ItemName, decimal UnitPrice, int Quantity, decimal LineTotal);

// ---- Delivery ----
public record DeliveryResponse(Guid Id, Guid OrderId, Guid? CourierId, string Status, DateTime CreatedAt, DateTime UpdatedAt);

// ---- Notification ----
public record NotificationResponse(Guid Id, Guid OrderId, string Message, string Type, bool IsRead, DateTime CreatedAt, DateTime? SentAt);

// ---- Payment ----
public record PaymentResponse(Guid Id, Guid OrderId, decimal Amount, string Status, DateTime CreatedAt);

// ---- Requests ----
public record CreateOrderRequest(Guid RestaurantId, List<OrderItemRequest> Items);
public record OrderItemRequest(Guid MenuItemId, int Quantity);
public record CreateRestaurantRequest(string Name, string Description, string Address);
public record AddMenuItemRequest(string Name, string Description, decimal Price);
public record UpdateMenuItemRequest(string? Name, string? Description, decimal? Price, bool? Available);
public record ChangeOrderStatusRequest(string NewStatus);
