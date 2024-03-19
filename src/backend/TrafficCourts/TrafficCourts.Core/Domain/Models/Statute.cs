﻿using System.Diagnostics.CodeAnalysis;

namespace TrafficCourts.Domain.Models;

[ExcludeFromCodeCoverage]
public record Statute(
    string Id, 
    string ActCode, 
    string SectionText, 
    string SubsectionText,
    string ParagraphText, 
    string SubparagraphText, 
    string Code, 
    string ShortDescriptionText, 
    string DescriptionText
);
