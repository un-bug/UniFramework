namespace UniFramework.Runtime
{
    public class GameEntry
    {
        public static AudioManager Audio => UniFrameworkEntry.GetModule(() => AudioManager.Instance);
        public static SceneManager Scene => UniFrameworkEntry.GetModule(() => SceneManager.Instance);
        public static UIManager UI => UniFrameworkEntry.GetModule(() => UIManager.Instance);
    }
}