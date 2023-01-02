using IWantApp.Domain.Users;
using IWantApp.Endpoints.Categories;
using IWantApp.Endpoints.Clients;
using IWantApp.Endpoints.Employees;
using IWantApp.Endpoints.Orders;
using IWantApp.Endpoints.Products;
using IWantApp.Endpoints.Security;
using IWantApp.Infra.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSqlServer<ApplicationDbContext>(builder.Configuration["ConnectionStrings:IWantDb"]);
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredLength = 3;
}).AddEntityFrameworkStores<ApplicationDbContext>();


//Service Token JWT
builder.Services.AddAuthorization(option =>
{
    option.FallbackPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
    option.AddPolicy("EmployeePolicy", p => p.RequireAuthenticatedUser().RequireClaim("EmployeeCode"));
    option.AddPolicy("Employee005Policy", p => p.RequireAuthenticatedUser().RequireClaim("EmployeeCode", "005"));
    option.AddPolicy("CpfPolicy", p => p.RequireAuthenticatedUser().RequireClaim("Cpf"));
});
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateActor = true,
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration["JwtBearerTokenSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtBearerTokenSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtBearerTokenSettings:SecretKey"]))
    };
});
//Service Query
builder.Services.AddScoped<QueryAllUsersWithClaimName>();
builder.Services.AddScoped<QueryAllProductsSold>();
//Service UserCreator
builder.Services.AddScoped<UserCreator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

//Endpoint Category
app.MapMethods(CategoryPost.Template, CategoryPost.Methods, CategoryPost.Handle).WithTags("Category API");
app.MapMethods(CategoryGetAll.Template, CategoryGetAll.Methods, CategoryGetAll.Handle).WithTags("Category API");
app.MapMethods(CategoryPut.Template, CategoryPut.Methods, CategoryPut.Handle).WithTags("Category API");

//Endpoint Employee
app.MapMethods(EmployeePost.Template, EmployeePost.Methods, EmployeePost.Handle).WithTags("Employee API");
app.MapMethods(EmployeeGetAll.Template, EmployeeGetAll.Methods, EmployeeGetAll.Handle).WithTags("Employee API");

//Endpoint Product
app.MapMethods(ProductPost.Template, ProductPost.Methods, ProductPost.Handle).WithTags("Product API");
app.MapMethods(ProductGetAll.Template, ProductGetAll.Methods, ProductGetAll.Handle).WithTags("Product API");
app.MapMethods(ProductGetShowcase.Template, ProductGetShowcase.Methods, ProductGetShowcase.Handle).WithTags("Product API");
app.MapMethods(ProductPut.Template, ProductPut.Methods, ProductPut.Handle).WithTags("Product API");
app.MapMethods(ProductSoldGet.Template, ProductSoldGet.Methods, ProductSoldGet.Handle).WithTags("Product API");

//Endpoint Client
app.MapMethods(ClientPost.Template, ClientPost.Methods, ClientPost.Handle).WithTags("Client API");
app.MapMethods(ClientGet.Template, ClientGet.Methods, ClientGet.Handle).WithTags("Client API");

//Endpoint Order
app.MapMethods(OrderPost.Template, OrderPost.Methods, OrderPost.Handle).WithTags("Order API");
app.MapMethods(OrderGet.Template, OrderGet.Methods, OrderGet.Handle).WithTags("Order API");

//Endpoint Token
app.MapMethods(TokenPost.Template, TokenPost.Methods, TokenPost.Handle).WithTags("Token API");

app.UseExceptionHandler("/error");
app.Map("/error", (HttpContext http) =>
{
    var error = http.Features?.Get<IExceptionHandlerFeature>()?.Error;
    if (error != null)
    {
        if (error is SqlException)
            return Results.Problem(title: "Database out", statusCode: 500);
        else if (error is BadHttpRequestException)
            return Results.Problem(title: "Error to convert data to other type. See all the information sent", statusCode: 500);

    }
    return Results.Problem(title: "An error ocurred", statusCode: 500);
});

app.Run();
