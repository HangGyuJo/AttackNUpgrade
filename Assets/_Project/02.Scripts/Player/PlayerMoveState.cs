using System;
using Unity.Netcode;
using UnityEngine;

public struct PlayerMoveState : INetworkSerializable, IEquatable<PlayerMoveState>
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float ServerTime;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Position);
        serializer.SerializeValue(ref Velocity);
        serializer.SerializeValue(ref ServerTime);
    }

    public bool Equals(PlayerMoveState other)
    {
        return Position.Equals(other.Position)
               && Velocity.Equals(other.Velocity)
               && ServerTime.Equals(other.ServerTime);
    }
}