using IWantApp.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IWantApp.Endpoints.Clients;

public class ClientPost
{
    public static string Template => "/clients";
    public static string[] Methods => new string[] { HttpMethod.Post.ToString() };
    public static Delegate Handle => Action;

    [AllowAnonymous]
    public static async Task<IResult> Action(ClientRequest clientRequest, UserCreator userCreator)
    {
        var userClaims = new List<Claim>
        {
            new Claim("Cpf", clientRequest.Cpf),
            new Claim("Name", clientRequest.Name)
        };
        (IdentityResult result, string userId) = await userCreator.Create(clientRequest.Email, clientRequest.Password, userClaims);
        if (!result.Succeeded)
            return Results.ValidationProblem(result.Errors.ConvertToProblemDetails());

        return Results.Created($"/clients/{userId}", userId);
    }
}
