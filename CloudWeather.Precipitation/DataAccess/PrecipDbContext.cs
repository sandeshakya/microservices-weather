using System;
using Microsoft.EntityFrameworkCore;

namespace CloudWeather.Precipitation.DataAccess
{
	public class PrecipDbContext: DbContext
	{
		public PrecipDbContext() { }
		public PrecipDbContext(DbContextOptions options): base(options) { }

        public DbSet<Precipitation> Precipitation { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            SnakeCaseTableName(modelBuilder);
        }

        private static void SnakeCaseTableName(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Precipitation>(b => b.ToTable("precipitation"));
        }
    }
}

