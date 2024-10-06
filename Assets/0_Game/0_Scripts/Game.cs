public static class Game
{
    public static bool IsPause { get; set; }
    public static bool IsByInputPause { get; set; }
    public static float MusicValue { get; set; } = 0.05f;
    public static float SoundValue { get; set; } = 0.2f;
    public static int AttackNotEnemy { get; set; }
    public static int RestartAfterComplete { get; set; }
    public static Difficulty Difficulty { get; set; }
    public static int UpgradeSize { get; set; }
    public static int UpgradeAttack { get; set; }
    // public static int UpgradeHealth { get; set; }
    public static int UpgradeAttackRange { get; set; }
    public static int UpgradeDash { get; set; }
    public static int UpgradeSpeed { get; set; }
}