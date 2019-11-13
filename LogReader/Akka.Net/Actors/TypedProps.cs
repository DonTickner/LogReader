using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Util;

namespace LogReader.Akka.Net.Actors
{
    public struct Props<T> : ISurrogated
    {
        public readonly Props Underlying;

        public Props(Props props) : this()
        {
            Underlying = props ?? throw new ArgumentNullException(nameof(props), $"{this} has received null instead of {nameof(Props)}");
        }

        public static Props<T> Create<TActor>(Expression<Func<TActor>> fac) where TActor : UntypedActor
            => new Props<T>(Props.Create(fac));

        public ISurrogate ToSurrogate(ActorSystem system) => new TypedPropsSurrogate<T>(Underlying.ToSurrogate(system));
    }

    internal struct TypedPropsSurrogate<T> : ISurrogate
    {
        public readonly ISurrogate PropsSurrogate;

        public TypedPropsSurrogate(ISurrogate propsSurrogate) : this()
        {
            PropsSurrogate = propsSurrogate;
        }

        public ISurrogated FromSurrogate(ActorSystem system) => new Props<T>((Props)PropsSurrogate.FromSurrogate(system));
    }
}
