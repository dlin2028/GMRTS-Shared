﻿using GMRTSClasses.CTSTransferData;
using GMRTSClasses.CTSTransferData.FactoryActions;
using GMRTSClasses.CTSTransferData.MetaActions;
using GMRTSClasses.CTSTransferData.UnitGround;
using GMRTSClasses.CTSTransferData.UnitUnit;
using GMRTSClasses.STCTransferData;
using GMRTSClasses.Units;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSClasses
{
    public class SignalRClient : IAsyncDisposable
    {
        private HubConnection connection;
        private Func<Guid, Unit> getUnit;
        private TimeSpan timeSinceHeartbeat;
        private TimeSpan heartbeatTimeout;

        private List<IDisposable> disposables;

        public event Action OnHeartbeatDeath = null;

        public SignalRClient(string url, Func<Guid, Unit> getUnit, TimeSpan heartbeatTimeout)
        {
            connection = new HubConnectionBuilder().AddJsonProtocol((a) => { a.PayloadSerializerOptions.IncludeFields = true; }).WithUrl(url).Build();
            this.getUnit = getUnit;
            timeSinceHeartbeat = TimeSpan.Zero;
            this.heartbeatTimeout = heartbeatTimeout;
            OnHeartbeat += Beat;

            disposables = new List<IDisposable>();

            disposables.Add(connection.On("HeartbeatFromServer", OnHeartbeat));
            disposables.Add(connection.On<Guid, ChangingData<Vector2>>("UpdatePosition", PosUpdate));
            disposables.Add(connection.On<Guid, ChangingData<float>>("UpdateHealth", HealthUpdate));
            disposables.Add(connection.On<Guid, ChangingData<float>>("UpdateRotation", RotationUpdate));
            disposables.Add(connection.On<Guid>("KillUnit", KillUnit));
            disposables.Add(connection.On<UnitSpawnData>("AddUnit", AddUnit));
            disposables.Add(connection.On<DateTime>("GameStarted", GameStart));
            disposables.Add(connection.On<ActionOver>("ActionOver", ActionDone));
            disposables.Add(connection.On<OrderCompleted>("OrderFinished", OrderFinished));
            disposables.Add(connection.On<ResourceUpdate>("ResourceUpdated", ResourceUpdated));
            disposables.Add(connection.On<ShotFiredData>("ShotFired", ShotFired));
        }

        public async ValueTask DisposeAsync()
        {
            foreach(IDisposable disposable in disposables)
            {
                disposable?.Dispose();
            }
            disposables.Clear();

            if (connection != null)
            {
                await connection.DisposeAsync();
                connection = null;
            }
        }

        public async Task<bool> JoinGameByName(string gameName, string userName)
        {
            return await connection.InvokeAsync<bool>("Join", gameName, userName);
        }

        public async Task<bool> JoinGameByNameAndCreateIfNeeded(string gameName, string userName)
        {
            return await connection.InvokeAsync<bool>("JoinAndMaybeCreate", gameName, userName);
        }

        public async Task RequestGameStart()
        {
            await connection.InvokeAsync("ReqStartGame");
        }

        public async Task LeaveGame()
        {
            await connection.InvokeAsync("Leave");
        }

        public async Task AssistAction(AssistAction action)
        {
            await connection.InvokeAsync("Assist", action);
        }

        public async Task AttackAction(AttackAction action)
        {
            await connection.InvokeAsync("Attack", action);
        }

        public async Task BuildFactoryAction(BuildBuildingAction action)
        {
            await connection.InvokeAsync("BuildBuilding", action);
        }

        public async Task MoveAction(MoveAction action)
        {
            await connection.InvokeAsync("Move", action);
        }

        public async Task DeleteAction(DeleteAction action)
        {
            await connection.InvokeAsync("Delete", action);
        }

        public async Task ReplaceAction<T>(ReplaceAction<T> action) where T : ClientAction
        {
            await connection.InvokeAsync("Replace" + typeof(T).Name, action);
        }

        public async Task ArbitraryMeta(MetaAction action)
        {
            switch (action)
            {
                case ReplaceAction<MoveAction> replace:
                    await ReplaceAction(replace);
                    break;
                case ReplaceAction<BuildBuildingAction> replace:
                    await ReplaceAction(replace);
                    break;
                case ReplaceAction<AttackAction> replace:
                    await ReplaceAction(replace);
                    break;
                case ReplaceAction<AssistAction> replace:
                    await ReplaceAction(replace);
                    break;
                case DeleteAction delete:
                    await DeleteAction(delete);
                    break;
                default:
                    throw new Exception();
            }
        }

        public async Task ArbitraryNonmeta(ClientAction action)
        {
            switch (action)
            {
                case MoveAction mv:
                    await MoveAction(mv);
                    break;
                case AssistAction assist:
                    await AssistAction(assist);
                    break;
                case AttackAction attack:
                    await AttackAction(attack);
                    break;
                case BuildBuildingAction build:
                    await BuildFactoryAction(build);
                    break;
                default:
                    throw new Exception();
            }
        }

        public async Task<bool> FactoryAct(FactoryAction action)
        {
            switch (action)
            {
                case EnqueueBuildOrder enq:
                    return await EnqueueFactoryOrder(enq);
                case CancelBuildOrder cancel:
                    return await CancelFactoryOrder(cancel);
                default:
                    throw new Exception();
            }
        }

        public async Task<bool> EnqueueFactoryOrder(EnqueueBuildOrder order)
        {
            return await connection.InvokeAsync<bool>("FactoryEnqueue", order);
        }

        public async Task<bool> CancelFactoryOrder(CancelBuildOrder order)
        {
            return await connection.InvokeAsync<bool>("FactoryCancel", order);
        }

        public async Task<bool> TryStart()
        {
            bool faulted = false;
            await connection.StartAsync().ContinueWith(t => faulted = t.IsFaulted);
            return !faulted;
        }

        public void UpdateHeartbeatTimer(TimeSpan elapsedTime)
        {
            timeSinceHeartbeat += elapsedTime;
            if (timeSinceHeartbeat > heartbeatTimeout)
            {
                OnHeartbeatDeath?.Invoke();
            }
        }

        public async Task KillConnection()
        {
            await connection.StopAsync();
        }

        public event Action OnHeartbeat;
        public event Action<Unit, ChangingData<Vector2>> OnPositionUpdate;
        public event Action<Unit, ChangingData<float>> OnHealthUpdate;
        public event Action<Unit, ChangingData<float>> OnRotationUpdate;
        public event Action<Unit> TriggerUnitDeath;
        public event Action<UnitSpawnData> SpawnUnit;
        public event Action<DateTime> OnGameStart;
        public event Action<ActionOver> OnActionFinish;
        public event Action<ResourceUpdate> OnResourceUpdated;
        public event Action<OrderCompleted> OnOrderCompleted;
        public event Action<ShotFiredData> OnShotFired;

        private async void Beat()
        {
            timeSinceHeartbeat = TimeSpan.Zero;
            await connection.InvokeAsync("HeartbeatFromClient");
        }

        private void PosUpdate(Guid id, ChangingData<Vector2> newPos)
        {
            Unit unit = getUnit(id);
            unit.Position = new Changing<Vector2>(newPos.Value, newPos.Delta, unit.Position.Changer, newPos.StartTime);
            OnPositionUpdate?.Invoke(unit, newPos);
        }

        private void HealthUpdate(Guid id, ChangingData<float> newHealth)
        {
            Unit unit = getUnit(id);
            unit.Health = new Changing<float>(newHealth.Value, newHealth.Delta, unit.Health.Changer, newHealth.StartTime);
            OnHealthUpdate?.Invoke(unit, newHealth);
        }

        private void RotationUpdate(Guid id, ChangingData<float> newRotation)
        {
            Unit unit = getUnit(id);
            unit.Rotation = new Changing<float>(newRotation.Value, newRotation.Delta, unit.Rotation.Changer, newRotation.StartTime);
            OnRotationUpdate?.Invoke(unit, newRotation);
        }

        private void ActionDone(ActionOver actOver)
        {
            OnActionFinish?.Invoke(actOver);
        }

        private void KillUnit(Guid id)
        {
            Unit unit = getUnit(id);
            TriggerUnitDeath?.Invoke(unit);
        }

        private void AddUnit(UnitSpawnData spawnInfo)
        {
            SpawnUnit?.Invoke(spawnInfo);
        }

        private void GameStart(DateTime time)
        {
            OnGameStart?.Invoke(time);
        }

        private void OrderFinished(OrderCompleted completed)
        {
            OnOrderCompleted?.Invoke(completed);
        }

        private void ResourceUpdated(ResourceUpdate update)
        {
            OnResourceUpdated?.Invoke(update);
        }
        
        private void ShotFired(ShotFiredData data)
        {
            OnShotFired?.Invoke(data);
        }
    }
}
