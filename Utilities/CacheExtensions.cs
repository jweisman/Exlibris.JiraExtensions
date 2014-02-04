using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace ExLibris.JiraExtensions.Utilities
{
    public static class CacheExtensions
    {
        public static T GetOrStore<T>(this Cache cache, string key, double expireSeconds, Func<T> generator)
        {
            var result = cache[key];
            if (result == null)
            {
                result = generator();
                cache.Insert(key, result, null, Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(expireSeconds));
            }
            return (T)result;
        }
    }

}