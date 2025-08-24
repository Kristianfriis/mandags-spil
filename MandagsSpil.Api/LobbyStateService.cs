using System.Collections.Concurrent;
using MandagsSpil.Shared.Contracts;

namespace MandagsSpil.Api.Services;

public class LobbyStateService
{
    private readonly ConcurrentDictionary<NationEnum, List<PlayerInfo>> _playersByNation = new();

    private readonly Dictionary<NationEnum, List<ClassInfo>> _classesByNation = new()
    {
        [NationEnum.USA] = new()
        {
            new ClassInfo(ClassNameEnum.Rifleman, new List<string> { "M1 Garand", "M1A1 Carbine" }, 8),
            new ClassInfo(ClassNameEnum.Support, new List<string> { "BAR (Browning Automatic Rifle)" }, 2),
            new ClassInfo(ClassNameEnum.Sniper, new List<string> { "Springfield" }, 1),
            new ClassInfo(ClassNameEnum.Engineer, new List<string> { "Thompson", "Grease Gun", "Shot gun" }, 2)
        },
        [NationEnum.UK] = new()
        {
            new ClassInfo(ClassNameEnum.Rifleman, new List<string> { "Lee-Enfield"}, 8),
            new ClassInfo(ClassNameEnum.Support, new List<string> { "Bren" }, 2),
            new ClassInfo(ClassNameEnum.Sniper, new List<string> { "Scoped Lee-Enfield" }, 1),
            new ClassInfo(ClassNameEnum.Engineer, new List<string> { "Sten", "Shot gun" }, 2)
        },
        [NationEnum.Germany] = new()
        {
            new ClassInfo(ClassNameEnum.Rifleman, new List<string> { "Kar98k", "Gewehr 43" }, 8),
            new ClassInfo(ClassNameEnum.Support, new List<string> { "MP44" }, 2),
            new ClassInfo(ClassNameEnum.Sniper, new List<string> { "Scoped Kar98k" }, 1),
            new ClassInfo(ClassNameEnum.Engineer, new List<string> { "MP40", "Shot gun" }, 2)
        },
        [NationEnum.USSR] = new()
        {
            new ClassInfo(ClassNameEnum.Rifleman, new List<string> { "Mosin-Nagant", "SVT-40" }, 8),
            new ClassInfo(ClassNameEnum.Support, new List<string> { "PPSH" }, 2),
            new ClassInfo(ClassNameEnum.Sniper, new List<string> { "Scoped Mosin-Nagant" }, 1),
            new ClassInfo(ClassNameEnum.Engineer, new List<string> { "PPS-42", "Shot gun" }, 2)
        }
    };

    public IReadOnlyDictionary<NationEnum, List<ClassInfo>> ClassesByNation => _classesByNation;

    public List<PlayerInfo> GetPlayers(NationEnum nation) =>
        _playersByNation.TryGetValue(nation, out var players) ? players : new List<PlayerInfo>();

    public void JoinLobby(string userName, NationEnum nation, Guid playerId, string connectionId)
    {
        var players = _playersByNation.GetOrAdd(nation, _ => new List<PlayerInfo>());

        lock (players)
        {
            if (!players.Any(p => p.Id == playerId))
            {
                players.Add(new PlayerInfo(userName, nation, ClassNameEnum.Unknown, playerId, connectionId));
            }
        }
    }

    public void LeaveLobby(string userName, NationEnum nation, Guid playerId)
    {
        if (_playersByNation.TryGetValue(nation, out var players))
        {
            lock (players)
            {
                var player = players.FirstOrDefault(p => p.Id == playerId);
                if (player != null)
                    players.Remove(player);
            }
        }
    }

    public NationEnum LeaveLobby(string connectionId)
    {
        foreach (var nation in _playersByNation.Keys)
        {
            if (_playersByNation.TryGetValue(nation, out var players))
            {
                lock (players)
                {
                    var player = players.FirstOrDefault(p => p.ConnectionId == connectionId);
                    if (player != null)
                    {
                        players.Remove(player);
                        return nation;
                    }
                }
            }
        }

        return NationEnum.Unknown;
    }

    public (bool success, string? message) SelectClass(string userName, NationEnum nation, ClassNameEnum className, Guid playerId)
    {
        if (_playersByNation.TryGetValue(nation, out var players))
        {
            lock (players)
            {
                var playerIndex = players.FindIndex(p => p.Id == playerId);
                if (playerIndex >= 0)
                {
                    var player = players[playerIndex];

                    bool classExists = _classesByNation.TryGetValue(nation, out var classes);

                    var classInfo = classes?.FirstOrDefault(c => c.Name == className);
                    int currentCount = players.Count(p => p.SelectedClass == className);

                    if (classInfo is not null)
                    {
                        if (classInfo.MaxPlayers > currentCount || player.SelectedClass == className)
                        {
                            players[playerIndex] = player with { SelectedClass = className };
                            return (true, null);
                        }
                        else
                        {
                            return (false, $"Class {className} is full. Max players: {classInfo.MaxPlayers}");
                        }
                    }
                }
            }
        }
        
        return (false, $"Could not select class {className} for player {userName}.");
    }

    public LobbyStateDto GetLobbyState(NationEnum nation)
    {
        var players = GetPlayers(nation);
        var classes = _classesByNation.TryGetValue(nation, out var result) ? result : new List<ClassInfo>();
        return new LobbyStateDto(nation, players, classes);
    }
}