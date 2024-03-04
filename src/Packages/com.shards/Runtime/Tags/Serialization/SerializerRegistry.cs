using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Shards.Tags.Serialization
{
    internal class SerializerRegistry
    {
        private static IComparer<SerializerRecord> serializerComparer = Comparer<SerializerRecord>.Create((a, b) =>
        {
            // TODO: All serializers equal right now
            // Need to allow user specified preferences
            return 0;
        });

        private class SerializerRecord
        {
            /// <summary>
            /// The type that this serializer serializes.
            /// May contain open generic type parameters, but they will be those defiend by <see cref="SerializerType"/>
            /// </summary>
            public Type SerializedType { get; }

            /// <summary>
            /// The type definition of this serializer.
            /// May contain generic parameters which will match up with those in the template type.
            /// </summary>
            public Type SerializerType { get; }

            public SerializerRecord(Type serializedType, Type serializerType)
            {
                SerializedType = serializedType;
                SerializerType = serializerType;
            }
        }

        /**
         * Maps a normalized serialized type (could be generic) to a set of possible serializer types for that type.
         * This set is sorted by priority of the serializer, with higher priority
         * serializers appearing first.
         */
        private readonly Dictionary<Type, SortedSet<SerializerRecord>> declaredSerializers = new();

        /**
         * Maps a desired type (not generic) to a serializer for that type.
         * Maps to null if no serializer is known to exist for the given type.
         */
        private readonly Dictionary<Type, ITagSerializer> serializers = new();

        public SerializerRegistry()
        {
            // Go through all types deriving from ITagSerializer
            // and add them to the list of declared serializers

            var serializerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(s => !s.IsAbstract && typeof(ITagSerializer).IsAssignableFrom(s));

            foreach (var serializerType in serializerTypes)
            {
                var serializedType = GetSerializedType(serializerType);
                var normalizedType = GetNormalizedType(serializedType);
                var record = new SerializerRecord(serializedType, serializerType);

                if (!declaredSerializers.ContainsKey(normalizedType))
                    declaredSerializers[normalizedType] = new SortedSet<SerializerRecord>(serializerComparer);
                declaredSerializers[normalizedType].Add(record);
            }
        }

        public ITagSerializer Get<T>()
        {
            var objectType = typeof(T);
            if (serializers.TryGetValue(objectType, out var serializer)) return serializer;
            serializers[objectType] = ResolveSerializer(objectType);
            return serializers[objectType];
        }

        private ITagSerializer ResolveSerializer(Type objectType)
        {
            // Need to examine all possible type expansions of T
            // Within each complexity group, we will scan for available serializers
            // If a complexity group has more than one serializer, then it's ambiguous which
            // one we should use and should throw an error

            var groups = GetTypeExpansions(objectType)
                .GroupBy(item => item.Complexity, item => item.Expansion)
                .OrderBy(group => group.Key);

            foreach (var group in groups)
            {
                Type serializerType = null;

                foreach (var normalizedType in group)
                {
                    if (TryResolveSerializerType(normalizedType, objectType, out var candidate))
                    {
                        if (serializerType is not null) throw new ShardException("Multiple serializers found for the same type.");
                        serializerType = candidate;
                    }
                }

                if (serializerType is not null)
                {
                    // Found an unambiguous serializer for this type
                    return Activator.CreateInstance(serializerType) as ITagSerializer;
                }
            }

            // No serializer found across all expansions
            return null;
        }

        private bool TryResolveSerializerType(Type normalizedType, Type objectType, out Type serializerType)
        {
            serializerType = null;

            if (!declaredSerializers.TryGetValue(normalizedType, out var records)) return false;

            foreach (var record in records)
            {
                if (TryBindSerializerType(objectType, record.SerializedType, record.SerializerType, out serializerType))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the desired type of a serializer. This type could potentially be
        /// generic if the serializer serializes a generic type.
        /// </summary>
        /// <param name="serializerType">The type of the serializer.</param>
        /// <returns>The serializer's desired type.</returns>
        public static Type GetSerializedType(Type serializerType)
        {
            Debug.Assert(serializerType is not null);

            // If this type matches TagSerializer<T>,
            // then the value of T is the serialized type
            if (serializerType.IsGenericType && serializerType.GetGenericTypeDefinition() == typeof(TagSerializer<>))
                return serializerType.GetGenericArguments()[0];

            // Otherwise, walk up type hierarchy until we find the TagSerializer<T>
            if (serializerType.BaseType is not null) return GetSerializedType(serializerType.BaseType);
            return null;
        }

        public static Type GetNormalizedType(Type type)
        {
            if (!type.IsGenericType) return type;

            Type genericDefinition = type.GetGenericTypeDefinition();
            Type[] genericArguments = type.GetGenericArguments();
            Type[] definedArguments = genericDefinition.GetGenericArguments();
            Type[] normalizedArguments = new Type[genericArguments.Length];

            for (int i = 0; i < normalizedArguments.Length; i++)
            {
                if (genericArguments[i].IsGenericTypeParameter)
                    normalizedArguments[i] = definedArguments[i];
                else normalizedArguments[i] = GetNormalizedType(genericArguments[i]);
            }

            return genericDefinition.MakeGenericType(normalizedArguments);
        }

        public static bool TryBindSerializerType(Type objectType, Type templateType, Type serializerType, out Type constructedType)
        {
            constructedType = serializerType;

            // Serializer needs no type arguments bound, so it can be instantiated directly
            if (!serializerType.IsGenericType) return true;

            Debug.Assert(serializerType.IsGenericTypeDefinition);

            // If building type map fails (because a type in template type mapped to multiple
            // different types in the object type), we'll just return null
            if (!TryGetGenericTypeMap(objectType, templateType, out var typeMap)) return false;
            var serializerObjectArgs = serializerType.GetGenericArguments().Select(t => typeMap[t]).ToArray();

            // Instantiating generic type may fail if the type has a generic type constraint
            // If this is the case, we'll just return null to indicate that a serializer cannot be
            // made for this object type
            try
            {
                constructedType = serializerType.MakeGenericType(serializerObjectArgs);
            } catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        //      objectType = Dictionary<string, bool>
        //      templateType = Dictionary<K, V>
        //      K -> string, V -> bool
        public static bool TryGetGenericTypeMap(Type objectType, Type templateType, out IDictionary<Type, Type> typeMap)
        {
            // Recursively builds the generic type map.
            bool ConstructGenericTypeMap(Type objectType, Type templateType, IDictionary<Type, Type> typeMap)
            {
                if (templateType.IsGenericTypeParameter)
                {
                    if (typeMap.TryGetValue(templateType, out var existingType) && existingType != objectType) return false;
                    typeMap.Add(templateType, objectType);
                    return true;
                }

                Type[] objectArguments = objectType.GetGenericArguments();
                Type[] templateArguments = templateType.GetGenericArguments();
                Debug.Assert(objectArguments.Length == templateArguments.Length);

                for (int i = 0; i < objectArguments.Length; i++)
                {
                    if (!ConstructGenericTypeMap(objectArguments[i], templateArguments[i], typeMap))
                        return false;
                }

                return true;
            }

            typeMap = new Dictionary<Type, Type>();
            return ConstructGenericTypeMap(objectType, templateType, typeMap);
        }

        //                 Dict<int, HS<string>>
        //                       |       |
        //                  {int, K}  {HS<string>, HS<U>, V}
        //
        //  Dict<int, HS<string>>
        // 
        //  Dict<int, HS<U>>
        //  Dict<U, HS<string>>
        //
        //  Dict<K, HS<U>>
        //
        //  Dict<K, V>

        public static IEnumerable<(int Complexity, Type Expansion)> GetTypeExpansions(Type type)
        {
            if (!type.IsGenericType) return new[] { (0, type) };
            return GetTypeExpansions(type, type.GetGenericArguments(), type.GetGenericTypeDefinition().GetGenericArguments(), 0, 0);
        }

        private static IEnumerable<(int Complexity, Type Expansion)> GetTypeExpansions(Type type, Type[] arguments, Type[] openArguments, int index, int complexity)
        {
            if (index >= arguments.Length)
            {
                yield return (complexity, type.GetGenericTypeDefinition().MakeGenericType(arguments));
                yield break;
            }

            Type given = arguments[index];
            int maxComplexity = 0;

            foreach (var (prevComplexity, substitution) in GetTypeExpansions(given))
            {
                maxComplexity = Mathf.Max(maxComplexity, prevComplexity);
                arguments[index] = substitution;
                foreach (var expansion in GetTypeExpansions(type, arguments, openArguments, index + 1, complexity + prevComplexity))
                    yield return expansion;
            }

            // Substitute open argument
            arguments[index] = openArguments[index];
            foreach (var expansion in GetTypeExpansions(type, arguments, openArguments, index + 1, complexity + maxComplexity + 1))
                yield return expansion;

            arguments[index] = given;
        }
    }
}