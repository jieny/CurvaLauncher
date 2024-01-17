﻿using System.IO;

namespace CurvaLauncher.Plugins.Encoding
{
    public partial class EncodingPlugin
    {
        public class Decoders
        {
            private readonly EncodingPlugin _plugin;

            public Decoders(EncodingPlugin plugin)
            {
                _plugin = plugin;
            }

            public async Task Base64(Stream sourceStream, Stream destStream, CancellationToken cancellationToken)
            {
                int bufferSize = Math.Min((int)sourceStream.Length, _plugin.BufferSize);
                byte[] buffer = new byte[bufferSize];
                byte[] outputBuffer = new byte[System.Buffers.Text.Base64.GetMaxDecodedFromUtf8Length(bufferSize)];

                long totalRead = 0;

                while (!cancellationToken.IsCancellationRequested)
                {
                    int readCount = await sourceStream.ReadAsync(buffer, 0, buffer.Length);
                    totalRead += readCount;

                    bool finalBlock = readCount < bufferSize || totalRead == sourceStream.Length;

                    var inputSpan = new Memory<byte>(buffer, 0, readCount);
                    var outputSpan = new Memory<byte>(outputBuffer, 0, outputBuffer.Length);
                    var status = System.Buffers.Text.Base64.DecodeFromUtf8(inputSpan.Span, outputSpan.Span, out int bytesCosumed, out int bytesWritten, finalBlock);

                    destStream.Write(outputBuffer, 0, bytesWritten);

                    if (finalBlock)
                        break;
                }
            }
        }
    }
}
