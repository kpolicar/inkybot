using System;
using System.Collections.Generic;
using Inkybot.Dofus;
using Inkybot.Services;

namespace Inkybot.Extensions
{
    public static class IEnumerable
    {

        public static UserRunes ToDictionary<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, Stat> keySelector,
            Func<TSource, UserRune[]> elementSelector) {
            
            if (source == null)
                throw new ArgumentNullException(nameof (source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof (keySelector));
            if (elementSelector == null)
                throw new ArgumentNullException(nameof (elementSelector));
            
            var dictionary = new UserRunes();
            foreach (TSource source1 in source)
                dictionary.Add(keySelector(source1), elementSelector(source1));
            return dictionary;
        }
    }
}
