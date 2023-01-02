using IWantApp.Endpoints.Products;
using IWantApp.Infra.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IWantApp.Endpoints.Categories;

public class ProductPut
{
    public static string Template => "/products/{id:guid}";
    public static string[] Methods => new string[] { HttpMethod.Put.ToString() };
    public static Delegate Handle => Action;

    [Authorize(Policy = "EmployeePolicy")]
    public static async Task<IResult> Action([FromRoute] Guid id, HttpContext http, ProductRequest productRequest, ApplicationDbContext context)
    {
        var userId = http.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == productRequest.CategoryId);
        var product = await context.Products.Where(p => p.Id == id).FirstOrDefaultAsync();

        if (product == null)
            return Results.NotFound();

        product.EditInfo(
            productRequest.Name, category, productRequest.Description, productRequest.HasStock,
            productRequest.Active, productRequest.Price, userId);

        if (!product.IsValid)
            return Results.ValidationProblem(category.Notifications.ConvertToProblemDetails());

        await context.SaveChangesAsync();

        return Results.Ok();
    }
}
