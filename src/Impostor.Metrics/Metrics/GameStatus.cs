using System.Linq;
using Impostor.Api.Games;
using Impostor.Api.Games.Managers;
using Impostor.Api.Innersloth;
using Impostor.Api.Net.Manager;

namespace Impostor.Metrics.Metrics
{
    public class GameStatus
    {
        public int MiraGames => _gameManager.GetGameCount(MapFlags.MiraHQ);

        public int SkeldGames => _gameManager.GetGameCount(MapFlags.Skeld);

        public int PolusGames => _gameManager.GetGameCount(MapFlags.Polus);

        public int ShipGames => _gameManager.Games.Count(game => game?.Options.Map == MapTypes.Airship);

        public int PlayersWaiting => _clientManager.Clients.Count(c => c.Player?.Game.GameState == GameStates.NotStarted);

        public int PlayersInGame => _clientManager.Clients.Count(c => c.Player?.Game.GameState == GameStates.Started);

        private readonly IGameManager _gameManager;

        private readonly IClientManager _clientManager;

        public GameStatus(IGameManager gameManager, IClientManager clientManager)
        {
            this._gameManager = gameManager;
            this._clientManager = clientManager;
        }
    }
}
