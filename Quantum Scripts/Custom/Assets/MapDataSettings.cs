using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.Deterministic;
using Quantum.Core;
using Quantum.Inspector;

namespace Quantum
{
    public unsafe partial class CustomMapDataSettings
    {
        [Serializable]
        public struct SpawnPointData
        {
            public FPVector3 Position;

            [Degrees]
            public FPQuaternion Rotation;
        }

        public SpawnPointData DefaultSpawnPoint;
        public SpawnPointData[] SpawnPoints;

        public (FPVector3, FPQuaternion) GetSpawnPoint(Frame f, Int32? index)
        {
            var spawnPoint = index.HasValue && index.Value < SpawnPoints.Length ? SpawnPoints[index.Value] : DefaultSpawnPoint;

            return (spawnPoint.Position, spawnPoint.Rotation);
        }

        public void SuffleSpawnPoint(Frame f)
        {
            int n = SpawnPoints.Length;

            for (int i = 0; i < n; i++)
            {
                // i부터 n-1까지의 랜덤한 인덱스를 선택합니다.
                f.Global->RngSession = new RNGSession(n);
                int index = f.RNG->Next(i, n);

                // 배열 요소를 교환합니다.
                SpawnPointData temp = SpawnPoints[i];
                SpawnPoints[i] = SpawnPoints[index];
                SpawnPoints[index] = temp;
            }
        }
    }
}
