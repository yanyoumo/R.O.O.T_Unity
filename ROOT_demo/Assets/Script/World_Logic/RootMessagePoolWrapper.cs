using System;
using System.Collections;
using System.Collections.Generic;
using com.ootii.Collections;
using com.ootii.Messages;
using Rewired;
using UnityEngine;


namespace ROOT.Message
{
    public abstract class RootMessageBase : IMessage
    {
        public abstract string Type { get; set; }
        public abstract float Delay { get; set; }
        
        
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

    /*public class RootMessagePoolWrapper<T> where T : RootMessageBase, new()
    {
        // ******************************** OBJECT POOL ********************************
        private ObjectPool<T> sPool = new ObjectPool<T>(40, 10);

        public T Allocate()
        {
            // Grab the next available object
            var lInstance = sPool.Allocate();
            lInstance.IsSent = false;
            lInstance.IsHandled = false;
            return lInstance;
        }

        public void Release(T rInstance)
        {
            if (rInstance == null)
            {
                return;
            }

            rInstance.IsSent = true;
            rInstance.IsHandled = true;

            // Make it available to others.
            sPool.Release(rInstance);
        }

        public void Release(IMessage rInstance)
        {
            if (rInstance == null)
            {
                return;
            }

            rInstance.Clear();
            rInstance.IsSent = true;
            rInstance.IsHandled = true;

            if (rInstance is T message)
            {
                sPool.Release(message);
            }
        }
    }*/
}

