using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


#if NETSTANDARD2_0
namespace System.Runtime.CompilerServices
{
	internal static class IsExternalInit
	{
	}
}

internal static class LinqPolyfills
{
	public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
		=> new HashSet<T>(source);

	public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
		=> new HashSet<T>(source, comparer);
}
#endif
