using glTFLoader;
using System.IO;

namespace glTF
{
    internal class Animation
    {
        public static void NormalizeTimes(string filePath)
        {
            var model = Interface.LoadModel(filePath);

            byte[] buffer = null;
            if (model.Buffers != null && string.IsNullOrEmpty(model.Buffers[0].Uri))
            {
                var binBuffer = model.Buffers[0];
                buffer = model.LoadBinaryBuffer(0, filePath);
            }

            foreach (var animation in model.Animations)
            {
                foreach (var sampler in animation.Samplers)
                {
                    var accessor = model.Accessors[sampler.Input];
                    var bufferView = model.BufferViews[accessor.BufferView.Value];
                    float earliestTime = float.NaN;
                    using (var memoryStream = new MemoryStream(buffer, bufferView.ByteOffset, bufferView.ByteLength))
                    {
                        using (var binaryReader = new BinaryReader(memoryStream))
                        using (var binaryWriter = new BinaryWriter(memoryStream))
                        {
                            while (memoryStream.Position < memoryStream.Length)
                            {
                                var position = memoryStream.Position;
                                var time = binaryReader.ReadSingle();
                                if (float.IsNaN(earliestTime))
                                {
                                    earliestTime = time;
                                }

                                memoryStream.Seek(position, SeekOrigin.Begin);
                                binaryWriter.Write(time - earliestTime);
                            }
                        }
                    }
                    accessor.Min[0] -= earliestTime;
                    accessor.Max[0] -= earliestTime;
                }
            }

            Interface.SaveBinaryModel(model, buffer, filePath);
        }
    }
}
