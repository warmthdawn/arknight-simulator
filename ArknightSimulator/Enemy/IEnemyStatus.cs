namespace ArknightSimulator.Enemy
{
    public interface IEnemyStatus
    {
        int Attack { get; }
        int AttackSpeed { get; }
        float AttackTime { get; }
        float Range { get; }
        int Count { get; }
        int Defense { get; }
        int MaxLife { get; }
        int CurrentLife { get; }
        int MagicDefense { get; }
        int RecoverSpeed { get; }
        int Weight { get; }
        bool SilenceDefence { get; }
        bool DizzyDefense { get; }
        bool SleepDefence { get; }
        float MoveSpeed { get; }
    }
}