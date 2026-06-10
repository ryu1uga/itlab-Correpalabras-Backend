using Microsoft.EntityFrameworkCore;
using CorrePalabras.Models.Common;

namespace CorrePalabras.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {}

        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Avatar> Avatars { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<PageContent> PageContents { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<ProfileStory> ProfileStories { get; set; }
        public DbSet<Story> Stories { get; set; }
        public DbSet<StoryCategory> StoryCategories { get; set; }
        public DbSet<StoryLanguage> StoryLanguages { get; set; }
        public DbSet<UnlockedAvatar> UnlockedAvatars { get; set; }
        public DbSet<UnlockedBadge> UnlockedBadges { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Attachment>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()"); // Use gen_random_uuid() for PostgreSQL

            modelBuilder.Entity<Avatar>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()"); // Use gen_random_uuid() for PostgreSQL

            modelBuilder.Entity<Badge>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()"); // Use gen_random_uuid() for PostgreSQL

            modelBuilder.Entity<Category>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()"); // Use gen_random_uuid() for PostgreSQL

            modelBuilder.Entity<Language>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()"); // Use gen_random_uuid() for PostgreSQL

            modelBuilder.Entity<Page>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()"); // Use gen_random_uuid() for PostgreSQL

            modelBuilder.Entity<PageContent>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()"); // Use gen_random_uuid() for PostgreSQL

            modelBuilder.Entity<Profile>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()"); // Use gen_random_uuid() for PostgreSQL

            modelBuilder.Entity<ProfileStory>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()"); // Use gen_random_uuid() for PostgreSQL

            modelBuilder.Entity<Story>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()"); // Use gen_random_uuid() for PostgreSQL

            modelBuilder.Entity<StoryCategory>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()"); // Use gen_random_uuid() for PostgreSQL

            modelBuilder.Entity<StoryLanguage>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()"); // Use gen_random_uuid() for PostgreSQL

            modelBuilder.Entity<UnlockedAvatar>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()"); // Use gen_random_uuid() for PostgreSQL

            modelBuilder.Entity<UnlockedBadge>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()"); // Use gen_random_uuid() for PostgreSQL
            
            modelBuilder.Entity<User>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()"); // Use gen_random_uuid() for PostgreSQL

            modelBuilder.Entity<RefreshToken>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()"); // Use gen_random_uuid() for PostgreSQL

            // Foreign Keys
            modelBuilder.Entity<User>()
                .HasMany<Profile>(u => u.Profiles)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany<RefreshToken>()
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StoryLanguage>()
                .HasMany<ProfileStory>(u => u.ProfileStories)
                .WithOne(p => p.StoryLanguage)
                .HasForeignKey(p => p.StoryLanguageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Story>()
                .HasMany<Page>(s => s.Pages)
                .WithOne(p => p.Story)
                .HasForeignKey(p => p.StoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Story>()
                .HasMany<StoryLanguage>(s => s.StoryLanguages)
                .WithOne(sl => sl.Story)
                .HasForeignKey(sl => sl.StoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Story>()
                .HasMany<StoryCategory>(s => s.StoryCategories)
                .WithOne(sc => sc.Story)
                .HasForeignKey(sc => sc.StoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Story>()
                .HasMany<Attachment>(s => s.Attachments)
                .WithOne(sc => sc.Story)
                .HasForeignKey(sc => sc.StoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Story>()
                .HasMany<Avatar>(s => s.Avatars)
                .WithOne(sc => sc.Story)
                .HasForeignKey(sc => sc.StoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Profile>()
                .HasMany<UnlockedBadge>(s => s.UnlockedBadges)
                .WithOne(sc => sc.Profile)
                .HasForeignKey(sc => sc.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Profile>()
                .HasMany<UnlockedAvatar>(s => s.UnlockedAvatars)
                .WithOne(sc => sc.Profile)
                .HasForeignKey(sc => sc.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Profile>()
                .HasMany<ProfileStory>(p => p.ProfileStories)
                .WithOne(ps => ps.Profile)
                .HasForeignKey(ps => ps.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Page>()
                .HasMany<PageContent>(s => s.PageContents)
                .WithOne(sc => sc.Page)
                .HasForeignKey(sc => sc.PageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Language>()
                .HasMany<StoryLanguage>(s => s.StoryLanguages)
                .WithOne(sc => sc.Language)
                .HasForeignKey(sc => sc.LanguageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Language>()
                .HasMany<Attachment>(s => s.Attachments)
                .WithOne(sc => sc.Language)
                .HasForeignKey(sc => sc.LanguageId);

            modelBuilder.Entity<Language>()
                .HasMany<PageContent>(s => s.PageContents)
                .WithOne(sc => sc.Language)
                .HasForeignKey(sc => sc.LanguageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Category>()
                .HasMany<StoryCategory>(s => s.StoryCategories)
                .WithOne(sc => sc.Category)
                .HasForeignKey(sc => sc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Badge>()
                .HasMany<UnlockedBadge>(s => s.UnlockedBadges)
                .WithOne(sc => sc.Badge)
                .HasForeignKey(sc => sc.BadgeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Avatar>()
                .HasMany<UnlockedAvatar>(s => s.UnlockedAvatars)
                .WithOne(sc => sc.Avatar)
                .HasForeignKey(sc => sc.AvatarId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
