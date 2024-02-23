using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RokoJelinicWeinerTest.Models;

namespace RokoJelinicWeinerTest.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<PartnersModel> Partners { get; set; } = default!;
        public DbSet<PoliciesModel> Policies { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PartnersModel>(p =>
            {
                p.Property<int>("Id").HasColumnType("int").ValueGeneratedOnAdd();
                p.Property<string>("FirstName").HasColumnType("nvarchar(255)");
                p.Property<string>("LastName").HasColumnType("nvarchar(255)");
                p.Property<string>("Address").HasColumnType("nvarchar(255)");
                p.Property<string>("PartnerNumber").HasColumnType("nvarchar(20)").HasMaxLength(20);
                p.Property<string?>("CroatianPIN").HasColumnType("nvarchar(11)").HasMaxLength(11);
                p.Property<int>("PartnerTypeId").HasColumnType("int").HasMaxLength(2);
                p.Property<DateTime>("CreatedAtUtc").HasColumnType("datetime");
                p.Property<string>("CreateByUser").HasColumnType("nvarchar(255)");
                p.Property<bool>("IsForeign").HasColumnType("boolean");
                p.Property<string>("ExternalCode").HasColumnType("nvarchar(20)");
                p.Property<string>("Gender").HasColumnType("nvarchar(1)");
                p.HasKey("Id");
                p.HasIndex("ExternalCode").IsUnique().HasDatabaseName("Unique_ExternalCode");
            });



            modelBuilder.Entity<PoliciesModel>(p =>
            {
                p.Property<string>("PolicyNumber").HasColumnType("nvarchar(15)");
                p.Property<decimal>("PolicyPrice").HasColumnType("decimal(18,2)");
                p.HasKey("PolicyNumber");
            });
               

            modelBuilder.Entity<PartnersPolicies>()
                .HasKey(pp => new { pp.PartnerId, pp.PolicyNumber });

            modelBuilder.Entity<PartnersPolicies>()
                .HasOne(pp => pp.Partner)
                .WithMany(p => p.PartnersPolicies)
                .HasForeignKey(pp => pp.PartnerId);

            modelBuilder.Entity<PartnersPolicies>()
                .HasOne(pp => pp.Policies)
                .WithMany(p => p.PartnersPolicies)
                .HasForeignKey(pp => pp.PolicyNumber);
        }
    }
}
