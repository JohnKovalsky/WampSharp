﻿using Fleck;
using WampSharp.Binding;
using WampSharp.Core.Message;
using WampSharp.V2.Core.Listener;

namespace WampSharp.Fleck
{
    internal class FleckWampTextConnection<TMessage> : FleckWampConnection<TMessage>
    {
        private readonly IWampTextBinding<TMessage> mBinding;

        public FleckWampTextConnection(IWebSocketConnection webSocketConnection,
                                       IWampTextBinding<TMessage> binding)
            : base(webSocketConnection)
        {
            mBinding = binding;
            webSocketConnection.OnMessage = OnConnectionMessage;
        }

        private void OnConnectionMessage(string message)
        {
            WampMessage<TMessage> parsed =
                mBinding.Parse(message);

            RaiseNewMessageArrived(parsed);
        }

        protected override void InnerSend(WampMessage<TMessage> message)
        {
            TextMessage<TMessage> casted =
                message as TextMessage<TMessage>;

            if (casted == null)
            {
                casted = mBinding.GetRawMessage(message) as TextMessage<TMessage>;
            }

            string raw = casted.Text;

            mWebSocketConnection.Send(raw);
        }
    }
}