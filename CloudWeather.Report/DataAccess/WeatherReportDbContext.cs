using System;
using Microsoft.EntityFrameworkCore;

namespace CloudWeather.Report.DataAccess
{
	public class WeatherReportDbContext:DbContext
	{
		public WeatherReportDbContext()
		{
		}

        public WeatherReportDbContext(DbContextOptions options) : base(options)
        {
        }

		public DbSet<WeatherReport> WeatherReport { get; set; }

        private static void SnakeCaseIdentityTableName(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WeatherReport>(b => { b.ToTable("weather_report"); });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            SnakeCaseIdentityTableName(modelBuilder);
        }
    }
}

