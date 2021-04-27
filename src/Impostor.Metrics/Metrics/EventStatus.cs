using System;
using System.Threading;
using Impostor.Api.Events;
using Impostor.Api.Events.Managers;
using Impostor.Api.Events.Player;
using Impostor.Api.Innersloth;
using Impostor.Metrics.Config;
using Microsoft.Extensions.Logging;
using Prometheus.Client;

namespace Impostor.Metrics.Metrics
{
    public class EventStatus : IEventListener, IMetricStatus
    {
        #region Counts

        #region Game Over

        public int GameOverByHumanVote => _overByHumanVote;

        public int GameOverByHumanTasks => _overByHumanTasks;

        public int GameOverByHumanDisconnect => _overByHumanDisconnect;

        public int GameOverByImpostorVote => _overByImpostorVote;

        public int GameOverByImpostorKill => _overByImpostorKill;

        public int GameOverByImpostorSabotage => _overBySabotage;

        public int GameOverByImpostorDisconnect => _overByImpostorDisconnect;

        private int 
            _overByHumanVote,
            _overByHumanTasks,
            _overByImpostorVote,
            _overByImpostorKill,
            _overBySabotage,
            _overByImpostorDisconnect,
            _overByHumanDisconnect;

        #endregion
        
        public int LobbiesPerSecond { get; private set; }

        public int MessagesPerSecond { get; private set; }

        public int MessageBytesPerSecond { get; private set; }

        public long MessageAverageLength => _messagesTotalCount != 0 ? _messagesTotalBytes / _messagesTotalCount : 0;

        private int _lobbies, _messagesRate, _messageBytesRate;

        private long _messagesTotalCount, _messagesTotalBytes;

        #endregion


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


        public EventStatus(IEventManager eventManager, IMetricFactory metrics, ILogger<EventStatus> logger)
        {
            eventManager.RegisterListener(this);
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
            logger.LogInformation("Impostor.Metrics: enabled event status.");

            var tmr = new System.Timers.Timer(1000) {AutoReset = true, Enabled = true};
            tmr.Elapsed += Update;
        }

        private void Update(object _, object __)
        {
            this.LobbiesPerSecond = _lobbies;
            this.MessagesPerSecond = _messagesRate;
            this.MessageBytesPerSecond = _messageBytesRate;
            this._lobbies = this._messagesRate = this._messageBytesRate = 0;
        }

        [EventListener(EventPriority.High)]
        public void OnGameCreated(IGameCreatedEvent  evt)
        {
            Interlocked.Increment(ref _lobbies);
        }

        [EventListener(EventPriority.High)]
        public void OnPlayerChat(IPlayerChatEvent evt)
        {
            Interlocked.Increment(ref _messagesRate);
            Interlocked.Increment(ref _messagesTotalCount);
            Interlocked.Add(ref _messageBytesRate, evt.Message.Length);
            Interlocked.Add(ref _messagesTotalBytes, evt.Message.Length);
        }

        [EventListener]
        public void OnGameWin(IGameEndedEvent evt)
        {
            switch (evt.GameOverReason)
            {
                case GameOverReason.HumansByVote:
                    Interlocked.Increment(ref _overByHumanVote);
                    return;
                case GameOverReason.HumansByTask:
                    Interlocked.Increment(ref _overByHumanTasks);
                    return;
                case GameOverReason.ImpostorByVote:
                    Interlocked.Increment(ref _overByImpostorVote);
                    return;
                case GameOverReason.ImpostorByKill:
                    Interlocked.Increment(ref _overByImpostorKill);
                    return;
                case GameOverReason.ImpostorBySabotage:
                    Interlocked.Increment(ref _overBySabotage);
                    return;
                case GameOverReason.ImpostorDisconnect:
                    Interlocked.Increment(ref _overByImpostorDisconnect);
                    return;
                case GameOverReason.HumansDisconnect:
                    Interlocked.Increment(ref _overByHumanDisconnect);
                    return;
                default:
                    throw new ArgumentOutOfRangeException("!");
            }
        }

        public void Update()
        {
            this._messagesPerSecond.Set(this.MessagesPerSecond);
            this._messagesAverageLength.Set(this.MessageAverageLength);
            this._messagesKbPerSecond.Set(this.MessageBytesPerSecond / 1024);

            this._gameOverHumanVote.Set(this.GameOverByHumanVote);
            this._gameOverHumanTasks.Set(this.GameOverByHumanTasks);
            this._gameOverHumanDisconnect.Set(this.GameOverByHumanDisconnect);
            this._gameOverImpostorVote.Set(this.GameOverByImpostorVote);
            this._gameOverImpostorKill.Set(this.GameOverByImpostorKill);
            this._gameOverImpostorSabotage.Set(this.GameOverByImpostorSabotage);
            this._gameOverImpostorDisconnect.Set(this.GameOverByImpostorDisconnect);
        }
    }
}
