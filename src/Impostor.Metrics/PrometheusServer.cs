using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Impostor.Api.Events.Managers;
using Impostor.Metrics.Metrics;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.Logging;
using Prometheus.Client;
using Prometheus.Client.Collectors;
using Prometheus.Client.MetricServer;

namespace Impostor.Metrics
{
    public class PrometheusServer
    {
        private readonly ILogger<PrometheusServer> _logger;

        private readonly EventStatus _monitor;

        private readonly GameStatus _games;

        private readonly CpuStatus _cpu;

        private readonly ThreadStatus _threads;

        private readonly IMetricServer _server;

        private readonly System.Timers.Timer _clock;

        private readonly Process _proc;

        #region Guages

        private readonly IGauge<long> _playerGauge;

        private readonly IGauge<long> _clientsInGame;

        private readonly IGauge<long> _clientsInLobby;

        private readonly IGauge<long> _lobbyGauge;

        private readonly IGauge<long> _miraGauge;

        private readonly IGauge<long> _skeldGauge;

        private readonly IGauge<long> _polusGauge;

        private readonly IGauge<long> _shipGames;

        private readonly IGauge<long> _cpuGauge;

        private readonly IGauge<long> _memoryGauge;

        private readonly IGauge<long> _totalThreads;

        private readonly IGauge<long> _threadPoolThreads;

        private readonly IGauge<long> _messagesPerSecond;

        private readonly IGauge<long> _messagesAverageLength;

        private readonly IGauge<long> _messagesKbPerSecond;

        private readonly IGauge<long> _gameOverHumanVote;

        private readonly IGauge<long> _gameOverHumanTasks;

        private readonly IGauge<long> _gameOverHumanDisconnect;

        private readonly IGauge<long> _gameOverImpostorVote;

        private readonly IGauge<long> _gameOverImpostorKill;

        private readonly IGauge<long> _gameOverImpostorSabotage;

        private readonly IGauge<long> _gameOverImpostorDisconnect;

        #endregion

        public PrometheusServer(ILogger<PrometheusServer> logger, IEventManager eventManager, ICollectorRegistry registry, IMetricFactory metrics, EventStatus eventStatus, CpuStatus cpuStatus, GameStatus gameStatus, ThreadStatus threadStatus)
        {
            this._logger = logger;
            this._monitor = eventStatus;
            this._cpu = cpuStatus;
            this._games = gameStatus;
            this._threads = threadStatus;
            eventManager.RegisterListener(_monitor);

            this._server = new MetricServer(registry, new MetricServerOptions()
            {
                Port = 8080
            });

            this._proc = Process.GetCurrentProcess();

            this._clock = new System.Timers.Timer(1000) { AutoReset = true };
            this._clock.Elapsed += Update;

            this._playerGauge = metrics.CreateGaugeInt64("players", "The Player Count");
            this._clientsInGame = metrics.CreateGaugeInt64("players_ingame", "Number of players in-game");
            this._clientsInLobby = metrics.CreateGaugeInt64("players_waiting", "Number of players waiting in lobby.");
            this._lobbyGauge = metrics.CreateGaugeInt64("lobbies", "The Lobby Count");
            this._miraGauge = metrics.CreateGaugeInt64("mirahq_lobbies", "The MiraHQ Lobby Count");
            this._skeldGauge = metrics.CreateGaugeInt64("skeld_lobbies", "The Skeld Lobby Count");
            this._polusGauge = metrics.CreateGaugeInt64("polus_lobbies", "The Polus Lobby Count");
            this._shipGames = metrics.CreateGaugeInt64("ship_lobbies", "The Airship Lobby Count");
            this._memoryGauge = metrics.CreateGaugeInt64("memory_bytes", "The Memory Usage");
            this._cpuGauge = metrics.CreateGaugeInt64("cpu_percent", "CPU Usage percent (process only)");
            this._totalThreads = metrics.CreateGaugeInt64("total_threads", "Total process threads.");
            this._threadPoolThreads = metrics.CreateGaugeInt64("pool_threads", "Total Thread Pool threads.");

            this._messagesPerSecond = metrics.CreateGaugeInt64("chat_rate", "Chat Messages / second");
            this._messagesKbPerSecond = metrics.CreateGaugeInt64("chat_kbytespersecond", "Chat Messages Kilobyte / second");
            this._messagesAverageLength = metrics.CreateGaugeInt64("chat_avg_len","Chat average message length");
            
            this._gameOverHumanVote = metrics.CreateGaugeInt64("over_human_vote", "Game over by crewmate vote.");
            this._gameOverHumanTasks = metrics.CreateGaugeInt64("over_human_tasks", "Game over by crewmates completing tasks.");
            this._gameOverHumanDisconnect = metrics.CreateGaugeInt64("over_human_disconnect", "Game over by crewmate disconnect.");
            this._gameOverImpostorVote = metrics.CreateGaugeInt64("over_impostor_vote", "Game over by impostors voting crewmates.");
            this._gameOverImpostorKill = metrics.CreateGaugeInt64("over_impostor_kill", "Game over by impostors killing everyone.");
            this._gameOverImpostorSabotage = metrics.CreateGaugeInt64("over_impostor_sabotage", "Game over by sabotage.");
            this._gameOverImpostorDisconnect = metrics.CreateGaugeInt64("over_impostor_disconnect", "Game over by impostors disconnecting.");


            _logger.LogInformation("Impostor.Metrics: created exports.");

        }

        private void Update(object _, object __)
        {
            this._playerGauge.Set(_games.Players);
            this._clientsInLobby.Set(_games.PlayersWaiting);
            this._clientsInGame.Set(_games.PlayersInGame);

            //avoid redundant enumeration
            var miraLobbies = _games.MiraGames;
            var skeldLobbies = _games.SkeldGames;
            var polusLobbies = _games.PolusGames;
            var shipLobbies = _games.ShipGames;

            this._lobbyGauge.Set(miraLobbies + skeldLobbies + polusLobbies + shipLobbies);
            this._miraGauge.Set(miraLobbies);
            this._skeldGauge.Set(skeldLobbies);
            this._polusGauge.Set(polusLobbies);
            this._shipGames.Set(shipLobbies);

            this._memoryGauge.Set(_proc.PrivateMemorySize64);
            _proc.Refresh();

            this._cpuGauge.Set((long)_cpu.UsagePercent);

            this._totalThreads.Set(_threads.Total);
            this._threadPoolThreads.Set(_threads.Pooled);

            this._messagesPerSecond.Set(_monitor.MessagesPerSecond);
            this._messagesAverageLength.Set(_monitor.MessageAverageLength);
            this._messagesKbPerSecond.Set(_monitor.MessageBytesPerSecond / 1024);

            this._gameOverHumanVote.Set(_monitor.GameOverByHumanVote);
            this._gameOverHumanTasks.Set(_monitor.GameOverByHumanTasks);
            this._gameOverHumanDisconnect.Set(_monitor.GameOverByHumanDisconnect);
            this._gameOverImpostorVote.Set(_monitor.GameOverByImpostorVote);
            this._gameOverImpostorKill.Set(_monitor.GameOverByImpostorKill);
            this._gameOverImpostorSabotage.Set(_monitor.GameOverByImpostorSabotage);
            this._gameOverImpostorDisconnect.Set(_monitor.GameOverByImpostorDisconnect);
        }

        public void Start()
        {
            _server.Start();
            _clock.Start();
            _logger.LogInformation("Impostor.Metrics: successfully started server.");
        }

        public void Stop()
        {
            _server.Stop();
            _clock.Stop();
            _clock.Dispose();
        }
    }
}
