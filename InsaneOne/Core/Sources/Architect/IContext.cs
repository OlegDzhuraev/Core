namespace InsaneOne.Core.Architect
{
	/// <summary> Use this for your MonoBehaviour, if it should receive some game context. </summary>
	public interface IContext<in T>
	{
		public void ReloadContext(T newContext) => OnContextReloaded(newContext);
		public void OnContextReloaded(T newContext) { }
	}
}