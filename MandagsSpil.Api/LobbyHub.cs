using MandagsSpil.Api.Services;
using MandagsSpil.Shared.Contracts;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace MandagsSpil.Api.Hubs;

public class LobbyHub : Hub
{
    private readonly LobbyStateService _lobbyState;

    public LobbyHub(LobbyStateService lobbyState)
    {
        _lobbyState = lobbyState;
    }

    public async Task JoinLobby(string userName, string nation, Guid playerId)
    {
        var mappedNation = Enum.Parse<NationEnum>(nation, true);
        await Groups.AddToGroupAsync(Context.ConnectionId, nation.ToString());

        _lobbyState.JoinLobby(userName, mappedNation, playerId, Context.ConnectionId);

        await Clients.Group(nation.ToString()).SendAsync("LobbyUpdated", _lobbyState.GetLobbyState(mappedNation));
    }

    public async Task LeaveLobby(string userName, string nation, Guid playerId)
    {
        var mappedNation = Enum.Parse<NationEnum>(nation, true);
        _lobbyState.LeaveLobby(userName, mappedNation, playerId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, nation.ToString());

        await Clients.Group(nation.ToString()).SendAsync("LobbyUpdated", _lobbyState.GetLobbyState(mappedNation));
    }

    public async Task<ResponseType> SelectClass(string userName, string nation, string className, Guid playerId)
    {
        var mappedNation = Enum.Parse<NationEnum>(nation, true);
        var classEnum = Enum.Parse<ClassNameEnum>(className, true);
        var result = _lobbyState.SelectClass(userName, mappedNation, classEnum, playerId);

        var mappedResult = new ResponseType
        {
            Success = result.success,
            Message = result.message
        };

        if (result.success)
        {
            await Clients.Group(nation.ToString()).SendAsync("LobbyUpdated", _lobbyState.GetLobbyState(mappedNation));
            return mappedResult;
        }

        return mappedResult;
    }

    public LobbyStateDto GetLobbyState(string nation)
    {
        var mappedNation = Enum.Parse<NationEnum>(nation, true);
        return _lobbyState.GetLobbyState(mappedNation);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var nation = _lobbyState.LeaveLobby(Context.ConnectionId);

        if (nation != NationEnum.Unknown)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, nation.ToString());
            await Clients.Group(nation.ToString()).SendAsync("LobbyUpdated", _lobbyState.GetLobbyState(nation));
        }
        await base.OnDisconnectedAsync(exception);
    }
}