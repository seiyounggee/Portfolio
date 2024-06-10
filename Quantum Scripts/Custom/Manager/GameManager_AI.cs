using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantum
{
    public unsafe partial struct GameManager
    {
        private void Update_AI(Frame f)
        {
            var list_ai = f.ResolveList(ListOfPlayers_AI);

            foreach (var entity in list_ai)
            {
                if (f.Unsafe.TryGetPointer<AIPlayerRules>(entity, out AIPlayerRules* ai))
                {
                    ai->Update_AI_Inputs(f);
                    ai->Update_Raycast(f);
                }
            }
        }
    }
}
