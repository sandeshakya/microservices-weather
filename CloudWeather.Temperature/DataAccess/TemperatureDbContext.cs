using System;
using Microsoft.EntityFrameworkCore;

namespace CloudWeather.Temperature.DataAccess
{
	public class TemperatureDbContext:DbContext
	{
		public TemperatureDbContext() {}

		public TemperatureDbContext(DbContextOptions options):base(options) { }
		public DbSet<Temperature> Temperature { get; set; }


		private static void SnakeCaseIdentityTableName(ModelBuilder modelBuilder) {
			modelBuilder.Entity<Temperature>(b => { b.ToTable("temperature"); });
		}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
			SnakeCaseIdentityTableName(modelBuilder);
			
        }
    }

}

