﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace StreamSurfer.Models
{
    public class PostgresDataContext : IdentityDbContext<AppUser>
    {
        public PostgresDataContext(DbContextOptions<PostgresDataContext> options)
            : base(options) { }
        public DbSet<Show> Shows { get; set; }
        public DbSet<Synonym> Synonyms { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ShowService> ShowServices { get; set; }
        public DbSet<ShowGenre> ShowGenre { get; set; }
        public DbSet<MyList> MyList { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ShowService>().HasKey(x => new { x.ShowID, x.ServiceID });

            modelBuilder.Entity<ShowService>()
            .HasOne(ss => ss.Show)
            .WithMany(s => s.ShowService)
            .HasForeignKey(ss => ss.ShowID);

            modelBuilder.Entity<ShowService>()
            .HasOne(ss => ss.Service)
            .WithMany(s => s.ShowService)
            .HasForeignKey(ss => ss.ServiceID);

            modelBuilder.Entity<ShowGenre>().HasKey(x => new { x.ShowID, x.GenreID });

            modelBuilder.Entity<ShowGenre>()
            .HasOne(sg => sg.Show)
            .WithMany(s => s.ShowGenre)
            .HasForeignKey(sg => sg.ShowID);

            modelBuilder.Entity<ShowGenre>()
            .HasOne(sg => sg.Genre)
            .WithMany(g => g.ShowGenre)
            .HasForeignKey(sg => sg.GenreID);

            modelBuilder.Entity<MyList>().HasKey(x => x.Id);

            modelBuilder.Entity<MyList>()
                .Property(ml => ml.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<MyListShows>().HasKey(x => new { x.MyListId, x.ShowId });

            modelBuilder.Entity<MyListShows>()
                .HasOne(mls => mls.MyList)
                .WithMany(s => s.MyListShows)
                .HasForeignKey(mls => mls.MyListId);

            modelBuilder.Entity<MyListShows>()
                .HasOne(s => s.Show)
                .WithMany(s => s.MyListShows)
                .HasForeignKey(s => s.ShowId);
        }
    }
}
