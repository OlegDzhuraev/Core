using UnityEngine;

namespace InsaneOne.Core
{
    /// <summary>Used to handle OnTrigger* event in separated class. </summary>
    public interface ITrigger2DEnterHandler
    {
        void Trigger2DEnterAction(Collider2D entered);
    }
}