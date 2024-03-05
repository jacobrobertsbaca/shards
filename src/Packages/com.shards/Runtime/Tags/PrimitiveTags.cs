using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Shards.Tags
{
    [Tag(TagType.Null)]
    public struct NullTag : ITag
    {
        public void Read(BinaryReader reader) {}
        public void Write(BinaryWriter writer) {}
    }

    [Tag(TagType.String)]
    public struct StringTag : IValueTag<string>
    {
        public string Value { get; set; }
        public StringTag(string value) => Value = value;
        public void Read(BinaryReader reader) => Value = reader.ReadString();
        public void Write(BinaryWriter writer) => writer.Write(Value);
    }

    [Tag(TagType.Byte)]
    public struct ByteTag : IValueTag<byte>
    {
        public byte Value { get; set; }
        public ByteTag(byte value) => Value = value;
        public void Read(BinaryReader reader) => Value = reader.ReadByte();
        public void Write(BinaryWriter writer) => writer.Write(Value);
    }

    [Tag(TagType.Int16)]
    public struct Int16Tag : IValueTag<short>
    {
        public short Value { get; set; }
        public Int16Tag(short value) => Value = value;
        public void Read(BinaryReader reader) => Value = reader.ReadInt16();
        public void Write(BinaryWriter writer) => writer.Write(Value);
    }

    [Tag(TagType.Int32)]
    public struct Int32Tag : IValueTag<int>
    {
        public int Value { get; set; }
        public Int32Tag(int value) => Value = value;
        public void Read(BinaryReader reader) => Value = reader.ReadInt32();
        public void Write(BinaryWriter writer) => writer.Write(Value);
    }

    [Tag(TagType.Int64)]
    public struct Int64Tag : IValueTag<long>
    {
        public long Value { get; set; }
        public Int64Tag(long value) => Value = value;
        public void Read(BinaryReader reader) => Value = reader.ReadInt64();
        public void Write(BinaryWriter writer) => writer.Write(Value);
    }

    [Tag(TagType.Float)]
    public struct FloatTag : IValueTag<float>
    {
        public float Value { get; set; }
        public FloatTag(float value) => Value = value;
        public void Read(BinaryReader reader) => Value = reader.ReadSingle();
        public void Write(BinaryWriter writer) => writer.Write(Value);
    }

    [Tag(TagType.Double)]
    public struct DoubleTag : IValueTag<double>
    {
        public double Value { get; set; }
        public DoubleTag(double value) => Value = value;
        public void Read(BinaryReader reader) => Value = reader.ReadSingle();
        public void Write(BinaryWriter writer) => writer.Write(Value);
    }

    [Tag(TagType.Bool)]
    public struct BoolTag : IValueTag<bool>
    {
        public bool Value { get; set; }
        public BoolTag(bool value) => Value = value;
        public void Read(BinaryReader reader) => Value = reader.ReadBoolean();
        public void Write(BinaryWriter writer) => writer.Write(Value);
    }
}