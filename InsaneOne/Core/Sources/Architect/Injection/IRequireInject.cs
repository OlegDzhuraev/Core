namespace InsaneOne.Core.Injection
{
	/// <summary> Use this for MonoBehaviours which require data injection. They will receive injection info at scene load. </summary>
	public interface IRequireInject
	{
		// todo InsaneOne.Core : add event?
		//public static event Action<IRequestInject> InjectionRequested;
	}
}