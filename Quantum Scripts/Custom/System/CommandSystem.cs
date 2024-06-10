using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantum.Custom
{
    public class CommandSystem : SystemMainThread
    {
        public override void Update(Frame f)
        {
            for (int i = 0; i < f.PlayerCount; i++)
            {
                var command = f.GetPlayerCommand(i);
                if (command != null)
                {
                    if (command.CommandType == typeof(CommandShareData))
                    {
                        CommandShareData action = command as CommandShareData;
                        action.Execute(f);
                    }

                    if (command.CommandType == typeof(CommandCheat))
                    {
                        CommandCheat action = command as CommandCheat;
                        action.Execute(f);
                    }
                }
            }
        }
    }
}
