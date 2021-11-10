namespace ArknightSimulator.Enemy
{
    public interface IEnemyStatus
    {
        int Attack { get; }
        int AttackSpeed { get; }
        float AttackTime { get; }
        float Range { get; }
        int Count { get; }
        int Defence { get; }
        int MaxLife { get; }
        int CurrentLife { get; }
        int MagicDefence { get; }
        int RecoverSpeed { get; }
        int Weight { get; }
        bool SilenceDefence { get; }
        bool DizzyDefence { get; }
        bool SleepDefence { get; }
        float MoveSpeed { get; }
    }
}