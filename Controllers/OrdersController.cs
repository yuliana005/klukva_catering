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