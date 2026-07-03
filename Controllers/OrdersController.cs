using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BistroAPI.Data;
using BistroAPI.Models;
using BistroAPI.DTOs;

namespace BistroAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrdersController(AppDbContext context)
    {
        _context = context;
    }

    // POST: api/orders/create
    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
    {
        try
        {
            // Создаём заказ
            var order = new Order
            {
                FullName = request.OrderData.FullName,
                Contact = request.OrderData.Contact,
                EventType = request.OrderData.EventType,
                EventDate = request.OrderData.EventDate,
                GuestCount = request.OrderData.GuestCount,
                Location = request.OrderData.Location ?? "",
                AdditionalInfo = request.OrderData.AdditionalInfo ?? "",
                TotalPrice = request.TotalAmount,
                CreatedAt = DateTime.Now
            };

            // Добавляем позиции заказа
            foreach (var item in request.CartItems)
            {
                order.Items.Add(new OrderItem
                {
                    Name = item.Name,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    Comment = item.Comment ?? ""
                });
            }

            // Сохраняем в базу
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await SendVkNotification(order);
            Console.WriteLine($"✅ Заказ #{order.Id} сохранён в БД!");

            // Отправка уведомления в Telegram (заглушка)
            // await SendTelegramNotification(order);

            return Ok(new { 
                success = true, 
                message = "Заказ успешно создан", 
                orderId = order.Id 
            });
            
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
    private async Task SendVkNotification(Order order)
    {
        var token = "vk1.a.nfVPLRl6EoBSG-njj6p-e9z5bKohFEyQvexsACYTsaXhx1zgmueggFUuJx0rk32wXrRmqes2lN76DUn09lfIsD5HH3_LRbFBvu4Ra3eSq_fE2eyvTA5XBRBfPbza3jESOfCk6G-NRQUTsyE18VEUhX8u0Zeg_dHuXaY-GZd9rogFjaqp7lgn2bVJIWXDSTR237vpyD1D4rgQ0VVPY4Wo9A";  // Замените на ваш токен
        var userId = 295381770;              // Замените на ваш ID ВК

        var message = $"🆕 Новый заказ #{order.Id}!\n\n" +
                      $"👤 Клиент: {order.FullName}\n" +
                      $"📞 Контакт: {order.Contact}\n" +
                      $"📅 Мероприятие: {order.EventType} на {order.EventDate}\n" +
                      $"👥 Гостей: {order.GuestCount}\n\n" +
                      $"🍽️ Блюда:\n";

        foreach (var item in order.Items)
        {
            message += $"   • {item.Name} x{item.Quantity} = {item.Price * item.Quantity}₽";
            if (!string.IsNullOrEmpty(item.Comment))
                message += $" (Комментарий: {item.Comment})";
            message += "\n";
        }

        message += $"\n💰 Итого: {order.TotalPrice}₽\n" +
                   $"📍 {order.Location}\n" +
                   $"📝 {order.AdditionalInfo}";

        try
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.vk.com/method/messages.send");
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("user_id", userId.ToString()),
                new KeyValuePair<string, string>("message", message),
                new KeyValuePair<string, string>("random_id", new Random().Next(1000000, 9999999).ToString()),
                new KeyValuePair<string, string>("access_token", token),
                new KeyValuePair<string, string>("v", "5.131")
            });
            request.Content = content;
            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"✅ Уведомление отправлено: {responseString}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка отправки уведомления: {ex.Message}");
        }
    }

    // GET: api/orders/all
    [HttpGet("all")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        
        return Ok(orders);
    }

    // GET: api/orders/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
        
        if (order == null)
            return NotFound(new { success = false, message = "Заказ не найден" });
        
        return Ok(order);
    }
}