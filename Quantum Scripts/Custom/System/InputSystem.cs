using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantum.Custom
{
    public unsafe class InputSystem : SystemMainThreadFilter<InputSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef entity;
            public CharacterController3D* characterController;
            public Transform3D* transform3d;
            public PhysicsBody3D* physicsBody3d;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            Input input = default;

            if (f.Unsafe.TryGetPointer(filter.entity, out PlayerLink* playerLink))
            {
                input = *f.GetPlayerInput(playerLink->PlayerRef);

                if (f.Unsafe.TryGetPointer<PlayerRules>(filter.entity, out var playerRules))
                {
                    playerRules->Update_Movement(f, input, filter);
                }
            }
        }
    }
}
