using System;
using System.Threading;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Innersloth;
using Microsoft.Extensions.Logging;

namespace Impostor.Metrics.Metrics
{
    public class EventStatus : IEventListener
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

        public long MessageAverageLength => _messagesTotalBytes / _messagesTotalCount;

        private int _lobbies, _messagesRate, _messageBytesRate;

        private long _messagesTotalCount, _messagesTotalBytes;

        #endregion

        public EventStatus()
        {
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
    }
}
