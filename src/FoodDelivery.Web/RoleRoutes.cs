namespace FoodDelivery.Web;

public static class RoleRoutes
{
    public static string HomeFor(string role) => role switch
    {
        "Customer"        => "/customer/restaurants",
        "RestaurantOwner" => "/owner/restaurants",
        "Courier"         => "/courier/pending",
        _                  => "/account"
    };
}
