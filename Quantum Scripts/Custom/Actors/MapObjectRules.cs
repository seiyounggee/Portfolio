using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct MapObjectRules
    {
        public unsafe void Update(Frame f)
        {
            switch (ObjectType)
            {
                case MapObjectType.Default:
                    { 
                    
                    }
                    break;

                case MapObjectType.StaticWall:
                    { 
                    
                    }
                    break;

                case MapObjectType.StaticPillar:
                    { 
                    
                    }
                    break;

                case MapObjectType.MovingPillar:
                    { 
                    
                    }
                    break;
            }
        }
    }
}
