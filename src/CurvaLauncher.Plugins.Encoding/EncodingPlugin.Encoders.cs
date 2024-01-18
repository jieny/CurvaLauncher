﻿using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Text = System.Buffers.Text;

namespace CurvaLauncher.Plugins.Encoding;

public partial class EncodingPlugin
{
    public class Encoders
    {
        private readonly EncodingPlugin _plugin;

        public Encoders(EncodingPlugin plugin)
        {
            _plugin = plugin;
        }

        public async Task Base64(Stream sourceStream, Stream destStream, CancellationToken cancellationToken)
        {
            int bufferSize = _plugin.BufferSize; // 网络流不支持读取steam.Length
            byte[] buffer = new byte[bufferSize];
            byte[] writeBuffer = new byte[Text::Base64.GetMaxEncodedToUtf8Length(bufferSize)];

            while (!cancellationToken.IsCancellationRequested)
            {
                int readLen = await sourceStream.ReadAsync(buffer.AsMemory(0, bufferSize), cancellationToken);
                if (readLen == 0)
                    break;

                Text::Base64.EncodeToUtf8(buffer, writeBuffer, out _, out var outputLen);
                destStream.Write(writeBuffer, 0, outputLen);
            }
        }

        private static ReadOnlySpan<ushort> HexChars => [
            0x3030, 0x3130, 0x3230, 0x3330, 0x3430, 0x3530, 0x3630, 0x3730, 0x3830, 0x3930, 0x4130, 0x4230, 0x4330, 0x4430, 0x4530, 0x4630, 
            0x3031, 0x3131, 0x3231, 0x3331, 0x3431, 0x3531, 0x3631, 0x3731, 0x3831, 0x3931, 0x4131, 0x4231, 0x4331, 0x4431, 0x4531, 0x4631, 
            0x3032, 0x3132, 0x3232, 0x3332, 0x3432, 0x3532, 0x3632, 0x3732, 0x3832, 0x3932, 0x4132, 0x4232, 0x4332, 0x4432, 0x4532, 0x4632, 
            0x3033, 0x3133, 0x3233, 0x3333, 0x3433, 0x3533, 0x3633, 0x3733, 0x3833, 0x3933, 0x4133, 0x4233, 0x4333, 0x4433, 0x4533, 0x4633,
            0x3034, 0x3134, 0x3234, 0x3334, 0x3434, 0x3534, 0x3634, 0x3734, 0x3834, 0x3934, 0x4134, 0x4234, 0x4334, 0x4434, 0x4534, 0x4634, 
            0x3035, 0x3135, 0x3235, 0x3335, 0x3435, 0x3535, 0x3635, 0x3735, 0x3835, 0x3935, 0x4135, 0x4235, 0x4335, 0x4435, 0x4535, 0x4635, 
            0x3036, 0x3136, 0x3236, 0x3336, 0x3436, 0x3536, 0x3636, 0x3736, 0x3836, 0x3936, 0x4136, 0x4236, 0x4336, 0x4436, 0x4536, 0x4636, 
            0x3037, 0x3137, 0x3237, 0x3337, 0x3437, 0x3537, 0x3637, 0x3737, 0x3837, 0x3937, 0x4137, 0x4237, 0x4337, 0x4437, 0x4537, 0x4637, 
            0x3038, 0x3138, 0x3238, 0x3338, 0x3438, 0x3538, 0x3638, 0x3738, 0x3838, 0x3938, 0x4138, 0x4238, 0x4338, 0x4438, 0x4538, 0x4638, 
            0x3039, 0x3139, 0x3239, 0x3339, 0x3439, 0x3539, 0x3639, 0x3739, 0x3839, 0x3939, 0x4139, 0x4239, 0x4339, 0x4439, 0x4539, 0x4639, 
            0x3041, 0x3141, 0x3241, 0x3341, 0x3441, 0x3541, 0x3641, 0x3741, 0x3841, 0x3941, 0x4141, 0x4241, 0x4341, 0x4441, 0x4541, 0x4641, 
            0x3042, 0x3142, 0x3242, 0x3342, 0x3442, 0x3542, 0x3642, 0x3742, 0x3842, 0x3942, 0x4142, 0x4242, 0x4342, 0x4442, 0x4542, 0x4642, 
            0x3043, 0x3143, 0x3243, 0x3343, 0x3443, 0x3543, 0x3643, 0x3743, 0x3843, 0x3943, 0x4143, 0x4243, 0x4343, 0x4443, 0x4543, 0x4643, 
            0x3044, 0x3144, 0x3244, 0x3344, 0x3444, 0x3544, 0x3644, 0x3744, 0x3844, 0x3944, 0x4144, 0x4244, 0x4344, 0x4444, 0x4544, 0x4644, 
            0x3045, 0x3145, 0x3245, 0x3345, 0x3445, 0x3545, 0x3645, 0x3745, 0x3845, 0x3945, 0x4145, 0x4245, 0x4345, 0x4445, 0x4545, 0x4645,
            0x3046, 0x3146, 0x3246, 0x3346, 0x3446, 0x3546, 0x3646, 0x3746, 0x3846, 0x3946, 0x4146, 0x4246, 0x4346, 0x4446, 0x4546, 0x4646];

        public async Task Hex(Stream sourceStream, Stream destStream, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[_plugin.BufferSize];
            byte[] outputBuffer = new byte[_plugin.BufferSize * 2];

            while (await sourceStream.ReadAsync(buffer, cancellationToken) is int len && len > 0)
            {
                int i = 0;
                while (i + 3 < len)
                {
                    Unsafe.As<byte, ushort>(ref Unsafe.Add(ref outputBuffer[0], i * 2 + 0)) = HexChars[Unsafe.Add(ref buffer[0], i + 0)];
                    Unsafe.As<byte, ushort>(ref Unsafe.Add(ref outputBuffer[0], i * 2 + 2)) = HexChars[Unsafe.Add(ref buffer[0], i + 1)];
                    Unsafe.As<byte, ushort>(ref Unsafe.Add(ref outputBuffer[0], i * 2 + 4)) = HexChars[Unsafe.Add(ref buffer[0], i + 2)];
                    Unsafe.As<byte, ushort>(ref Unsafe.Add(ref outputBuffer[0], i * 2 + 6)) = HexChars[Unsafe.Add(ref buffer[0], i + 3)];
                    i += 4;
                }
                for (; i < len; i++)
                {
                    Unsafe.As<byte, ushort>(ref Unsafe.Add(ref outputBuffer[0], i * 2 + 0)) = HexChars[Unsafe.Add(ref buffer[0], i + 0)];
                }

                await destStream.WriteAsync(outputBuffer.AsMemory(0, len * 2), cancellationToken);
            }
        }

        public async Task Html(Stream sourceStream, Stream destStream, CancellationToken cancellationToken)
        {
            var originLength = _plugin.BufferSize;
            string buffer = new('\0', originLength);
            static void SetLength(string str, int len) => Unsafe.Add(ref Unsafe.As<char, int>(ref Unsafe.AsRef(in str.GetPinnableReference())), -1) = len;

            StreamReader sr = new(sourceStream);
            StreamWriter sw = new(destStream);

            try
            {
                while (true)
                {
                    SetLength(buffer, originLength);
                    var inMemory = buffer.AsMemory();
                    int readlen = await sr.ReadBlockAsync(Unsafe.As<ReadOnlyMemory<char>, Memory<char>>(ref inMemory), cancellationToken); // ReadOnlyMemory和Memory的内部结构相同
                    if (readlen == 0)
                        break;

                    SetLength(buffer, readlen);
                    WebUtility.HtmlEncode(buffer, sw);
                }
            }
            finally
            {
                sw.Flush();
                SetLength(buffer, originLength);
            }
        }
    }
}
