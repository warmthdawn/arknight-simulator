namespace ArknightSimulator.Operators
{
    public interface IStatus
    {
        int MaxLife { get; }
        int CurrentLife { get; }
        int SkillPoint { get; }
        int Attack { get; }
        int Defence { get; }
        int MagicDefence { get; }
        int Time { get; }
        int[] Cost { get; }
        int DeployCount { get; }
        int Block { get; }
        float AttackTime { get; }
        int[][][] Range { get; }
    }
}