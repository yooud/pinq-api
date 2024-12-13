using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using pinq.api.Filters;
using pinq.api.Repository;
using pinq.api.Services;
using IAuthorizationService = pinq.api.Services.IAuthorizationService;

namespace pinq.api.WebSockets;

[Authorize]
[ValidateSession]
public class MapWebSocketHandler(
    IAuthorizationService authorizationService,
    ISessionCacheService sessionService,
    IUserRepository userRepository,
    IMapRepository mapRepository) : WebSocketHandler(authorizationService, sessionService)
{
    protected override async Task HandleMessageAsync(JsonElement message)
    {
        Console.WriteLine($"type={message.GetProperty("type").GetString()}");
    }

    protected override async Task OnInitialAsync()
    {
        var user = await userRepository.GetUserByUid(Token.Uid);
        var profiles = await mapRepository.GetFriendsLocationsAsync(user.Id);
        await SendMessage( new { type = "initial", data = profiles });
    }
}