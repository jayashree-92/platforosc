using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using log4net;

namespace Com.HedgeMark.Commons.Extensions
{
    public static class DictionaryExtension
    {
        public static NullSafeDictionary<TK, TV> ToNullSafeDictionary<TS, TK, TV>(this IEnumerable<TS> source, Func<TS, TK> key, Func<TS, TV> value)
        {
            return new NullSafeDictionary<TK, TV>(source.ToDictionary(key, value));
        }
    }

    public class NullSafeDictionary<TK, TV>
    {
        private readonly Dictionary<TK, TV> dictionary;
        private static readonly ILog logger = LogManager.GetLogger("NullSafeDictionary");

        public NullSafeDictionary(Dictionary<TK, TV> dictionary)
        {
            this.dictionary = dictionary;
        }

        public TV this[TK key]
        {
            get
            {
                if(!dictionary.TryGetValue(key, out var value))
                    logger.ErrorFormat("Key {0} not found in dictionary",key);
                return value;
            }
        }
    }

    public class DataDictionary<TK, TV> : Dictionary<TK, TV>
    {
        private readonly ILog logger;
        private readonly TK keyColumn;

        public DataDictionary(IDictionary<TK, TV> dictionary, TK keyColumn, [CallerMemberName] string memberName = "DataDictionary") : base(dictionary)
        {
            this.keyColumn = keyColumn;
            logger = LogManager.GetLogger(memberName);
        }

        public TV GetNullOrValue(TK key)
        {
            if (!TryGetValue(key, out var value))
            {
                TryGetValue(keyColumn, out var keyColumnValue);
                logger.ErrorFormat("Key '{0}' not found in dictionary for '{1}'", key, keyColumnValue);
            }
            return value;
        }
    }
}