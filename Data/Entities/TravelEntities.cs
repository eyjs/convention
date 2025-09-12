using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalRAG.Data.Entities;

[Table("ScheduleItems")]
public class ScheduleItem
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int ConvId { get; set; }
    
    [Required]
    public int GroupId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string GroupName { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? GroupIcon { get; set; }
    
    [Required]
    public string SubGroupName { get; set; } = string.Empty;
    
    [Required]
    public int GuestId { get; set; }
    
    public byte DeleteYn { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[Table("TravelTrips")]
public class TravelTrip
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string TripName { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    [MaxLength(100)]
    public string? Destination { get; set; }
    
    public decimal? Budget { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public virtual ICollection<ScheduleItem> ScheduleItems { get; set; } = new List<ScheduleItem>();
    public virtual ICollection<TripGuest> TripGuests { get; set; } = new List<TripGuest>();
}

[Table("TripGuests")]
public class TripGuest
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int TripId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string GuestName { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Email { get; set; }
    
    [MaxLength(50)]
    public string? Phone { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    [ForeignKey("TripId")]
    public virtual TravelTrip TravelTrip { get; set; } = null!;
}