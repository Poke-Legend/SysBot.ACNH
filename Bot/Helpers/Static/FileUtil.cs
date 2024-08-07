﻿using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace SysBot.ACNHOrders
{
    public static class FileUtil
    {
        private const int MaxRetryAttempts = 5;
        private const int DelayBetweenRetries = 1000; // 1 second

        public static async Task WriteBytesToFileAsync(byte[] bytes, string path, CancellationToken token)
        {
            for (int attempt = 0; attempt < MaxRetryAttempts; attempt++)
            {
                try
                {
                    await using FileStream sourceStream = new(path,
                        FileMode.Create, FileAccess.Write, FileShare.None,
                        bufferSize: 4096, useAsync: true);
                    await sourceStream.WriteAsync(bytes, token).ConfigureAwait(false);
                    return; // Success, exit method
                }
                catch (IOException ex) when (attempt < MaxRetryAttempts - 1)
                {
                    Console.WriteLine($"Attempt {attempt + 1} failed: {ex.Message}. Retrying in {DelayBetweenRetries}ms...");
                    await Task.Delay(DelayBetweenRetries, token).ConfigureAwait(false);
                }
            }

            // If the code reaches here, all attempts have failed
            throw new IOException($"Failed to write to file '{path}' after {MaxRetryAttempts} attempts.");
        }

        public static string GetEmbeddedResource(string namespacename, string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            if (assembly == null)
                return string.Empty;

            var resourceName = namespacename + "." + filename;
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                return string.Empty;

            using StreamReader reader = new(stream);
            string result = reader.ReadToEnd();
            return result;
        }
    }
}
