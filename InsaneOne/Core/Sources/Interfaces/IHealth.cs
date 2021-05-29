namespace InsaneOne.Core
{
    /// <summary> For game entities, which have health. </summary>
    public interface IHealth
    {
        float GetHealthPercents();

        void SetHealthPercents(float percents);
        void SetHealth(float value);
    }
}