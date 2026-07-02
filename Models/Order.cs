using System.Text.Json.Serialization;

namespace BistroAPI.Models;

public class Order
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string EventDate { get; set; } = string.Empty;
    public int GuestCount { get; set; }
    public string Location { get; set; } = string.Empty;
    public string AdditionalInfo { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int TotalPrice { get; set; }
    
    // Связь с позициями заказа
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Price { get; set; }
    public int Quantity { get; set; }
    public string Comment { get; set; } = string.Empty;
    
    public int OrderId { get; set; }
    [JsonIgnore]
    public Order? Order { get; set; }
}