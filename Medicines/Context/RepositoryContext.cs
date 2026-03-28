using Medicines.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Context
{
    public class RepositoryContext : DbContext
    {
        public DbSet<Medicine> Medicines { get; private set; }

        public RepositoryContext(DbContextOptions<RepositoryContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Medicine>(med =>
            {
                med.HasKey(m => m.Id);
                med.Property(m => m.Name).IsRequired();
                med.Property(m => m.UserId).IsRequired();
                med.Property(m => m.ScheduledTime).IsRequired();
                med.HasIndex(m => new { m.Name })
                    .IsUnique();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
