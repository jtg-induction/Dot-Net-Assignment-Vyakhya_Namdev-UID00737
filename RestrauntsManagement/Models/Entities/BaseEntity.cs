using System;
using System.ComponentModel.DataAnnotations;
public abstract class BaseEntity
{
    [Required]
    public DateTime CreatedAt { get; set; }
    [Required]
    public DateTime UpdatedAt { get; set; }
}
