using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantum
{
    public unsafe partial struct GameManager
    {
        private void Update_MapObject(Frame f)
        {
            var list_mapObj = f.ResolveList(ListOfMapObject);

            foreach (var entity in list_mapObj)
            {
                if (f.Unsafe.TryGetPointer<MapObjectRules>(entity, out MapObjectRules* mo))
                {
                    mo->Update(f);
                }
            }
        }
    }
}
