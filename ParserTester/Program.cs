using ParsingEngine;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace ParserTester
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                while (true)
                {
                    Console.Write("> ");
                    Monad<string>.Of(Console.ReadLine())
                        .IfNone(() => throw new Exception("Input was null"))
                        .Bind(x => Encoding.UTF8.GetBytes(x))
                        .Bind(x => new MemoryStream(x))
                        .Bind(x => new StreamReader(x))
                        .Bind(x => new TokenLexer(x))
                        .Bind(x => x.ToList())
                        .Pipe(x => x.ForEach(Console.WriteLine))
                        .Bind(x => new Parser(x))
                        .Pipe(x => Console.WriteLine(x.Parse()))
                        .Pipe(x => x.Errors.ForEach(Console.WriteLine));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    internal interface IMonad<out T>
    {
        IMonad<T> IfNone(Action action);
        IMonad<TV> Bind<TV>(Func<T, TV> func);
        IMonad<T> Pipe(Action<T> action);
    }

    internal class Monad<T> : IMonad<T>
    {
        private T Value { get; }

        public static Monad<T> Of(T value)
        {
            if (value == null)
                return None<T>.Create();
            return new Monad<T>(value);
        }

        protected Monad(T value) => Value = value;

        public virtual IMonad<TV> Bind<TV>(Func<T, TV> func) => Monad<TV>.Of(func(Value));

        public virtual IMonad<T> Pipe(Action<T> action)
        {
            action(Value);
            return this;
        }

        public IMonad<T> IfNone(Action action)
        {
            if (this is None<T>)
                action();

            return this;
        }
    }

    internal class None<T> : Monad<T>
    {
        protected None() : base(default) { }

        public static None<T> Create() => new None<T>();

        public override IMonad<TV> Bind<TV>(Func<T, TV> func) => new None<TV>();

        public override IMonad<T> Pipe(Action<T> action) => this;
    }
}
