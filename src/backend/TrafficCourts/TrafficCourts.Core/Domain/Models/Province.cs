﻿using System.Diagnostics.CodeAnalysis;

namespace TrafficCourts.Domain.Models;

[ExcludeFromCodeCoverage]
public record Province(
    string CtryId, 
    string ProvSeqNo, 
    string ProvNm, 
    string ProvAbbreviationCd
);
