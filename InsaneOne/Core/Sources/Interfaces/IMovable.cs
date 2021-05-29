using UnityEngine;

namespace InsaneOne.Core
{
    /// <summary> For game entities which can move in specified direction. </summary>
    public interface IMovable
    {
        void Move(Vector3 direction);
    }
}