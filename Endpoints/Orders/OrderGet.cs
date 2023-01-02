using IWantApp.Infra.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IWantApp.Endpoints.Orders;

public class OrderGet
{
    public static string Template => "/orders/{id:guid}";
    public static string[] Methods => new string[] { HttpMethod.Get.ToString() };
    public static Delegate Handle => Action;

    [Authorize]
    public static async Task<IResult> Action(
        [FromRoute] Guid id, HttpContext http, ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        var order = await context.Orders.Include(o => o.Products).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return Results.BadRequest("Order not found");

        var clientClaim = http.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
        var employeeClaim = http.User.Claims.FirstOrDefault(c => c.Type == "EmployeeCode");

        if (order.ClientId != clientClaim.Value && employeeClaim == null)
            return Results.Forbid();

        var client = await userManager.FindByIdAsync(order.ClientId);

        var productResponse = order.Products.Select(p => new OrderProduct(p.Id, p.Name));
        var results = new OrderResponse(order.Id, client.Email, order.Total, productResponse, order.DeliveryAddress);

        return Results.Ok(results);
    }

}
