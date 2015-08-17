﻿using System;
using MbCache.Configuration;
using MbCacheTest.Caches;
using NUnit.Framework;

namespace MbCacheTest
{
	public abstract class SimpleTest
	{
		protected SimpleTest(): this("MbCacheTest.ProxyImplThatThrows") 
		{
		}

		protected SimpleTest(string proxyTypeString)
		{
			var proxyType = Type.GetType(proxyTypeString, true);
			ProxyFactory = (IProxyFactory) Activator.CreateInstance(proxyType);
		}

		[SetUp]
		public void Setup()
		{
			var lockObjectGenerator = CreateLockObjectGenerator();
			CacheBuilder = new CacheBuilder(ProxyFactory)
					.SetCache(CreateCache())
					.SetCacheKey(CreateCacheKey());
			if (lockObjectGenerator != null)
				CacheBuilder.SetLockObjectGenerator(lockObjectGenerator);
			TestSetup();
		}

		protected virtual void TestSetup(){}

		protected CacheBuilder CacheBuilder { get; private set; }
		protected IProxyFactory ProxyFactory { get; private set; }

		protected virtual ICache CreateCache()
		{
			Tools.ClearMemoryCache();
			return new InMemoryCache(new FixedNumberOfLockObjects(40) , 20);
		}

		protected virtual ICacheKey CreateCacheKey()
		{
			return new ToStringCacheKey();
		}

		protected virtual ILockObjectGenerator CreateLockObjectGenerator()
		{
			return null;
		}
	}
}