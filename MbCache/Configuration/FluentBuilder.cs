using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MbCache.Logic;

namespace MbCache.Configuration
{
	public class FluentBuilder<T> : IFluentBuilder<T>
	{
		private readonly IDictionary<Type, ImplementationAndMethods> _cachedMethods;
		private readonly ImplementationAndMethods _details;
		private readonly ProxyValidator _proxyValidator;

		public FluentBuilder(IDictionary<Type, ImplementationAndMethods> cachedMethods, 
									ImplementationAndMethods details, 
									ProxyValidator proxyValidator)
		{
			_cachedMethods = cachedMethods;
			_details = details;
			_proxyValidator = proxyValidator;
		}

		public IFluentBuilder<T> CacheMethod(Expression<Func<T, object>> expression)
		{
			var method = ExpressionHelper.MemberName(expression.Body);
			//if(!method.IsVirtual || method.IsFinal)
			//    throw new InvalidOperationException("Method " + method.Name + " on type " + method.DeclaringType.Name + " must be virtual.");
			_details.Methods.Add(method);
			return this;
		}

		public IFluentBuilder<T> PerInstance()
		{
			_details.CachePerInstance = true;
			return this;
		}

		public void As<TInterface>()
		{
			addToCachedMethods(typeof(TInterface));
		}

		public void AsImplemented()
		{
			_proxyValidator.Validate(_details);
			addToCachedMethods(_details.ConcreteType);	
		}

		private void addToCachedMethods(Type type)
		{
			if (_cachedMethods.ContainsKey(type))
				throw new ArgumentException("Type " + type + " is already in CacheBuilder");
			_cachedMethods[type] = _details;
		}
	}
}