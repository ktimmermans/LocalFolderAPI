using Microsoft.EntityFrameworkCore;

namespace Services.Models
{
    public class ProjectContext : DbContext
    {
        public ProjectContext(DbContextOptions<ProjectContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelbuilder)
        {
            base.OnModelCreating(modelbuilder);

            modelbuilder.Entity<GlobalConfig>().ToTable("global_config");
            modelbuilder.Entity<GlobalConfig>().Property(x => x.InstanceName).HasColumnName("instance_name").IsRequired();
            modelbuilder.Entity<GlobalConfig>().Property(x => x.PollingInterval).HasColumnName("polling_interval").IsRequired();
            modelbuilder.Entity<GlobalConfig>().HasKey(x => new { x.InstanceName });

            modelbuilder.Entity<FolderConfig>().ToTable("folder_config");
            modelbuilder.Entity<FolderConfig>().Property(x => x.FolderName).HasColumnName("folder_name").IsRequired();
            modelbuilder.Entity<FolderConfig>().Property(x => x.Path).HasColumnName("folder_path").IsRequired();
            modelbuilder.Entity<FolderConfig>().Property(x => x.ApiUrl).HasColumnName("api_url").IsRequired();
            modelbuilder.Entity<FolderConfig>().Property(x => x.MoveToFolder).HasColumnName("move_to_folder").IsRequired();
            modelbuilder.Entity<FolderConfig>().Property(x => x.Polling).HasColumnName("is_polling");
            modelbuilder.Entity<FolderConfig>().Property(x => x.PollingType).HasColumnName("polling_type");
            modelbuilder.Entity<FolderConfig>().Property(x => x.IsRecursive).HasColumnName("is_recursive");
            modelbuilder.Entity<FolderConfig>().HasKey(x => new { x.FolderName });

        }
    }
}
