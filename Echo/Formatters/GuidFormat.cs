﻿// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

namespace Prowl.Echo.Formatters;

internal sealed class GuidFormat : ISerializationFormat
{
    public bool CanHandle(Type type) => type == typeof(Guid);

    public Echo Serialize(object value, SerializationContext context)
    {
        if (value is Guid guid)
            return new(PropertyType.String, guid.ToString());

        throw new NotSupportedException($"Type '{value.GetType()}' is not supported by GuidFormat.");
    }

    public object? Deserialize(Echo value, Type targetType, SerializationContext context)
    {
        if (value.TagType != PropertyType.String)
            throw new Exception($"Expected String type for Guid, got {value.TagType}");

        return Guid.Parse(value.StringValue);
    }
}
