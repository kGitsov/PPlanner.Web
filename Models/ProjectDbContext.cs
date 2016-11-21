using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace PPlanner.Models
{
    public class ProjectDbContext : DbContext
    {
        public ProjectDbContext()
            : base("name=DefaultConnection")
        {

        }

        //TODO write data-annotation
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<UserStory> UserStories { get; set; }
        //public DbSet<Release> Releases { get; set; }
        public DbSet<Sprint> Sprints { get; set; }
        public DbSet<ItemStatus> ItemStatuses { get; set; }

        public DbSet<Resources> Resources { get; set; }
        
        public DbSet<ProjectDevelopers> ProjectDevelopers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);

            //Correct
            //modelBuilder.Entity<Sprint>()
            //            .HasRequired(s => s.Project)
            //            .WithMany(s => s.Sprints)
            //            .HasForeignKey(f => f.Project_ProjectId)
            //            .WillCascadeOnDelete(false);

            //modelBuilder.Entity<UserStory>()
            //            .HasRequired(s => s.Sprint)
            //            .WithMany(s => s.UserStories)
            //            .HasForeignKey(f => f.Sprint_SprintId)
            //            .WillCascadeOnDelete(false);

            //modelBuilder.Entity<UserStory>()
            //            .HasRequired(us => us.Project)
            //            .WithMany(us => us.)
            //            .HasForeignKey(us => us.ProjectId)
            //            .WillCascadeOnDelete(false);

            //modelBuilder.Entity<UserStory>()
            //            .HasRequired(us => us.Sprint)
            //            .WithMany()
            //            .HasForeignKey(us => us.SprintId)
            //            .WillCascadeOnDelete(false);

            //modelBuilder.Entity<UserStory>()
            //            .HasRequired(us => us.ItemStatus)
            //            .WithMany()
            //            .HasForeignKey(us => us.ItemStatusId)
            //            .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Diner>()
            //.HasRequired(diner => diner.NameLocalization)
            //.WithMany()
            //.IsIndependent()
            //.Map(s =>
            //{
            //    s.MapKey(localization => localization.Id, "NameLocalizationID");
            //});

            //modelBuilder.Entity<Diner>()
            //.HasOptional(diner => diner.DescriptionLocalization)
            //.WithMany()
            //.IsIndependent()
            //.Map(s =>
            //{
            //    s.MapKey(localization => localization.Id, "DescriptionLocationID");
            //});

            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            //modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            base.OnModelCreating(modelBuilder);
        }

    }
}