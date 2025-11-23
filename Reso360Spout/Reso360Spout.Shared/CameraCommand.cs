using Renderite.Shared;

namespace Reso360Spout.Shared
{
    public enum CameraCommandType
    {
        UpdateTransform,
        Initialize,
        Shutdown
    }

    public class CameraCommand : RendererCommand
    {
        public CameraCommandType Type;
        public float OriginX;
        public float OriginY;
        public float OriginZ;
        public float RotationX;
        public float RotationY;
        public float RotationZ;
        public float RotationW;
        public float ScaleX;
        public float ScaleY;
        public float ScaleZ;

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(Type);
            packer.Write(OriginX);
            packer.Write(OriginY);
            packer.Write(OriginZ);
            packer.Write(RotationX);
            packer.Write(RotationY);
            packer.Write(RotationZ);
            packer.Write(RotationW);
            packer.Write(ScaleX);
            packer.Write(ScaleY);
            packer.Write(ScaleZ);
        }

        public override void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.Read(ref Type);
            unpacker.Read(ref OriginX);
            unpacker.Read(ref OriginY);
            unpacker.Read(ref OriginZ);
            unpacker.Read(ref RotationX);
            unpacker.Read(ref RotationY);
            unpacker.Read(ref RotationZ);
            unpacker.Read(ref RotationW);
            unpacker.Read(ref ScaleX);
            unpacker.Read(ref ScaleY);
            unpacker.Read(ref ScaleZ);
        }
    }
}

