using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrorDecor
{
    public class CustomUnlockable
    {
        public string name = "";

        public string unlockablePath = "";

        public string infoPath = "";

        public Action<UnlockableItem> unlockableAction = delegate (UnlockableItem item)
        {
        };

        public bool enabled = true;

        public int unlockCost = -1;

        public CustomUnlockable(string name, string unlockablePath, string infoPath, Action<UnlockableItem> action = null, int unlockCost = -1)
        {
            this.name = name;
            this.unlockablePath = unlockablePath;
            this.infoPath = infoPath;
            if (action != null)
            {
                this.unlockableAction = action;
            }
            this.unlockCost = unlockCost;
        }

        public static CustomUnlockable Add(string name, string unlockablePath, string infoPath = null, Action<UnlockableItem> action = null, int unlockCost = -1, bool enabled = true)
        {
            return new CustomUnlockable(name, unlockablePath, infoPath, action, unlockCost)
            {
                enabled = enabled
            };
        }
    }
}
