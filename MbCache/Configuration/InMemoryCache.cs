﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using MbCache.Core;
using MbCache.Core.Events;

namespace MbCache.Configuration
{
	[Serializable]
	public class InMemoryCache : ICache
	{
		private readonly ILockObjectGenerator _lockObjectGenerator;
		private readonly int _timeoutMinutes;
		private static readonly MemoryCache cache = MemoryCache.Default;
		private static readonly object dependencyValue = new object();
		private EventListenersCallback _eventListenersCallback;
		private const string mainCacheKey = "MainMbCacheKey";

		public InMemoryCache(ILockObjectGenerator lockObjectGenerator, int timeoutMinutes)
		{
			_lockObjectGenerator = lockObjectGenerator;
			_timeoutMinutes = timeoutMinutes;
		}

		public void Initialize(EventListenersCallback eventListenersCallback)
		{
			_eventListenersCallback = eventListenersCallback;
		}

		public CachedItem GetAndPutIfNonExisting(EventInformation eventInformation, Func<IEnumerable<string>> dependingRemoveKeys, Func<object> originalMethod)
		{
			var cachedItem = (CachedItem)cache.Get(eventInformation.CacheKey);
			if (cachedItem != null)
			{
				_eventListenersCallback.OnCacheHit(cachedItem);
				return cachedItem;
			}

			lock (lockObject(eventInformation))
			{
				var cachedItem2 = (CachedItem)cache.Get(eventInformation.CacheKey);
				if (cachedItem2 != null)
				{
					_eventListenersCallback.OnCacheHit(cachedItem2);
					return cachedItem2;
				}
				var addedValue = executeAndPutInCache(eventInformation, dependingRemoveKeys(), originalMethod);
				_eventListenersCallback.OnCacheMiss(addedValue);
				return addedValue;
			}
		}

		public void Delete(EventInformation eventInformation)
		{
			lock (lockObject(eventInformation))
			{
					cache.Remove(eventInformation.CacheKey);
			}
		}

		public void Clear()
		{
			//todo: should (maybe?) be a proper lock here
			cache.Remove(mainCacheKey);
		}

		private object lockObject(EventInformation eventInformation)
		{
			return _lockObjectGenerator.GetFor(eventInformation.Type.FullName);
		}

		private CachedItem executeAndPutInCache(EventInformation eventInformation, IEnumerable<string> dependingRemoveKeys, Func<object> originalMethod)
		{
			var methodResult = originalMethod();
			var cachedItem = new CachedItem(eventInformation, methodResult);
			var key = cachedItem.EventInformation.CacheKey;
			var dependedKeys = dependingRemoveKeys.ToList();
			dependedKeys.Add(mainCacheKey);
			createDependencies(dependedKeys);

			var policy = new CacheItemPolicy
			{
				AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(_timeoutMinutes),
				RemovedCallback = arguments => _eventListenersCallback.OnCacheRemoval(cachedItem)
			};
			policy.ChangeMonitors.Add(cache.CreateCacheEntryChangeMonitor(dependedKeys));
			cache.Set(key, cachedItem, policy);
			return cachedItem;
		}

		private static void createDependencies(IEnumerable<string> unwrappedKeys)
		{
			foreach (var key in unwrappedKeys)
			{
				var policy = new CacheItemPolicy { Priority = CacheItemPriority.NotRemovable };
				cache.Add(key, dependencyValue, policy);
			}
		}
	}
}