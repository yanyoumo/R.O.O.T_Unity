using com.ootii.Messages;

namespace ROOT.Message
{
    public abstract class RootMessageBase : IMessage
    {
        public virtual string Type
        {
            get => "";
            set { }
        }

        public virtual float Delay
        {
            get => 0.0f;
            set { }
        }


        protected object mSender = null;

        public object Sender
        {
            get => mSender;
            set => mSender = value;
        }

        protected object mRecipient = null;

        public object Recipient
        {
            get => mRecipient;
            set => mRecipient = value;
        }

        protected int mID = 0;

        public int ID
        {
            get => mID;
            set => mID = value;
        }

        protected object mData = null;

        public object Data
        {
            get => mData;
            set => mData = value;
        }

        protected bool mIsSent = false;

        public bool IsSent
        {
            get => mIsSent;
            set => mIsSent = value;
        }

        protected bool mIsHandled = false;

        public bool IsHandled
        {
            get => mIsHandled;
            set => mIsHandled = value;
        }

        protected int mFrameIndex = 0;

        public int FrameIndex
        {
            get => mFrameIndex;
            set => mFrameIndex = value;
        }

        public virtual void Clear()
        {
            Type = "";
            mSender = null;
            mRecipient = null;
            mID = 0;
            mData = null;
            mIsSent = false;
            mIsHandled = false;
            Delay = 0.0f;
        }

        public virtual void Release()
        {
            Clear();
            IsSent = true;
            IsHandled = true;
            //sPool.Release(this);
        }
    }
}

