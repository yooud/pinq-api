using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using pinq.api.Filters;
using pinq.api.Models.Dto.Map;
using pinq.api.Repository;
using pinq.api.Services;
using IAuthorizationService = pinq.api.Services.IAuthorizationService;

namespace pinq.api.WebSockets;

[Authorize]
[ValidateSession]
public class MapWebSocketHandler(
    IAuthorizationService authorizationService,
    ISessionCacheService sessionService,
    IUserProfileRepository profileRepository,
    IMapRepository mapRepository,
    IFriendRepository friendRepository) : WebSocketHandler(authorizationService, sessionService, profileRepository)
{
    protected override async Task HandleMessageAsync(JsonElement message)
    {
        Console.WriteLine(message.GetProperty("type").GetString());
        if (message.GetProperty("type").GetString().Equals("update_location"))
        {
            var data = message.GetProperty("data");
            await OnUpdateLocationAsync(data);
        }
        else if (message.GetProperty("type").GetString().Equals("message"))
        {
            var data = message.GetProperty("data");
            var userId = data.GetProperty("user_id").GetInt32();
            var userMessage = data.GetProperty("message").GetString();
            var connection = ConnectionManager.GetConnection(userId);
            if (connection is null)
                await SendMessageAsync(new { type = "error", data = "User is not connected" });
            else
                await connection.SendMessageAsync(new { type = "message", data = new { UserId, message = userMessage } });
        }
        else
        {
            await SendMessageAsync(new { type = "error", data = "Invalid message type" });
        }
    }

    protected override async Task OnInitialAsync()
    {
        var friendsLocations = await mapRepository.GetFriendsLocationsAsync(UserId);
        var myLocation = await mapRepository.GetLocationsAsync(UserId);
        var result = friendsLocations.ToArray().Append(myLocation);
        await SendMessageAsync( new { type = "initial", data =  result});
    }
    
    private async Task OnUpdateLocationAsync(JsonElement data)
    {
        var location = data.GetProperty("location");
        var locationDto = new LocationDto
        {
            Lat = location.GetProperty("lat").GetDouble(),
            Lng = location.GetProperty("lng").GetDouble()
        };
        await mapRepository.UpdateLocationAsync(UserId, locationDto);

        var friends = await friendRepository.GetFriendIdsAsync(UserId);
        foreach (var friendId in friends)
            ConnectionManager.GetConnection(friendId)?.SendMessageAsync(new
            {
                type = "move", 
                data = new
                {
                    Id = friendId, 
                    Username, 
                    location
                }
            });
    }

    public async Task OnNewFriend(int friendId)
    {
        var location = await mapRepository.GetLocationsAsync(friendId);
        await SendMessageAsync(new { type = "new_friend", data = location });
    }
    
    public async Task OnFriendRemoved(int friendId)
    {
        await SendMessageAsync(new { type = "friend_removed", data = friendId });
    }
}