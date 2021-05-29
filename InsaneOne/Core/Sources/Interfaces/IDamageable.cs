namespace InsaneOne.Core
{
	/// <summary>For game entities, which can be damaged. </summary>
	public interface IDamageable
	{
		void TakeDamage(float value);
	}
}