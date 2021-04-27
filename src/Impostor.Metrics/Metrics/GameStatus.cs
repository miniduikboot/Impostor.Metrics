using System.Linq;
using Impostor.Api.Games;
using Impostor.Api.Games.Managers;
using Impostor.Api.Innersloth;
using Impostor.Api.Net.Manager;
using Microsoft.Extensions.Logging;
using Prometheus.Client;

namespace Impostor.Metrics.Metrics
{
    public class GameStatus : IMetricStatus
    {
        private readonly IMetricFactory _metrics;

        public int MiraGames => _gameManager.GetGameCount(MapFlags.MiraHQ);

        public int SkeldGames => _gameManager.GetGameCount(MapFlags.Skeld);

        public int PolusGames => _gameManager.GetGameCount(MapFlags.Polus);

        public int ShipGames => _gameManager.Games.Count(game => game?.Options.Map == MapTypes.Airship);

        public int PlayersWaiting => _clientManager.Clients.Count(c => c.Player?.Game.GameState == GameStates.NotStarted);

        public int PlayersInGame => _clientManager.Clients.Count(c => c.Player?.Game.GameState == GameStates.Started);

        private readonly IGameManager _gameManager;

        private readonly IClientManager _clientManager;

        private readonly IGauge<long> _clientsInGame;

        private readonly IGauge<long> _clientsInLobby;

        private readonly IGauge<long> _miraGauge;

        private readonly IGauge<long> _skeldGauge;

        private readonly IGauge<long> _polusGauge;

        private readonly IGauge<long> _shipGames;

        public GameStatus(IGameManager gameManager, IClientManager clientManager, IMetricFactory metrics, ILogger<GameStatus> logger)
        {
            this._gameManager = gameManager;
            this._clientManager = clientManager;
            this._metrics = metrics;

            this._clientsInGame = metrics.CreateGaugeInt64("players_ingame", "Number of players in-game");
            this._clientsInLobby = metrics.CreateGaugeInt64("players_waiting", "Number of players waiting in lobby.");

            this._miraGauge = metrics.CreateGaugeInt64("mirahq_lobbies", "The MiraHQ Lobby Count");
            this._skeldGauge = metrics.CreateGaugeInt64("skeld_lobbies", "The Skeld Lobby Count");
            this._polusGauge = metrics.CreateGaugeInt64("polus_lobbies", "The Polus Lobby Count");
            this._shipGames = metrics.CreateGaugeInt64("ship_lobbies", "The Airship Lobby Count");
            logger.LogInformation("Impostor.Metrics: enabled game status.");
        }

        public void Update()
        {
            this._clientsInLobby.Set(this.PlayersWaiting);
            this._clientsInGame.Set(this.PlayersInGame);

            //avoid redundant enumeration
            var miraLobbies = this.MiraGames;
            var skeldLobbies = this.SkeldGames;
            var polusLobbies = this.PolusGames;
            var shipLobbies = this.ShipGames;

            this._miraGauge.Set(miraLobbies);
            this._skeldGauge.Set(skeldLobbies);
            this._polusGauge.Set(polusLobbies);
            this._shipGames.Set(shipLobbies);
        }
    }
}
