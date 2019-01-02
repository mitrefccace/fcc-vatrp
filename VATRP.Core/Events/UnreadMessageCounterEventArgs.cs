using System;

namespace Mediate.Console.Messenger.Managers.DataContainers
{
    public class UnreadMessageCounterEventArgs : EventArgs
    {
        public UnreadMessageCounterEventArgs(uint counter)
        {
            this.Counter = counter;
        }

        public uint Counter { get; private set; }
    }
}

