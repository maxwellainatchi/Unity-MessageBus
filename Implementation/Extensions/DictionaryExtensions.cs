using System.Collections.Generic;

namespace Messaging {
static class DictionaryExtensions {
	public static TValue? TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue: struct {
		TValue value;
		if (dictionary.TryGetValue(key, out value)) return value;
		return null;
	}

	public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue) {
		TValue value;
		return dictionary.TryGetValue(key, out value) ? value : defaultValue;
	}

	public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, System.Func<TValue> defaultValueProvider) {
		TValue value;
		return dictionary.TryGetValue(key, out value) ? value : defaultValueProvider();
	}
}

// needed for the overloaded generic
static class DictionaryExtensions2 {
	public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue: class {
		TValue value;
		if (dictionary.TryGetValue(key, out value)) return value;
		return null;
	}
}

[System.Serializable]
public class UpdateStageUIntSerializableDictionary: SerializableDictionary<UpdateStage, uint> {}

}
