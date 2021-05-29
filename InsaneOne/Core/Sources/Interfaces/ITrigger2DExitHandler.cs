using UnityEngine;

namespace InsaneOne.Core
{
    /// <summary>Used to handle OnTrigger* event in separated class. </summary>
    public interface ITrigger2DExitHandler
    {
        void Trigger2DExitAction(Collider2D exited);
    }
}