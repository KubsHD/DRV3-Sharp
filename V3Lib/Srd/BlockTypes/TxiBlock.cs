﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace V3Lib.Srd.BlockTypes
{
    public sealed class TxiBlock : Block
    {
        public int Unknown10;
        public int Unknown14;
        public int Unknown18;
        public int Unknown1C;
        public int Unknown20;
        public string TextureFilename;

        public override void DeserializeData(byte[] rawData)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(rawData));

            Unknown10 = reader.ReadInt32();
            Unknown14 = reader.ReadInt32();
            Unknown18 = reader.ReadInt32();
            Unknown1C = reader.ReadInt32();
            Unknown20 = reader.ReadInt32();
            TextureFilename = Utils.ReadNullTerminatedString(ref reader, Encoding.GetEncoding("shift-jis"));

            reader.Close();
            reader.Dispose();
        }

        public override byte[] SerializeData()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);

            writer.Write(Unknown10);
            writer.Write(Unknown14);
            writer.Write(Unknown18);
            writer.Write(Unknown1C);
            writer.Write(Unknown20);
            writer.Write(Encoding.GetEncoding("shift-jis").GetBytes(TextureFilename));
            writer.Write((byte)0);  // Null terminator

            byte[] result = ms.ToArray();
            writer.Close();
            writer.Dispose();
            return result;
        }

        public override string GetInfo()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"{nameof(Unknown10)}: {Unknown10}\n");
            sb.Append($"{nameof(Unknown14)}: {Unknown14}\n");
            sb.Append($"{nameof(Unknown18)}: {Unknown18}\n");
            sb.Append($"{nameof(Unknown1C)}: {Unknown1C}\n");
            sb.Append($"{nameof(Unknown20)}: {Unknown20}\n");
            sb.Append($"Texture Filename: {TextureFilename}");

            return sb.ToString();
        }
    }
}
