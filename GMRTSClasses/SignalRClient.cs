﻿using GMRTSClasses.CTSTransferData;
using GMRTSClasses.STCTransferData;
using GMRTSClasses.Units;

using Microsoft.AspNet.SignalR.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSClasses
{
    public class SignalRClient
    {
        private HubConnection connection;
        private IHubProxy hubProxy;
        private Func<Guid, Unit> getUnit;
        private TimeSpan timeSinceHeartbeat;
        private TimeSpan heartbeatTimeout;

        public event Action OnHeartbeatDeath = null;

        public SignalRClient(string url, string hubName, Func<Guid, Unit> getUnit, TimeSpan heartbeatTimeout)
        {
            connection = new HubConnection(url);
            hubProxy = connection.CreateHubProxy(hubName);
            this.getUnit = getUnit;
            timeSinceHeartbeat = TimeSpan.Zero;
            this.heartbeatTimeout = heartbeatTimeout;
            OnHeartbeat += Beat;

            //These are IDisposable. I should probably make this class IDisposable, too, so it can dispose of them.
            hubProxy.On("HeartbeatFromServer", OnHeartbeat);
            hubProxy.On<Guid, ChangingData<Vector2>>("UpdatePosition", PosUpdate);
            hubProxy.On<Guid, ChangingData<float>>("UpdateHealth", HealthUpdate);
            hubProxy.On<Guid, ChangingData<float>>("UpdateRotation", RotationUpdate);
            hubProxy.On<Guid>("KillUnit", KillUnit);
            hubProxy.On<UnitSpawnData>("AddUnit", AddUnit);
        }

        public async Task JoinGameByName(string name)
        {
            await hubProxy.Invoke("Join", name);
        }

        public async Task JoinGameByNameAndCreateIfNeeded(string name)
        {
            await hubProxy.Invoke("JoinAndMaybeCreate", name);
        }

        public async Task LeaveGame()
        {
            await hubProxy.Invoke("Leave");
        }

        public async Task AssistAction(AssistAction action)
        {
            await hubProxy.Invoke("Assist", action);
        }

        public async Task AttackAction(AttackAction action)
        {
            await hubProxy.Invoke("Attack", action);
        }

        public async Task BuildFactoryAction(BuildFactoryAction action)
        {
            await hubProxy.Invoke("BuildFactory", action);
        }

        public async Task MoveAction(MoveAction action)
        {
            await hubProxy.Invoke("Move", action);
        }

        public async Task PatrolAction(PatrolAction action)
        {
            await hubProxy.Invoke("Patrol", action);
        }

        public async Task<bool> TryStart()
        {
            bool faulted = false;
            await connection.Start().ContinueWith(t => faulted = t.IsFaulted);
            return !faulted;
        }

        public void UpdateHeartbeatTimer(TimeSpan elapsedTime)
        {
            timeSinceHeartbeat += elapsedTime;
            if(timeSinceHeartbeat > heartbeatTimeout)
            {
                OnHeartbeatDeath?.Invoke();
            }
        }

        public void KillConnection()
        {
            connection.Stop();
        }

        public event Action OnHeartbeat;
        public event Action<Unit, ChangingData<Vector2>> OnPositionUpdate;
        public event Action<Unit, ChangingData<float>> OnHealthUpdate;
        public event Action<Unit, ChangingData<float>> OnRotationUpdate;
        public event Action<Unit> TriggerUnitDeath;
        public event Action<UnitSpawnData> SpawnUnit;

        private async void Beat()
        {
            timeSinceHeartbeat = TimeSpan.Zero;
            await hubProxy.Invoke("HeartbeatFromClient");
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

        private void KillUnit(Guid id)
        {
            Unit unit = getUnit(id);
            TriggerUnitDeath?.Invoke(unit);
        }

        private void AddUnit(UnitSpawnData spawnInfo)
        {
            SpawnUnit?.Invoke(spawnInfo);
        }
    }
}
