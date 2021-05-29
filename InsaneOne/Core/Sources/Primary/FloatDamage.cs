namespace InsaneOne.Core
{
    public class FloatDamage : IDamage
    {
        readonly float value;
        
        public FloatDamage(float value)
        {
            this.value = value;
        }

        public float GetValue() => value;

        public static implicit operator float(FloatDamage damage) => damage.GetValue();
        public static implicit operator FloatDamage(float damageValue) => new FloatDamage(damageValue);
    }
}