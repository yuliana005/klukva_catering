namespace BistroAPI.DTOs;

public class OrderRequest
{
    public OrderDataDto OrderData { get; set; } = new();
    public List<CartItemDto> CartItems { get; set; } = new();
    public int TotalAmount { get; set; }
}

public class OrderDataDto
{
    public string FullName { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string EventDate { get; set; } = string.Empty;
    public int GuestCount { get; set; }
    public string Location { get; set; } = string.Empty;
    public string AdditionalInfo { get; set; } = string.Empty;
}

public class CartItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Price { get; set; }
    public int Quantity { get; set; }
    public string Comment { get; set; } = string.Empty;
}