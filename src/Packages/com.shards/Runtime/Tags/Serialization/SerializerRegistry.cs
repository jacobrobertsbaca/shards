using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Pool;

namespace Shards.Tags.Serialization
{
    internal class SerializerRegistry
    {
        internal struct Open {}
        internal static readonly Type OpenType = typeof(Open);

        private class SerializerRecord
        {
            /// <summary>
            /// The type that this serializer serializes.
            /// May contain open generic type parameters, but they will be those defined by <see cref="SerializerType"/>
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
         */
        private readonly Dictionary<Type, List<SerializerRecord>> declaredSerializers = new();

        /**
         * Maps a desired type (not generic) to a serializer for that type.
         * Maps to null if no serializer is known to exist for the given type.
         */
        private readonly Dictionary<Type, ITagSerializer> serializers = new();

        public SerializerRegistry(params Type[] source) : this(source as IEnumerable<Type>) {}

        public SerializerRegistry(IEnumerable<Type> source = null)
        {
            // Go through all types deriving from ITagSerializer
            // and add them to the list of declared serializers

            var sourceTypes = source ?? AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(s => s.GetCustomAttribute<TagSerializerAttribute>() is var attr && attr?.Ignore != true);
            var serializerTypes = sourceTypes.Where(s => !s.IsAbstract && typeof(ITagSerializer).IsAssignableFrom(s));

            foreach (var serializerType in serializerTypes)
            {
                var serializedType = GetSerializedType(serializerType);
                var normalizedType = GetNormalizedType(serializedType);
                var record = new SerializerRecord(serializedType, serializerType);

                if (!declaredSerializers.ContainsKey(normalizedType))
                    declaredSerializers[normalizedType] = new();
                declaredSerializers[normalizedType].Add(record);
            }
        }

        public TagSerializer<T> Get<T>()
        {
            var objectType = typeof(T);
            if (serializers.TryGetValue(objectType, out var serializer)) return (TagSerializer<T>) serializer;
            serializers[objectType] = ResolveSerializer(objectType);
            return (TagSerializer<T>) serializers[objectType];
        }

        private ITagSerializer ResolveSerializer(Type objectType)
        {
            IEnumerable<Type> ResolveSerializerTypesForType(Type normalizedType)
            {
                if (!declaredSerializers.TryGetValue(normalizedType, out var records)) yield break;

                foreach (var record in records)
                {
                    if (TryBindSerializerType(objectType, record.SerializedType, record.SerializerType, out var serializerType))
                        yield return serializerType;
                }
            }

            bool TryResolveSerializerForGroup(IEnumerable<Type> group, out Type serializerType)
            {
                // Need to find the best serializer within this complexity group.
                // Let's find ALL the valid serializers, and then choose the one with the lowest priority.

                serializerType = null;

                var bestSerializers = group.SelectMany(ResolveSerializerTypesForType)
                    .GroupBy(GetSerializerPriority)
                    .OrderBy(g => g.Key)
                    .FirstOrDefault();

                if (bestSerializers is null) return false;

                if (bestSerializers.Count() > 1)
                {
                    throw new SerializerNotFoundException($"Error while attempting to locate a serializer for type \n\n\t{objectType}\n\n" +
                        $"It is ambiguous which of the following serializers should serialize this type:" +
                        $"\n\n\t{string.Join("\n\t", bestSerializers)}\n\n" +
                        $"You can resolve this ambiguity by defining only one serializer for this type, " +
                        $"or by marking one of them as having a higher priority using the [{typeof(TagSerializerAttribute).Name}] attribute.");
                }

                serializerType = bestSerializers.First();
                return true;
            }

            // Need to examine all possible type expansions of T
            // Within each complexity group, we will scan for available serializers
            // If a complexity group has more than one serializer, then it's ambiguous which
            // one we should use and should throw an error

            var groups = GetTypeExpansions(objectType)
                .GroupBy(item => item.Complexity, item => item.Expansion)
                .OrderBy(group => group.Key);

            foreach (var group in groups)
            {
                if (TryResolveSerializerForGroup(group, out Type serializerType))
                {
                    // Found an unambiguous serializer for this type
                    return Activator.CreateInstance(serializerType) as ITagSerializer;
                }
            }

            // No serializer found across all expansions
            return null;
        }

        private static int GetSerializerPriority(Type serializerType)
        {
            var attr = serializerType.GetCustomAttribute<TagSerializerAttribute>();
            if (attr is null) return 0;
            if (attr.Fallback) return int.MaxValue;
            return attr.Priority;
        }

        internal static Type GetBaseType(Type derivedType, Type baseType)
        {
            Debug.Assert(derivedType is not null);
            Debug.Assert(baseType is not null);
            if (derivedType == baseType) return derivedType;
            if (!baseType.IsGenericType) return baseType.IsAssignableFrom(derivedType) ? baseType : null;
            if (baseType.IsInterface) return derivedType.GetInterfaces()
                    .Where(i => i.IsGenericType)
                    .Where(i => i.GetGenericTypeDefinition() == baseType.GetGenericTypeDefinition())
                    .SingleOrDefault();

            Type WalkHierarchy(Type derived)
            {
                if (derived.IsGenericType && derived.GetGenericTypeDefinition() == baseType.GetGenericTypeDefinition()) return derived;
                if (derived.BaseType is not null) return WalkHierarchy(derived.BaseType);
                return null;
            }

            return WalkHierarchy(derivedType);
        }

        /// <summary>
        /// Gets the desired type of a serializer. This type could potentially be
        /// generic if the serializer serializes a generic type.
        /// </summary>
        /// <param name="serializerType">The type of the serializer.</param>
        /// <returns>The serializer's desired type.</returns>
        private static Type GetSerializedType(Type serializerType) => GetBaseType(serializerType, typeof(TagSerializer<>)).GetGenericArguments()[0];

        internal static Type GetNormalizedType(Type type)
        {
            if (type.IsGenericParameter) return OpenType;

            if (type.IsArray)
            {
                if (type.IsSZArray) return GetNormalizedType(type.GetElementType()).MakeArrayType();
                return GetNormalizedType(type.GetElementType()).MakeArrayType(type.GetArrayRank());
            }

            if (!type.IsGenericType) return type;

            return type.GetGenericTypeDefinition().MakeGenericType(
                type.GetGenericArguments().Select(t => GetNormalizedType(t)).ToArray()
            );
        }

        private static bool TryBindSerializerType(Type objectType, Type templateType, Type serializerType, out Type constructedType)
        {
            Debug.Assert(!objectType.IsGenericType || objectType.IsConstructedGenericType);
            Debug.Assert(serializerType.IsTypeDefinition);
            constructedType = serializerType;

            // Serializer needs no type arguments bound, so it can be instantiated directly
            if (!serializerType.IsGenericType) return true;

            // If building type map fails (because a type in template type mapped to multiple
            // different types in the object type), we'll just return null
            if (!TryGetGenericTypeMap(objectType, templateType, out var typeMap)) return false;

            // If any of the types we need to construct this type could not be found in the
            // object type, then we'll just return null
            Type[] serializerArgs = serializerType.GetGenericArguments();
            if (serializerArgs.Any(sa => !typeMap.ContainsKey(sa))) return false;

            var serializerObjectArgs = serializerArgs.Select(t => typeMap[t]).ToArray();

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
        private static bool TryGetGenericTypeMap(Type objectType, Type templateType, out IDictionary<Type, Type> typeMap)
        {
            // Recursively builds the generic type map.
            bool ConstructGenericTypeMap(Type objectType, Type templateType, IDictionary<Type, Type> typeMap)
            {
                if (templateType.IsGenericTypeParameter)
                {
                    // Search generic constraints
                    foreach (var constraint in templateType.GetGenericParameterConstraints())
                    {
                        Type baseType = GetBaseType(objectType, constraint);
                        if (baseType is null) return false;
                        if (!ConstructGenericTypeMap(baseType, constraint, typeMap))
                            return false;
                    }

                    if (typeMap.TryGetValue(templateType, out var existingType) && existingType != objectType) return false;
                    typeMap.Add(templateType, objectType);
                    return true;
                }

                if (templateType.IsArray)
                {
                    if (!objectType.IsArray) return false;
                    if (objectType.IsSZArray != templateType.IsSZArray) return false;
                    if (objectType.GetArrayRank() != templateType.GetArrayRank()) return false;
                    return ConstructGenericTypeMap(objectType.GetElementType(), templateType.GetElementType(), typeMap);
                }

                if (templateType.IsGenericType)
                {
                    if (!objectType.IsGenericType) return false;
                    if (objectType.GetGenericTypeDefinition() != templateType.GetGenericTypeDefinition()) return false;

                    Type[] objectArguments = objectType.GetGenericArguments();
                    Type[] templateArguments = templateType.GetGenericArguments();

                    for (int i = 0; i < objectArguments.Length; i++)
                    {
                        if (!ConstructGenericTypeMap(objectArguments[i], templateArguments[i], typeMap))
                            return false;
                    }

                    return true;
                }

                return templateType == objectType;
            }

            typeMap = new Dictionary<Type, Type>();
            return ConstructGenericTypeMap(objectType, templateType, typeMap);
        }

        internal static IEnumerable<(int Complexity, Type Expansion)> GetTypeExpansions(Type type)
        {
            if (type.IsArray)
            {
                // Array<T>
                // -> (..., Array<Expand(T)>)
                // -> (Max(Expand(T)) + 1, OpenType)

                int max = 0;
                foreach (var (complexity, expansion) in GetTypeExpansions(type.GetElementType()))
                {
                    max = Mathf.Max(max, complexity);
                    if (type.IsSZArray) yield return (complexity, expansion.MakeArrayType());
                    else yield return (complexity, expansion.MakeArrayType(type.GetArrayRank()));
                }

                yield return (max + 1, OpenType);
                yield break;
            }

            if (!type.IsGenericType)
            {
                yield return (0, type);
                yield return (1, OpenType);
                yield break;
            }

            var expansions = GetTypeExpansions(type, type.GetGenericArguments(), 0, 0);
            int maxComplexity = 0;

            foreach (var expansion in expansions)
            {
                maxComplexity = Mathf.Max(maxComplexity, expansion.Complexity);
                yield return expansion;
            }

            yield return (maxComplexity + 1, OpenType);
        }

        private static IEnumerable<(int Complexity, Type Expansion)> GetTypeExpansions(Type type, Type[] arguments, int index, int complexity)
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
                foreach (var expansion in GetTypeExpansions(type, arguments, index + 1, complexity + prevComplexity))
                    yield return expansion;
            }

            arguments[index] = given;
        }
    }
}