using IWantApp.Infra.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IWantApp.Endpoints.Categories;

public class CategoryPut
{
    public static string Template => "/categories/{id:guid}";
    public static string[] Methods => new string[] { HttpMethod.Put.ToString() };
    public static Delegate Handle => Action;

    [Authorize(Policy = "EmployeePolicy")]
    public static async Task<IResult> Action([FromRoute] Guid id, HttpContext http, CategoryRequest categoryRequest, ApplicationDbContext context)
    {
        var userId = http.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        var category = await context.Categories.Where(c => c.Id == id).FirstOrDefaultAsync();

        if (category == null)
            return Results.NotFound();

        category.EditInfo(categoryRequest.Name, categoryRequest.Active, userId);

        if (!category.IsValid)
            return Results.ValidationProblem(category.Notifications.ConvertToProblemDetails());

        await context.SaveChangesAsync();

        return Results.Ok();
    }
}
