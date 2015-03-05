
namespace Flooder.Core.Settings
{
    public interface ISettingsCreator
    {
        Settings Create();
    }

    public class SettingsFactory
    {
        public static Settings Create<T>() where T : ISettingsCreator, new()
        {
            return new T().Create();
        }
    }
}
