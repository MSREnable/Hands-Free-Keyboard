using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Research.SpeechWriter.Core.Database
{
    internal class BinaryDatabaseHelper
    {
        private static void WriteLevel(CompressedIntWriter writer, TokenPredictorDatabase database, Dictionary<int, int> nativeToTransferToken)
        {
            writer.Write(database.Count);
            Debug.WriteLine(database.Count);

            foreach (var info in database.SortedEnumerable)
            {
                var nativeToken = info.Token;
                var transferToken = nativeToTransferToken[nativeToken];
                writer.Write(transferToken);
                writer.Write(info.Count);

                Debug.Write($"{transferToken}={info.Count}=>");
                var children = info.TryGetChildren();
                if (children != null)
                {
                    Debug.Indent();
                    WriteLevel(writer, children, nativeToTransferToken);
                    Debug.Unindent();
                }
                else
                {
                    writer.Write(0);
                    Debug.WriteLine(0);
                }
            }
        }

        public static void SaveDatabase(TokenPredictorDatabase database, StringTokens tokens)
        {
            var stream = new MemoryStream();
            var writer = new CompressedIntWriter(stream);

            var transferToNativeToken = new List<int>(database.Count);
            var nativeToTransferToken = new Dictionary<int, int>(database.Count);
            foreach (var info in database.SortedEnumerable)
            {
                nativeToTransferToken.Add(info.Token, transferToNativeToken.Count);
                transferToNativeToken.Add(info.Token);
            }

            // Write number of tokens.
            writer.Write(transferToNativeToken.Count);
            Debug.WriteLine($"tokenCount={transferToNativeToken.Count}");

            // Write the tokens.
            foreach (var nativeToken in transferToNativeToken)
            {
                var text = tokens.GetString(nativeToken);
                writer.Write(text);
                Debug.WriteLine(text);
            }

            WriteLevel(writer, database, nativeToTransferToken);

            Debug.WriteLine(string.Empty);
            Debug.WriteLine(string.Empty);
            Debug.WriteLine("***********");
            Debug.WriteLine(string.Empty);
            Debug.WriteLine(string.Empty);

            stream.Position = 0;
            LoadDatabase(stream, tokens);
        }

        private static void ReadLevel(CompressedIntReader reader, List<int> transferToNativeToken)
        {
            var count = reader.ReadInt();
            Debug.WriteLine(count);

            for (var i = 0; i < count; i++)
            {
                var transferToken = reader.ReadInt();
                var tokenCount = reader.ReadInt();
                Debug.WriteLine($"{transferToken}={tokenCount}=>");

                Debug.Indent();
                ReadLevel(reader, transferToNativeToken);
                Debug.Unindent();
            }
        }

        public static void LoadDatabase(Stream stream, StringTokens tokens)
        {
            var reader = new CompressedIntReader(stream);

            // Read number of tokens.
            var transferTokenCount = reader.ReadInt();
            Debug.WriteLine($"tokenCount={transferTokenCount}");

            // Read the tokens.
            var transferToNativeToken = new List<int>(transferTokenCount);
            for (var transferToken = 0; transferToken < transferTokenCount; transferToken++)
            {
                var text = reader.ReadString();
                var nativeToken = tokens.GetToken(text);
                transferToNativeToken.Add(nativeToken);
                Debug.WriteLine(text);
            }

            ReadLevel(reader, transferToNativeToken);
        }
    }
}
