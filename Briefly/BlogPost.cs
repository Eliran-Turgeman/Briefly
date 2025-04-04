﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Briefly;

public class BlogPost
{
    public int Id { get; set; }

    [Required]
    public string? Url { get; set; }

    public string? Summary { get; set; }

    public bool IsApprovedSummary { get; set; }

    public bool IsPublished { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
