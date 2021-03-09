using UnityEngine;

namespace FirstPersonCamera
{
    public interface ICameraInput
    {
        Vector2 LookInputVector { get; }
    }
}