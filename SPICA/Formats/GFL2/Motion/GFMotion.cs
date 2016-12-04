﻿using SPICA.Formats.CtrH3D.Animation;
using SPICA.Formats.GFL2.Model;
using SPICA.Math3D;

using System.Collections.Generic;
using System.IO;

namespace SPICA.Formats.GFL2.Motion
{
    public class GFMotion
    {
        private struct Section
        {
            public uint Count;
            public uint Length;
            public uint Address;
        }

        public Vector3D AnimRegionMin;
        public Vector3D AnimRegionMax;

        public uint FramesCount;

        public GFSkeletonMot SkeletalAnimation;
        public GFMaterialMot MaterialAnimation;
        public GFVisibilityMot VisibilityAnimation;

        public int Index;

        public GFMotion() { }

        public GFMotion(BinaryReader Reader, int Index)
        {
            this.Index = Index;

            long Position = Reader.BaseStream.Position;

            uint MagicNumber = Reader.ReadUInt32();
            uint SectionCount = Reader.ReadUInt32();

            Section[] AnimSections = new Section[SectionCount];

            for (int Anim = 0; Anim < AnimSections.Length; Anim++)
            {
                AnimSections[Anim] = new Section
                {
                    Count = Reader.ReadUInt32(), //TODO: Fix this, not a count it seems
                    Length = Reader.ReadUInt32(),
                    Address = Reader.ReadUInt32()
                };
            }

            //SubHeader
            Reader.BaseStream.Seek(Position + AnimSections[0].Address, SeekOrigin.Begin);

            FramesCount = Reader.ReadUInt32();

            Reader.ReadUInt32();

            AnimRegionMin = new Vector3D(Reader);
            AnimRegionMax = new Vector3D(Reader);

            uint AnimHash = Reader.ReadUInt32();

            //Content
            for (int Anim = 1; Anim < AnimSections.Length; Anim++)
            {
                Reader.BaseStream.Seek(Position + AnimSections[Anim].Address, SeekOrigin.Begin);

                switch (Anim)
                {
                    case 1: SkeletalAnimation = new GFSkeletonMot(Reader); break;
                    case 2: MaterialAnimation = new GFMaterialMot(Reader); break;
                    case 3: VisibilityAnimation = new GFVisibilityMot(Reader, FramesCount); break;
                }
            }
        }

        public H3DAnimation ToH3DSkeletalAnimation(List<GFBone> Skeleton)
        {
            return SkeletalAnimation?.ToH3DAnimation(Skeleton, FramesCount);
        }

        public H3DAnimation ToH3DMaterialAnimation()
        {
            return MaterialAnimation?.ToH3DAnimation(FramesCount);
        }

        public H3DAnimation ToH3DVisibilityAnimation()
        {
            return VisibilityAnimation?.ToH3DAnimation(FramesCount);
        }
    }
}
