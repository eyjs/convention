using Microsoft.EntityFrameworkCore;
using LocalRAG.DTOs.ScheduleModels;
using LocalRAG.Entities;
using LocalRAG.Entities.Action;
using LocalRAG.Entities.FormBuilder;
using LocalRAG.Entities.PersonalTrip;
using LocalRAG.Entities.Flight;

namespace LocalRAG.Data;

public class ConventionDbContext : DbContext
{
    public ConventionDbContext(DbContextOptions<ConventionDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Convention> Conventions { get; set; }
    public DbSet<UserConvention> UserConventions { get; set; }
    public DbSet<GuestAttribute> GuestAttributes { get; set; }
    public DbSet<Feature> Features { get; set; }
    public DbSet<Menu> Menus { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<Owner> Owners { get; set; }

    public DbSet<ScheduleTemplate> ScheduleTemplates { get; set; }
    public DbSet<ScheduleItem> ScheduleItems { get; set; }
    public DbSet<GuestScheduleTemplate> GuestScheduleTemplates { get; set; }
    public DbSet<Notice> Notices { get; set; }
    public DbSet<NoticeCategory> NoticeCategories { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<UserNotification> UserNotifications { get; set; }
    public DbSet<Comment> Comments { get; set; }
    
    // Action Management
    public DbSet<ConventionAction> ConventionActions { get; set; }
    public DbSet<UserActionStatus> UserActionStatuses { get; set; }
    public DbSet<ActionTemplate> ActionTemplates { get; set; }
    public DbSet<ActionSubmission> ActionSubmissions { get; set; }
    public DbSet<FileAttachment> FileAttachments { get; set; }
    public DbSet<Gallery> Galleries { get; set; }
    public DbSet<GalleryImage> GalleryImages { get; set; }
    // Form Builder System
    public DbSet<FormDefinition> FormDefinitions { get; set; }
    public DbSet<FormField> FormFields { get; set; }
    public DbSet<FormSubmission> FormSubmissions { get; set; }

    public DbSet<Survey> Surveys { get; set; }
    public DbSet<SurveyQuestion> SurveyQuestions { get; set; }
    public DbSet<QuestionOption> QuestionOptions { get; set; }
    public DbSet<SurveyResponse> SurveyResponses { get; set; }
    public DbSet<SurveyResponseDetail> SurveyResponseDetails { get; set; }

    // Personal Trip
    public DbSet<PersonalTrip> PersonalTrips { get; set; }
    public DbSet<Flight> Flights { get; set; }
    public DbSet<Accommodation> Accommodations { get; set; }
    public DbSet<ItineraryItem> ItineraryItems { get; set; }
    public DbSet<ChecklistCategory> ChecklistCategories { get; set; }
    public DbSet<ChecklistItem> ChecklistItems { get; set; }

    // Incheon Flight Data
    public DbSet<IncheonFlightData> IncheonFlightData { get; set; }

    // Option Tours
    public DbSet<OptionTour> OptionTours { get; set; }
    public DbSet<UserOptionTour> UserOptionTours { get; set; }

    // Schedule Images
    public DbSet<ScheduleImage> ScheduleImages { get; set; }

    // Companion Relations
    public DbSet<CompanionRelation> CompanionRelations { get; set; }

    // SMS Logs
    public DbSet<SmsLog> SmsLogs { get; set; }
    public DbSet<SmsTemplate> SmsTemplates { get; set; }

    // Home Banners
    public DbSet<HomeBanner> HomeBanners { get; set; }

    // Attribute Categories
    public DbSet<AttributeCategory> AttributeCategories { get; set; }
    public DbSet<AttributeCategoryItem> AttributeCategoryItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConventionDbContext).Assembly);
    }
}
