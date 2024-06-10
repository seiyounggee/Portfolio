using Photon.Deterministic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantum.Custom
{
    public unsafe static class CommonDefine
    {
        public enum PhysicsColliderUserTag
        { 
            None = 0,
            Player = 1,
            Ball = 2,
        }

        public static FPVector3[] SpawnPositionArray =   
        { 
            new FPVector3(20, 0, 20),
            new FPVector3(-20, 0, 20),
            new FPVector3(20, 0, -20),
            new FPVector3(-20, 0, -20),

            new FPVector3(0, 0, 20),
            new FPVector3(0, 0, -20),
            new FPVector3(20, 0, 0),
            new FPVector3(-20, 0, 0),
        };

        public static FP ROOM_OUT_OF_SIZE = 80; //한 변의 길이
        public static FP ROOM_OUT_OF_SIZE_HALF = ROOM_OUT_OF_SIZE / 2;  //한 변의 길이의 절반 

        public static FP ROOM_OUT_OF_BOUNDARY_XYZ = 90;
        public static FP ROOM_OUT_OF_BOUNDARY_XYZ_HALF = ROOM_OUT_OF_BOUNDARY_XYZ / 2;

        public static FPVector3 RandomSpawnPosition(Frame f)
        {
            if (f == null)
                return FPVector3.Zero;

            var random = f.RNG->Next(-20, 20);
            var random2 = f.RNG->Next(-20, 20);
            var pos = new FPVector3(random, 0, random2);

            return pos;
        }
    }
}
