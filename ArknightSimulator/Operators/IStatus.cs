namespace ArknightSimulator.Operators
{
    public interface IStatus
    {
        int Attack { get; }
        float AttackTime { get; }
        int Block { get; }
        int[] Cost { get;}
        int CurrentLife { get; }
        int Defence { get; }
        int MagicDefence { get; }
        int MaxLife { get; }
        int[][][] Range { get; }
        int SkillPoint { get; }
        int Time { get;}
    }
}