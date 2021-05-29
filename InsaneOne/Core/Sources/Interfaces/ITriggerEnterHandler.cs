using UnityEngine;

namespace InsaneOne.Core
{
    /// <summary>Used to handle OnTrigger* event in separated class. </summary>
    public interface ITriggerEnterHandler
    {
        void TriggerEnterAction(Collider entered);
    }
}