using MbCache.Logic;

namespace MbCache.Configuration
{
	 /// <summary>
	 /// Creates the proxy. 
	 /// The implementation of this interface needs a default ctor
	 /// </summary>
	 public interface IProxyFactory
	 {
		  /// <summary>
		  /// Called once
		  /// </summary>
		  void Initialize(ICache cache, IMbCacheKey mbCacheKey, ILockObjectGenerator lockObjectGenerator);

		  /// <summary>
		  /// Creates the proxy.
		  /// </summary>
		  /// <typeparam name="T"></typeparam>
		  /// <param name="methodData">The method data.</param>
		  /// <param name="parameters">The parameters.</param>
		  /// <returns></returns>
		  T CreateProxy<T>(ImplementationAndMethods methodData, params object[] parameters)
			  where T : class;

		  /// <summary>
		  /// Gets a value indicating whether [allow non virtual member]
		  /// for methods not marked for caching.
		  /// </summary>
		  /// <value>
		  /// 	<c>true</c> if [allow non virtual member]; otherwise, <c>false</c>.
		  /// </value>
		  bool AllowNonVirtualMember { get; }
	 }
}