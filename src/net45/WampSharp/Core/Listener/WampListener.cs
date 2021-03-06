﻿using System;
using WampSharp.Core.Dispatch;
using WampSharp.Core.Message;

namespace WampSharp.Core.Listener
{
    /// <summary>
    /// Listens to <see cref="IWampConnection{TMessage}"/>s, receives 
    /// <see cref="WampMessage{TMessage}"/>s and dispatches them to a given <see cref="IWampIncomingMessageHandler{TMessage,TClient}"/>.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TClient"></typeparam>
    public class WampListener<TMessage, TClient>
    {
        private readonly IWampIncomingMessageHandler<TMessage, TClient> mHandler;
        private readonly IWampClientContainer<TMessage, TClient> mClientContainer;
        private readonly IWampConnectionListener<TMessage> mListener;
        private IDisposable mSubscription;

        /// <summary>
        /// Creates a new instance of <see cref="WampListener{TMessage, TClient}"/>
        /// </summary>
        /// <param name="listener">The <see cref="IWampConnectionListener{TMessage}"/> used in order to 
        /// accept incoming connections.</param>
        /// <param name="handler">The <see cref="IWampIncomingMessageHandler{TMessage,TClient}"/> used
        /// in order to dispatch incoming messages.</param>
        /// <param name="clientContainer">The <see cref="IWampClientContainer{TMessage,TClient}"/> use
        /// in order to store the connected clients.</param>
        public WampListener(IWampConnectionListener<TMessage> listener,
                            IWampIncomingMessageHandler<TMessage, TClient> handler,
                            IWampClientContainer<TMessage, TClient> clientContainer)
        {
            mHandler = handler;
            mClientContainer = clientContainer;
            mListener = listener;
        }

        /// <summary>
        /// The <see cref="IWampClientContainer{TMessage,TClient}"/>
        /// holding all current connected clients.
        /// </summary>
        public IWampClientContainer<TMessage, TClient> ClientContainer
        {
            get
            {
                return mClientContainer;
            }
        }

        /// <summary>
        /// Starts listening for <see cref="IWampConnection{TMessage}"/>s.
        /// </summary>
        public virtual void Start()
        {
            mSubscription = mListener.Subscribe(x => OnNewConnection(x));
        }

        /// <summary>
        /// Stops the listener.
        /// </summary>
        public virtual void Stop()
        {
            IDisposable subscription = mSubscription;

            if (subscription != null)
            {
                subscription.Dispose();
                mSubscription = null;
            }
        }

        protected virtual void OnConnectionException(IWampConnection<TMessage> connection, Exception exception)
        {
        }

        protected virtual void OnCloseConnection(IWampConnection<TMessage> connection)
        {
            TClient client;

            if (ClientContainer.TryGetClient(connection, out client))
            {
                IDisposable casted = client as IDisposable;

                if (casted != null)
                {
                    casted.Dispose();
                }                
            }
        }

        protected virtual void OnNewMessage(IWampConnection<TMessage> connection, WampMessage<TMessage> message)
        {
            TClient client = ClientContainer.GetClient(connection);

            mHandler.HandleMessage(client, message);
        }

        protected virtual void OnNewConnection(IWampConnection<TMessage> connection)
        {
            TClient client = ClientContainer.GetClient(connection);

            connection.MessageArrived += OnNewMessage;
            connection.ConnectionOpen += OnConnectionOpen;
            connection.ConnectionClosed += OnConnectionClose;
        }

        protected virtual void OnConnectionOpen(IWampConnection<TMessage> connection)
        {
        }

        private void OnNewMessage(object sender, WampMessageArrivedEventArgs<TMessage> e)
        {
            IWampConnection<TMessage> connection = sender as IWampConnection<TMessage>;
            OnNewMessage(connection, e.Message);
        }

        private void OnConnectionOpen(object sender, EventArgs e)
        {
            IWampConnection<TMessage> connection = sender as IWampConnection<TMessage>;
            connection.ConnectionOpen -= OnConnectionOpen;
            OnConnectionOpen(connection);
        }

        private void OnConnectionClose(object sender, EventArgs e)
        {
            IWampConnection<TMessage> connection = sender as IWampConnection<TMessage>;
            connection.ConnectionClosed -= OnConnectionClose;
            connection.MessageArrived -= OnNewMessage;
            OnCloseConnection(connection);
        }
    }
}