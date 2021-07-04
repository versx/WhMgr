namespace WhMgr.Data.Contexts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    using WhMgr.Data.Models;
    using WhMgr.Extensions;

    public class MapDbContext : DbContext
    {
        public MapDbContext(DbContextOptions<MapDbContext> options)
            : base(options)
        {
        }

        public DbSet<Pokestop> Pokestops { get; set; }

        public DbSet<Gym> Gyms { get; set; }

        // TODO: Weather

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public static ValueConverter<T, string> CreateJsonValueConverter<T>()
        {
            return new ValueConverter<T, string>
            (
                v => v.ToJson(),
                v => v.FromJson<T>()
            );
        }

        public static ValueComparer<List<T>> CreateValueComparer<T>()
        {
            return new ValueComparer<List<T>>
            (
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()
            );
        }
    }
}