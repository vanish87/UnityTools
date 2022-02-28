using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Common
{
    public interface IEventSender
    {
        void RegisterOnMessage(EventHandler handler);
        void UnregisterOnMessage(EventHandler handler);
    }
    public interface IEventUser
    {
        void OnSenderAdded(IEventSender sender);
        void OnSenderRemoved(IEventSender sender);
    }

    public interface IEventData
    {

    }

    public class MonoEvent : MonoBehaviour, IEventSender
    {
        private event EventHandler OnEventMessage;
        protected void OnMessage(object sender, EventArgs args) { this.OnEventMessage?.Invoke(sender, args); }
        public void RegisterOnMessage(EventHandler handler) { this.OnEventMessage -= handler; this.OnEventMessage += handler; }
        public void UnregisterOnMessage(EventHandler handler) { this.OnEventMessage -= handler; }

    }
}
