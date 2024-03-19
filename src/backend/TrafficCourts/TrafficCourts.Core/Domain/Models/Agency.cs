﻿using System.Diagnostics.CodeAnalysis;

namespace TrafficCourts.Domain.Models;

[ExcludeFromCodeCoverage]
public record Agency(
    string Id, 
    string Name,
    string TypeCode
);
