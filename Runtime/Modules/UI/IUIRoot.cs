namespace UniFramework.Runtime
{
    public interface IUIRoot
    {
        T GetUIPanel<T>() where T : UIPanel;
    }
}