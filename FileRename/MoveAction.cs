using System;

namespace FileRename
{
    public class MoveAction : IEvent
    {
        public Action FinishedCallback { get; set; }

        public MoveAction(Action finishedCallback)
        {
            FinishedCallback = finishedCallback;
        }
    }
}

