using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Akka.Actor;
using Akka.Util;

namespace LogReader.Akka.Net.Actors
{
    /// <summary>
    /// A convenience class that creates a base Akka.Net <see cref="UntypedActor"/>.
    /// This implementation implements abstract <see cref="ActorMessages"/> class for contextual Messages access
    /// </summary>
    /// <typeparam name="T">The class which implements the abstract class <see cref="ActorMessages"/></typeparam>
    public abstract class BaseActorWithMessages<T>: UntypedActor, IActorRef where T : IActorMessages
    {
        /// <summary>
        /// The Messages that this Actor can receive.
        /// </summary>
        public T Messages { get; set; }

        public void Tell(object message, IActorRef sender)
        {
            Self.Tell(message, sender);
        }

        public void Tell(T message)
        {
            Self.Tell(message, ActorCell.GetCurrentSelfOrNoSender());
        }

        public bool Equals(IActorRef other)
        {
            return Self.Equals(other);
        }

        public int CompareTo(IActorRef other)
        {
            return Self.CompareTo(other);
        }

        public ISurrogate ToSurrogate(ActorSystem system)
        {
            return Self.ToSurrogate(system);
        }

        public int CompareTo(object obj)
        {
            return Self.CompareTo(obj);
        }

        public ActorPath Path => Self.Path;

        public override string ToString()
        {
            return Self.ToString();
        }
    }
}
