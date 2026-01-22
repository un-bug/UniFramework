namespace UniFramework.Runtime
{
    public class GameEntry
    {
        public static UIManager UI => UniFrameworkEntry.GetModule(() => UIManager.Instance);
        public static SceneManager Scene => UniFrameworkEntry.GetModule(() => SceneManager.Instance);
    }
}