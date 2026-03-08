namespace UniFramework.Runtime
{
    public interface IUIRoot
    {
        T LoadUIPanel<T>() where T : UIPanel;
    }
}