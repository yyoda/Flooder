using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Flooder.Utility
{
    public class FileUtility
    {
        /// <summary>
        /// 指定フォルダから最新のファイルを一つ取り、ファイルのフルパス、最終書き込み日時を返す
        /// </summary>
        /// <param name="filePath">指定フォルダ</param>
        /// <param name="latestFilePath">フルパス</param>
        /// <param name="lastWriteTime">最終書き込み日時</param>
        /// <returns>ファイルが存在しなかったらfalse、そうでなければtrue</returns>
        public static bool TryGetLatestFile(string filePath, out string latestFilePath, out DateTime lastWriteTime)
        {
            string tmpLatestFilePath = null;
            var tmpLastWriteTime = new DateTime();
            var result = false;

            foreach (var path in Directory.GetFiles(filePath))
            {
                var nowTime = File.GetLastWriteTime(path);
                if (tmpLastWriteTime < nowTime)
                {
                    tmpLastWriteTime = nowTime;
                    tmpLatestFilePath = path;
                    result = true;
                }
            }

            latestFilePath = tmpLatestFilePath;
            lastWriteTime = tmpLastWriteTime;
            return result;
        }

        public static IEnumerable<Exception> RemoveFile(string filePath, int retryCount = 3, int interval = 1000)
        {
            for (var i = 0; i < retryCount; i++)
            {
                Exception exception;

                try
                {
                    Thread.Sleep(interval);
                    File.Delete(filePath);
                    yield break;

                }
                catch (Exception e)
                {
                    exception = e;
                }

                yield return exception;
            }
        }
    }
}
