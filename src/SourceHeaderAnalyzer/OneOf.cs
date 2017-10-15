using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SourceHeaderAnalyzer
{
    [DebuggerDisplay("{ToString(),nq}")]
    [DebuggerTypeProxy(typeof(OneOf<,>.DebuggerTypeProxy))]
    public struct OneOf<T1, T2> : IEquatable<OneOf<T1, T2>>
    {
        public sealed class DebuggerTypeProxy
        {
            public DebuggerTypeProxy(OneOf<T1, T2> value)
            {
                switch (value.which)
                {
                    case 1:
                        Display = value.item1;
                        break;
                    case 2:
                        Display = value.item2;
                        break;
                    default:
                        throw CreateInvalidStateException();
                }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public object Display { get; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly T1 item1;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly T2 item2;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly int which;

        public OneOf(T1 item1)
        {
            this.item1 = item1;
            item2 = default;
            which = 1;
        }

        public OneOf(T2 item2)
        {
            item1 = default;
            this.item2 = item2;
            which = 2;
        }

        public override string ToString()
        {
            switch (Which)
            {
                case 1:
                    return "Item1: " + item1;
                case 2:
                    return "Item2: " + item2;
                default:
                    throw CreateInvalidStateException();
            }
        }

        public static implicit operator OneOf<T1, T2>(T1 item1) => new OneOf<T1, T2>(item1);

        public static implicit operator OneOf<T1, T2>(T2 item2) => new OneOf<T1, T2>(item2);

        public static explicit operator T1(OneOf<T1, T2> hasItem1) => hasItem1.Item1;

        public static explicit operator T2(OneOf<T1, T2> hasItem2) => hasItem2.Item2;


        #region Functional API

        public void Switch(Action<T1> item1, Action<T2> item2)
        {
            switch (which)
            {
                case 1:
                    item1?.Invoke(this.item1);
                    return;
                case 2:
                    item2?.Invoke(this.item2);
                    return;
                default:
                    throw CreateInvalidStateException();
            }
        }

        public TResult Match<TResult>(Func<T1, TResult> item1, Func<T2, TResult> item2)
        {
            if (item1 == null) throw new ArgumentNullException(nameof(item1));
            if (item2 == null) throw new ArgumentNullException(nameof(item2));

            switch (which)
            {
                case 1:
                    return item1.Invoke(this.item1);
                case 2:
                    return item2.Invoke(this.item2);
                default:
                    throw CreateInvalidStateException();
            }
        }

        public OneOf<TResult1, TResult2> Select<TResult1, TResult2>(Func<T1, TResult1> item1, Func<T2, TResult2> item2)
        {
            if (item1 == null) throw new ArgumentNullException(nameof(item1));
            if (item2 == null) throw new ArgumentNullException(nameof(item2));

            switch (which)
            {
                case 1:
                    return new OneOf<TResult1, TResult2>(item1.Invoke(this.item1));
                case 2:
                    return new OneOf<TResult1, TResult2>(item2.Invoke(this.item2));
                default:
                    throw CreateInvalidStateException();
            }
        }

        #endregion

        #region If statement API

        public bool TryGetItem1(out T1 item1)
        {
            item1 = this.item1;
            return which == 1;
        }

        public bool TryGetItem2(out T2 item2)
        {
            item2 = this.item2;
            return which == 2;
        }

        #endregion

        #region Switch statement API

        public int Which
        {
            get
            {
                CheckInvalidState();
                return which;
            }
        }

        private void CheckInvalidState()
        {
            if (which < 1 || which > 2) throw CreateInvalidStateException();
        }
        private static Exception CreateInvalidStateException()
        {
            throw new InvalidOperationException("Struct state was not correctly initialized.");
        }

        private void CheckInvalidField(int tried)
        {
            if (tried == which) return;
            CheckInvalidState();
            throw new InvalidOperationException($"Tried to access field {tried}, but only field {Which} is present.");
        }

        public T1 Item1
        {
            get
            {
                CheckInvalidField(1);
                return item1;
            }
        }

        public T2 Item2
        {
            get
            {
                CheckInvalidField(2);
                return item2;
            }
        }

        #endregion

        #region Equality

        public override bool Equals(object obj)
        {
            return obj is OneOf<T1, T2> && Equals((OneOf<T1, T2>)obj);
        }

        public bool Equals(OneOf<T1, T2> other)
        {
            if (which != other.which) return false;
            switch (which)
            {
                case 1:
                    return EqualityComparer<T1>.Default.Equals(item1, other.item1);
                case 2:
                    return EqualityComparer<T2>.Default.Equals(item2, other.item2);
                default:
                    throw CreateInvalidStateException();
            }
        }

        public override int GetHashCode()
        {
            var hash = unchecked((int)(((2166136261 * 16777619) ^ (uint)which) * 16777619));

            switch (which)
            {
                case 1:
                    if (item1 != null) hash ^= item1.GetHashCode();
                    break;
                case 2:
                    if (item2 != null) hash ^= item2.GetHashCode();
                    break;
                default:
                    throw CreateInvalidStateException();
            }

            return hash;
        }

        public static bool operator ==(OneOf<T1, T2> left, OneOf<T1, T2> right) => left.Equals(right);

        public static bool operator !=(OneOf<T1, T2> left, OneOf<T1, T2> right) => !left.Equals(right);

        #endregion
    }
}
