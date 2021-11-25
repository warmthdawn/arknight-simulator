namespace ArknightSimulator.Operators
{
    public interface IStatus
    {
        int MaxLife { get; }
        int CurrentLife { get; }
        int SkillPoint { get; set; }  // 技力需要更新
        int SkillPointUnit { get; set; } // 技力单元需要更新
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