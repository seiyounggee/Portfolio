using Quantum.Core;
using Quantum.Physics3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantum.Custom
{
    public unsafe class CollisionSystem : SystemSignalsOnly, ISignalOnCollisionEnter3D, ISignalOnCollisionExit3D, ISignalOnTrigger3D, ISignalOnTriggerEnter3D,
        ISignalOnTriggerExit3D, IKCCCallbacks3D
    {
        public bool OnCharacterCollision3D(FrameBase f, EntityRef character, Hit3D hit)
        {
            if (f.Unsafe.TryGetPointer<PlayerRules>(character, out PlayerRules* pr))
            {
                pr->OnCharacterCollision3D(f, character, hit);
                return true;
            }

            return false;
        }

        public void OnCharacterTrigger3D(FrameBase f, EntityRef character, Hit3D hit)
        {
            if (f.Unsafe.TryGetPointer<PlayerRules>(character, out PlayerRules* pr))
            {
                pr->OnCharacterTrigger3D(f, character, hit);
            }
        }

        public void OnCollisionEnter3D(Frame f, CollisionInfo3D info)
        {
            if (info.EntityShapeUserTag == (int)CommonDefine.PhysicsColliderUserTag.Player)
            {
                if (f.Unsafe.TryGetPointer<PlayerRules>(info.Entity, out PlayerRules* pr))
                {
                    pr->OnCollisionEnter3D(f, info);
                }
            }
        }

        public void OnCollisionExit3D(Frame f, ExitInfo3D info)
        {
            if (f.Unsafe.TryGetPointer<PlayerRules>(info.Entity, out PlayerRules* pr))
            {
                pr->OnCollisionExit3D(f, info);
            }
        }

        public void OnTrigger3D(Frame f, TriggerInfo3D info)
        {
            if (info.EntityShapeUserTag == (int)CommonDefine.PhysicsColliderUserTag.Player)
            {
                if (f.Unsafe.TryGetPointer<PlayerRules>(info.Entity, out PlayerRules* pr))
                {
                    pr->OnTrigger3D(f, info);
                }
            }
        }

        public void OnTriggerEnter3D(Frame f, TriggerInfo3D info)
        {
            if (info.EntityShapeUserTag == (int)CommonDefine.PhysicsColliderUserTag.Player)
            {
                if (f.Unsafe.TryGetPointer<PlayerRules>(info.Entity, out PlayerRules* pr))
                {
                    pr->OnTriggerEnter3D(f, info);
                }
            }
        }

        public void OnTriggerExit3D(Frame f, ExitInfo3D info)
        {
            if (f.Unsafe.TryGetPointer<PlayerRules>(info.Entity, out PlayerRules* pr))
            {
                pr->OnTriggerExit3D(f, info);
            }
        }
    }
}
