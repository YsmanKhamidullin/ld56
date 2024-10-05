public static class Game
{
    public static bool IsPause { get; set; }
    public static bool IsByInputPause { get; set; }
    public static float MusicValue { get; set; } = 0.5f;
    public static float SoundValue { get; set; } = 0.5f;
    public static int AttackNotEnemy { get; set; }
    public static int RestartAfterComplete { get; set; }
    public static Main.Difficulty Difficulty { get; set; }
}