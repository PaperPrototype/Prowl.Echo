﻿// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Prowl.Echo;

[RequiresUnreferencedCode("These methods use reflection and can't be statically analyzed.")]
public static class ReflectionUtils
{
    // Cache for type lookups
    private static readonly ConcurrentDictionary<string, Type?> TypeCache = new();
    // Cache for serializable fields
    private static readonly ConcurrentDictionary<RuntimeTypeHandle, FieldInfo[]> SerializableFieldsCache = new();

    /// <summary>
    /// Clears all reflection caches. Call this when you need to reload assemblies or refresh type information.
    /// </summary>
    public static void ClearCache()
    {
        TypeCache.Clear();
        SerializableFieldsCache.Clear();
    }
    internal static Type? FindTypeByName(string qualifiedTypeName)
    {
        return TypeCache.GetOrAdd(qualifiedTypeName, typeName => {
            // First try direct type lookup
            Type? t = Type.GetType(typeName);
            if (t != null)
                return t;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly asm in assemblies)
            {
                // Try full name lookup
                t = asm.GetType(typeName);
                if (t != null)
                    return t;
                // Try name-only lookup (case insensitive)
                t = asm.GetTypes().FirstOrDefault(type => type.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
                if (t != null)
                    return t;
            }
            return null;
        });
    }

    internal static FieldInfo[] GetSerializableFields(this object target)
    {
        Type targetType = target.GetType();
        return SerializableFieldsCache.GetOrAdd(targetType.TypeHandle, _ => {
            const BindingFlags flags = BindingFlags.Public |
                                     BindingFlags.NonPublic |
                                     BindingFlags.Instance;
            return targetType.GetFields(flags)
                .Where(field => IsFieldSerializable(field))
                .ToArray();
        });
    }

    private static bool IsFieldSerializable(FieldInfo field)
    {
        // Check if field should be serialized
        bool shouldSerialize = field.IsPublic || field.GetCustomAttribute<SerializeFieldAttribute>() != null;
        if (!shouldSerialize)
            return false;
        // Check if field should be ignored
        bool shouldIgnore = field.GetCustomAttribute<SerializeIgnoreAttribute>() != null ||
                            field.GetCustomAttribute<NonSerializedAttribute>() != null;
        if (shouldIgnore)
            return false;
        return true;
    }
}
