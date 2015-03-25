using System;
using NLog;

namespace Flooder
{
    public class ServiceFactory
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static IFlooderService Create<T>() where T : IFlooderService, new()
        {
            try
            {
                return new T().Create();
            }
            catch (Exception ex)
            {
                Logger.FatalException("FlooderService create failue.", ex);
                throw;
            }
        }
    }
}
