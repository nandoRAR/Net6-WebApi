using Dapper;
using IWantApp.Endpoints.Products;
using Microsoft.Data.SqlClient;

namespace IWantApp.Infra.Data;

public class QueryAllProductsSold
{
    private readonly IConfiguration configuration;

    public QueryAllProductsSold(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public async Task<IEnumerable<ProductSoldReportResponse>> Execute(int page, int rows)
    {
        var db = new SqlConnection(configuration["ConnectionStrings:IWantDb"]);
        var query =
            @"select p.Id, p.Name, count(*) Amount
            from Orders o inner join OrderProducts op on o.Id = op.OrdersId
            inner join Products p on p.Id = op.ProductsId
            group by p.Id, p.Name
            order by Amount desc
            OFFSET (@page - 1) * @rows ROWS FETCH NEXT @rows ROWS ONLY";
        return await db.QueryAsync<ProductSoldReportResponse>(query, new { page, rows });
    }
}
