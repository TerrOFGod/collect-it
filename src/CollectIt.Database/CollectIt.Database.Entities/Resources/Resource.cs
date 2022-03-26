﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CollectIt.Database.Entities.Account;

namespace CollectIt.Database.Entities.Resources;

public class Resource
{
    [Key]
    public int Id { get; set; }
        
    // [Required]
    public User Owner { get; set; }

    [Required]
    [ForeignKey(nameof(Owner))]
    public int OwnerId { get; set; }
    
    [Required]
    public string Path { get; set; }

    [Required]
    public string Name { get; set; }
    
    [Required]
    public DateTime UploadDate { get; set; }
}