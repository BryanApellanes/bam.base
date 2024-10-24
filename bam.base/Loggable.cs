/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bam;
using Bam.Configuration;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Xml.Serialization;
using YamlDotNet.Serialization;
using EventInfo = System.Reflection.EventInfo;

namespace Bam.Logging
{
    /// <summary>
    /// An abstract base class providing methods for 
    /// subscribing to and firing events defined on derived
    /// classes.
    /// </summary>
	[Serializable]
    public abstract class Loggable: ILoggable
    {
        private static HashSet<Loggable> _allInstances = new HashSet<Loggable>();

        protected Loggable()
        {
            this._subscribers = new HashSet<ILogger>();
            this.LogVerbosity = VerbosityLevel.Custom;
            _allInstances.Add(this);
        }

        public static void SetGlobalVerbosity(VerbosityLevel level)
        {
            _allInstances.Each(l=> l.LogVerbosity = level);
        }
        
        public static IEnumerable<ILoggable> AllInstances => _allInstances.ToArray();

        public string LoggableIdentifier => $"TypeName={GetType().Name};{IdentifierTag?.Invoke()}";


        public Func<string> IdentifierTag { get; set; }

        /// <summary>
        /// A value from 0 - 5, represented by the LogEventType enum.
        /// The higher the value the more log entries are logged.
        /// </summary>
        public VerbosityLevel LogVerbosity { get; set; }

        private HashSet<ILogger> _subscribers;
        
        /// <summary>
        /// An array of all the ILoggers that have
        /// been subscribed to this Loggable
        /// </summary>
        [XmlIgnore]
        [YamlIgnore]
        [JsonIgnore]
        [Exclude]
        public virtual ILogger[] Subscribers => _subscribers.ToArray();

        private object _subscriberLock = new object();

		/// <summary>
		/// Subscribe the current Loggables subscribers
		/// to the specified Loggable and vice versa.
		/// </summary>
		/// <param name="loggable"></param>
        [Exclude]
		public virtual void Subscribe(Loggable loggable)
		{
			Subscribers.Each(loggable.Subscribe);
            loggable.Subscribers.Each(Subscribe);
		}

        public virtual void Subscribe(VerbosityLevel levelToSubscribe, Action<ILoggable, LoggableEventArgs> handler)
        {
            Type emittingType = this.GetType();
            EventInfo[] eventInfos = emittingType.GetEvents();
            eventInfos.Each(eventInfo =>
            {
                if (eventInfo.HasCustomAttributeOfType(out VerbosityAttribute verbosityAttribute))
                {
                    if (verbosityAttribute.Value == levelToSubscribe)
                    {
                        eventInfo.AddEventHandler(this, (EventHandler)((s, a) =>
                        {
                            handler(this, LoggableEventArgs.ForLoggable(this, verbosityAttribute));
                        }));
                    }
                }
            });
        }
        
        [Exclude]
        [DebuggerStepThrough]
        public virtual void Subscribe(ILogger logger)
        {
            Subscribe(logger, LogVerbosity);
        }

        /// <summary>
        /// Subscribe the specified logger to
        /// all the events of the current instance.
        /// </summary>
        ///
        /// <remarks>
        /// Considers the 
        /// current value of LogVerbosity if
        /// the events found are adorned with the 
        /// Verbosity attribute.
        /// </remarks>
        /// <param name="logger"></param>
        [Exclude]
        [DebuggerStepThrough]
        public virtual void Subscribe(ILogger logger, VerbosityLevel levelToSubscribe)
        {
            lock (_subscriberLock)
            {
                if (logger != null && !IsSubscribed(logger))
                {
                    _subscribers.Add(logger);
                    Type emittingType = this.GetType();
                    EventInfo[] eventInfos = emittingType.GetEvents();
                    eventInfos.Each(eventInfo =>
                    {
                        bool shouldSubscribe = true;
                        VerbosityLevel logEventType = VerbosityLevel.Information;
                        if (eventInfo.HasCustomAttributeOfType(out VerbosityAttribute verbosity))
                        {
                            shouldSubscribe = (int)verbosity.Value <= (int)levelToSubscribe;
                            logEventType = verbosity.Value;
                        }

                        if (shouldSubscribe)
                        {
                            // TODO: review this to determine how to properly handle generic EventHandler<TEventArgs>
                            if (eventInfo.EventHandlerType == typeof(EventHandler))
                            {
                                eventInfo.AddEventHandler(this, (EventHandler)((s, a) =>
                                {
                                    if (a is MessageEventArgs messageEventArgs)
                                    {
                                        LogMessage msg = messageEventArgs.LogMessage;
                                        msg?.Log(logger, messageEventArgs.LogEventType);
                                    }
                                    else
                                    {
                                        string message = string.Empty;
                                        if (verbosity != null)
                                        {
                                            message = verbosity.GetMessage(s, a);
                                        }

                                        if (!string.IsNullOrEmpty(message))
                                        {
                                            logger.AddEntry(message, (int)logEventType);
                                        }
                                        else
                                        {
                                            logger.AddEntry("Event {0} raised on type {1}::{2}", (int)logEventType, logEventType.ToString(), emittingType.Name, eventInfo.Name);
                                        }
                                    }
                                }));
                            }
                        }
                    });
                }
            }
        }

        public event EventHandler MessageReceived;

        /// <summary>
        /// Fire the Message event with the specified information message
        /// </summary>
        /// <param name="messageFormat"></param>
        /// <param name="messageArgs"></param>
        [Local]
        public void Info(string messageFormat, params string[] messageArgs)
        {
            EventMessage(LogEventType.Information, messageFormat, messageArgs);
        }
        
        /// <summary>
        /// Fire the Message event with the specified warning message
        /// </summary>
        /// <param name="messageFormat"></param>
        /// <param name="messageArgs"></param>
        [Local]
        public void Warn(string messageFormat, params string[] messageArgs)
        {
            EventMessage(LogEventType.Warning, messageFormat, messageArgs);
        }
        
        /// <summary>
        /// Fire the Message event with the specified error message.
        /// </summary>
        /// <param name="messageFormat"></param>
        /// <param name="messageArgs"></param>
        [Local]
        public void Error(string messageFormat, params string[] messageArgs)
        {
            EventMessage(LogEventType.Error, messageFormat, messageArgs);
        }

        /// <summary>
        /// Output a message to the console and fire the Message
        /// event with  the specified information message.
        /// </summary>
        /// <param name="messageFormat"></param>
        /// <param name="messageArgs"></param>
        [Local]
        public void Console(string messageFormat, params string[] messageArgs)
        {
            if (messageArgs.Length > 0)
            {
                System.Console.WriteLine(messageFormat, messageArgs);
                Info(messageFormat, messageArgs);
            }
            else
            {
                System.Console.WriteLine(messageFormat);
                Info(messageFormat);
            }
        }
        
        [Exclude]
        public void EventMessage(LogEventType eventType, string format, params string[] args)
        {
            FireEvent(MessageReceived, new MessageEventArgs() {LogEventType = eventType, LogMessage = new LogMessage(format, args) { SourceType = this.GetType() } });
        }
        
        /// <summary>
        /// Returns true if the specified logger is 
        /// subscribed to the current Loggable
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        [Exclude]
        public virtual bool IsSubscribed(ILogger logger)
        {
            return _subscribers.Contains(logger);
        }

        protected void FireEventAsync(EventHandler eventHandler, object? sender = null, EventArgs? eventArgs = null)
        {
            Task.Run(() => FireEvent(eventHandler, sender ?? this, eventArgs ?? EventArgs.Empty));
        }

        protected void FireEvent<T>(EventHandler<T> eventHandler) where T: EventArgs, new()
        {
            FireEvent<T>(eventHandler, new T());
        }
        
        protected void FireEvent<T>(EventHandler<T> eventHandler, T eventArgs) where T: EventArgs, new()
        {
            try
            {
                eventHandler?.Invoke(this, eventArgs);
            }
            catch (Exception ex)
            {
                Log.Trace("Exception in FireEvent<{0}>: {1}", typeof(T).Name, ex.Message);
            }
        }
        
        /// <summary>
        /// Fire the specified event if there are
        /// subscribers.
        /// </summary>
        /// <param name="eventHandler"></param>
        protected void FireEvent(EventHandler eventHandler)
        {
            FireEvent(eventHandler, EventArgs.Empty);
        }

        /// <summary>
        /// Fire the specified event if there are subscribers
        /// </summary>
        /// <param name="eventHandler"></param>
        /// <param name="eventArgs"></param>
		protected void FireEvent(EventHandler? eventHandler, EventArgs eventArgs)
        {
            try
            {
                eventHandler?.Invoke(this, eventArgs);
            }
            catch (Exception ex)
            {
                Log.Trace("Exception in FireEvent: {0}", ex.Message);
            }
        }

        protected void FireEvent(EventHandler eventHandler, object sender, EventArgs eventArgs)
        {
            try
            {
                eventHandler?.Invoke(sender, eventArgs);
            }
            catch (Exception ex)
            {
                Log.Trace("Exception in FireEvent2: {0}", ex.Message);
            }
        }
    }
}
