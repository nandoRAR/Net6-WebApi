using IWantApp.Infra.Data;
using Microsoft.AspNetCore.Authorization;

namespace IWantApp.Endpoints.Products;

public class ProductSoldGet
{
    public static string Template => "/products/sold";
    public static string[] Methods => new string[] { HttpMethod.Get.ToString() };
    public static Delegate Handle => Action;

    [Authorize(Policy = "EmployeePolicy")]
    public static async Task<IResult> Action(QueryAllProductsSold query, int page = 1, int rows = 10)
    {
        if (rows > 10) return Results.Problem(title: "Row with max 10", statusCode: 400);

        var result = await query.Execute(page, rows);
        return Results.Ok(result);
    }

}
