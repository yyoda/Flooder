using System.ServiceProcess;

namespace Flooder.Service
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        static void Main()
        {
            ServiceBase.Run(new ServiceBase[] 
            { 
                new FlooderService() 
            });
        }
    }
}
