using System;

public static class WeakReferenceExtensions {
	public static T TryGetTarget<T>(this WeakReference<T> item) where T: class {
		T target;
		if (!item.TryGetTarget(out target)) return null;
		return target;
	}
}
