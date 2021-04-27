using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Impostor.Api.Events.Managers;
using Impostor.Metrics.Config;
using Impostor.Metrics.Export;
using Impostor.Metrics.Metrics;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.Logging;
using Prometheus.Client;
using Prometheus.Client.Collectors;
using Prometheus.Client.MetricServer;

namespace Impostor.Metrics
{
    public class PrometheusServer : IPrometheusServer
    {
        private readonly ILogger<PrometheusServer> _logger;

        public IMetricFactory Metrics { get; }

        private readonly EventStatus _events;

        private readonly GameStatus _games;

        private readonly CpuStatus _cpu;

        private readonly MemoryStatus _memory;

        private readonly ThreadStatus _threads;

        private readonly IMetricServer _server;

        private readonly System.Timers.Timer _clock;

        private readonly StatusConfiguration _config;

        private readonly Action[] _updates;

        #region Guages

        #region Clients

        private readonly IGauge<long> _clientsInGame;

        private readonly IGauge<long> _clientsInLobby;

        private readonly IGauge<long> _miraGauge;

        private readonly IGauge<long> _skeldGauge;

        private readonly IGauge<long> _polusGauge;

        private readonly IGauge<long> _shipGames;

        #endregion

        private readonly IGauge<long> _cpuGauge;

        #region Memory

        private readonly IGauge<long> _privateMemory;

        private readonly IGauge<long> _peakPagedMemory;

        private readonly IGauge<long> _peakWorkingSet;

        private readonly IGauge<long> _peakVirtualMemory;

        private readonly IGauge<long> _workingSet;

        private readonly IGauge<long> _pagedMemory;

        private readonly IGauge<long> _pagedSystemMemory;

        #endregion

        #region Threads
        
        private readonly IGauge<long> _totalThreads;

        private readonly IGauge<long> _threadPoolThreads;

        #endregion

        #region Events

        #region Chat

        private readonly IGauge<long> _messagesPerSecond;

        private readonly IGauge<long> _messagesAverageLength;

        private readonly IGauge<long> _messagesKbPerSecond;

        #endregion

        #region Game Over

        private readonly IGauge<long> _gameOverHumanVote;

        private readonly IGauge<long> _gameOverHumanTasks;

        private readonly IGauge<long> _gameOverHumanDisconnect;

        private readonly IGauge<long> _gameOverImpostorVote;

        private readonly IGauge<long> _gameOverImpostorKill;

        private readonly IGauge<long> _gameOverImpostorSabotage;

        private readonly IGauge<long> _gameOverImpostorDisconnect;

        #endregion

        #endregion

        #endregion

        public PrometheusServer(
            ILogger<PrometheusServer> logger,
            IEventManager eventManager,
            ICollectorRegistry registry,
            IMetricFactory metrics,
            EventStatus eventStatus,
            CpuStatus cpuStatus,
            MemoryStatus memoryStatus,
            GameStatus gameStatus,
            ThreadStatus threadStatus,
            StatusConfiguration config)
        {
            this._logger = logger;
            this.Metrics = metrics;
            this._events = eventStatus;
            this._cpu = cpuStatus;
            this._memory = memoryStatus;
            this._games = gameStatus;
            this._threads = threadStatus;
            this._config = config;

            eventManager.RegisterListener(_events);

            this._server = new MetricServer(registry, new MetricServerOptions()
            {
                Port = config.Port,
                MapPath = config.ExportEndPoint,
                Host = config.Host
            });

            this._clock = new System.Timers.Timer(1000) { AutoReset = true };
            this._clock.Elapsed += Update;

            var updater = new List<Action>();

            if (config.EnableGameStatus)
            {
                this._clientsInGame = metrics.CreateGaugeInt64("players_ingame", "Number of players in-game");
                this._clientsInLobby = metrics.CreateGaugeInt64("players_waiting", "Number of players waiting in lobby.");

                this._miraGauge = metrics.CreateGaugeInt64("mirahq_lobbies", "The MiraHQ Lobby Count");
                this._skeldGauge = metrics.CreateGaugeInt64("skeld_lobbies", "The Skeld Lobby Count");
                this._polusGauge = metrics.CreateGaugeInt64("polus_lobbies", "The Polus Lobby Count");
                this._shipGames = metrics.CreateGaugeInt64("ship_lobbies", "The Airship Lobby Count");
                updater.Add(UpdateGameStatus);
                _logger.LogInformation("Impostor.Metrics: enabled game status.");
            }

            if (config.EnableCpuStatus)
            {
                this._cpuGauge = metrics.CreateGaugeInt64("cpu_percent", "CPU Usage percent (process only)");
                updater.Add(UpdateCpuStatus);
                _logger.LogInformation("Impostor.Metrics: enabled CPU status.");
            }

            if (config.EnableMemoryStatus)
            {
                this._privateMemory = metrics.CreateGaugeInt64("memory_bytes", "The Memory Usage");
                this._peakPagedMemory = metrics.CreateGaugeInt64("peak_paged_memory_bytes", "N/A");
                this._peakWorkingSet = metrics.CreateGaugeInt64("peak_working_set_bytes", "N/A");
                this._peakVirtualMemory = metrics.CreateGaugeInt64("peak_virtual_memory_bytes", "N/A");
                this._workingSet = metrics.CreateGaugeInt64("working_set_bytes", "N/A");
                this._pagedMemory = metrics.CreateGaugeInt64("paged_memory_bytes", "N/A");
                this._pagedSystemMemory = metrics.CreateGaugeInt64("paged_system_memory_bytes", "N/A");
                updater.Add(UpdateMemoryStatus);
                _logger.LogInformation("Impostor.Metrics: enabled memory status.");
            }

            if (config.EnableThreadStatus)
            {
                this._totalThreads = metrics.CreateGaugeInt64("total_threads", "Total process threads.");
                this._threadPoolThreads = metrics.CreateGaugeInt64("pool_threads", "Total Thread Pool threads.");
                updater.Add(UpdateThreadStatus);
                _logger.LogInformation("Impostor.Metrics: enabled thread status.");
            }

            if (config.EnableEventStatus)
            {
                this._messagesPerSecond = metrics.CreateGaugeInt64("chat_rate", "Chat Messages / second");
                this._messagesKbPerSecond =
                    metrics.CreateGaugeInt64("chat_kbytespersecond", "Chat Messages Kilobyte / second");
                this._messagesAverageLength = metrics.CreateGaugeInt64("chat_avg_len", "Chat average message length");

                this._gameOverHumanVote = metrics.CreateGaugeInt64("over_human_vote", "Game over by crewmate vote.");
                this._gameOverHumanTasks =
                    metrics.CreateGaugeInt64("over_human_tasks", "Game over by crewmates completing tasks.");
                this._gameOverHumanDisconnect =
                    metrics.CreateGaugeInt64("over_human_disconnect", "Game over by crewmate disconnect.");
                this._gameOverImpostorVote =
                    metrics.CreateGaugeInt64("over_impostor_vote", "Game over by impostors voting crewmates.");
                this._gameOverImpostorKill =
                    metrics.CreateGaugeInt64("over_impostor_kill", "Game over by impostors killing everyone.");
                this._gameOverImpostorSabotage =
                    metrics.CreateGaugeInt64("over_impostor_sabotage", "Game over by sabotage.");
                this._gameOverImpostorDisconnect = metrics.CreateGaugeInt64("over_impostor_disconnect",
                    "Game over by impostors disconnecting.");
                updater.Add(UpdateEventStatus);
                _logger.LogInformation("Impostor.Metrics: enabled event status.");
            }

            this._updates = updater.ToArray();

            _logger.LogInformation("Impostor.Metrics: created exports.");
        }


        public void Start()
        {
            _server.Start();
            _clock.Start();
            _logger.LogInformation("Impostor.Metrics: successfully started server.");
        }

        private void Update(object _, object __)
        {
            foreach (var update in _updates) update();
        }

        private void UpdateCpuStatus()
        {
            this._cpuGauge.Set((long)_cpu.UsagePercent);
        }

        private void UpdateMemoryStatus()
        {
            this._peakPagedMemory.Set(_memory.PeakPagedMemory);
            this._peakWorkingSet.Set(_memory.PeakWorkingSet);
            this._peakVirtualMemory.Set(_memory.PeakVirtualMemory);
            this._workingSet.Set(_memory.WorkingSet);
            this._pagedMemory.Set(_memory.PagedMemorySize);
            this._pagedSystemMemory.Set(_memory.PagedSystemMemorySize);
            this._privateMemory.Set(_memory.PrivateBytes);
        }

        private void UpdateEventStatus()
        {
            this._messagesPerSecond.Set(_events.MessagesPerSecond);
            this._messagesAverageLength.Set(_events.MessageAverageLength);
            this._messagesKbPerSecond.Set(_events.MessageBytesPerSecond / 1024);

            this._gameOverHumanVote.Set(_events.GameOverByHumanVote);
            this._gameOverHumanTasks.Set(_events.GameOverByHumanTasks);
            this._gameOverHumanDisconnect.Set(_events.GameOverByHumanDisconnect);
            this._gameOverImpostorVote.Set(_events.GameOverByImpostorVote);
            this._gameOverImpostorKill.Set(_events.GameOverByImpostorKill);
            this._gameOverImpostorSabotage.Set(_events.GameOverByImpostorSabotage);
            this._gameOverImpostorDisconnect.Set(_events.GameOverByImpostorDisconnect);
        }

        private void UpdateGameStatus()
        {
            this._clientsInLobby.Set(_games.PlayersWaiting);
            this._clientsInGame.Set(_games.PlayersInGame);

            //avoid redundant enumeration
            var miraLobbies = _games.MiraGames;
            var skeldLobbies = _games.SkeldGames;
            var polusLobbies = _games.PolusGames;
            var shipLobbies = _games.ShipGames;

            this._miraGauge.Set(miraLobbies);
            this._skeldGauge.Set(skeldLobbies);
            this._polusGauge.Set(polusLobbies);
            this._shipGames.Set(shipLobbies);
        }

        private void UpdateThreadStatus()
        {
            this._totalThreads.Set(_threads.Total);
            this._threadPoolThreads.Set(_threads.Pooled);

        }

        public void Stop()
        {
            _server.Stop();
            _clock.Stop();
            _clock.Dispose();
        }
    }
}
