namespace ArknightSimulator.Enemy
{
    public interface IEnemyStatus
    {
        int Attack { get; }
        int AttackSpeed { get; }
        float AttackTime { get; }
        int Count { get; }
        int CurrentLife { get; }
        int Defence { get; }
        bool DizzyDefence { get; }
        int MagicDefense { get; }
        int MaxLife { get; }
        float MoveSpeed { get; }
        float Range { get; }
        int RecoverSpeed { get; }
        bool SilenceDefence { get; }
        bool SleepDefence { get; }
        int Weight { get; }
    }
}