﻿// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

namespace Prowl.Echo;

public interface ISerializationFormat
{
    bool CanHandle(Type type);
    Echo Serialize(object value, SerializationContext context);
    object? Deserialize(Echo value, Type targetType, SerializationContext context);
}