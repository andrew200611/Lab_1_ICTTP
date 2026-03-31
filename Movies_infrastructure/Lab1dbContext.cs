using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Movies_domain.Model;

namespace Movies_infrastructure;

public partial class Lab1dbContext : DbContext
{
    public Lab1dbContext()
    {
    }

    public Lab1dbContext(DbContextOptions<Lab1dbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Actor> Actors { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=lab1db;Username=Andrew;Password=postgres");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Actor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Actors_pkey");

            entity.Property(e => e.Id)
                .HasColumnName("Act_id")
                .HasDefaultValueSql("nextval('\"Actors_Act_id_seq\"'::regclass)");
            entity.Property(e => e.ActName)
                .HasMaxLength(100)
                .HasColumnName("Act_name");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Genres_pkey");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('\"Genres_Genres_seq\"'::regclass)")
                .HasColumnName("Gr_id");
            entity.Property(e => e.GrName)
                .HasMaxLength(100)
                .HasColumnName("Gr_name");
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Movies_pkey");

            entity.Property(e => e.Id)
                .HasColumnName("Mv_id")
                .HasDefaultValueSql("nextval('\"Movies_Mv_id_seq\"'::regclass)");
            entity.Property(e => e.MvDescription).HasColumnName("Mv_description");
            entity.Property(e => e.MvName)
                .HasMaxLength(100)
                .HasColumnName("Mv_name");
            entity.Property(e => e.MvYear).HasColumnName("Mv_year");

            entity.HasMany(d => d.Acts).WithMany(p => p.Mvs)
                .UsingEntity<Dictionary<string, object>>(
                    "MovieActor",
                    r => r.HasOne<Actor>().WithMany()
                        .HasForeignKey("ActId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("FK_MA_Actor"),
                    l => l.HasOne<Movie>().WithMany()
                        .HasForeignKey("MvId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("FK_MA_Movie"),
                    j =>
                    {
                        j.HasKey("MvId", "ActId").HasName("Movie_actors_pkey");
                        j.ToTable("Movie_actors");
                        j.IndexerProperty<int>("MvId").HasColumnName("Mv_id");
                        j.IndexerProperty<int>("ActId").HasColumnName("Act_id");
                    });

            entity.HasMany(d => d.Grs).WithMany(p => p.Mvs)
                .UsingEntity<Dictionary<string, object>>(
                    "MovieGenre",
                    r => r.HasOne<Genre>().WithMany()
                        .HasForeignKey("GrId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("FK_Movie_Genres_Genre"),
                    l => l.HasOne<Movie>().WithMany()
                        .HasForeignKey("MvId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("FK_Movie_Genres_Movie"),
                    j =>
                    {
                        j.HasKey("MvId", "GrId").HasName("Movie_genres_pkey");
                        j.ToTable("Movie_genres");
                        j.IndexerProperty<int>("MvId").HasColumnName("Mv_id");
                        j.IndexerProperty<int>("GrId").HasColumnName("Gr_id");
                    });
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Reviews_pkey");

            entity.Property(e => e.Id)
                .HasColumnName("RW_ID")
                .HasDefaultValueSql("nextval('\"Reviews_Rw_id_seq\"'::regclass)");
            entity.Property(e => e.RwDate).HasColumnName("Rw_date");
            entity.Property(e => e.RwMovie).HasColumnName("RW_Movie");
            entity.Property(e => e.RwRate).HasColumnName("Rw_rate");
            entity.Property(e => e.RwText)
                .HasMaxLength(1000)
                .HasColumnName("Rw_text");
            entity.Property(e => e.RwUser).HasColumnName("RW_User");

            entity.HasOne(d => d.RwMovieNavigation).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.RwMovie)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Review_Movie");

            entity.HasOne(d => d.RwUserNavigation).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.RwUser)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Review_User");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Users_pkey");

            entity.Property(e => e.Id)
                .HasColumnName("Us_ID")
                .HasDefaultValueSql("nextval('\"Users_Us_id_seq\"'::regclass)");
            entity.Property(e => e.UsEmail)
                .HasMaxLength(100)
                .HasColumnName("Us_Email");
            entity.Property(e => e.UsName)
                .HasMaxLength(50)
                .HasColumnName("Us_name");
            entity.Property(e => e.UsPassword)
                .HasMaxLength(50)
                .HasColumnName("Us_password");
            entity.Property(e => e.UsRole)
                .HasMaxLength(50)
                .HasColumnName("Us_role");

            entity.HasMany(d => d.FavMovies).WithMany(p => p.FavUsers)
                .UsingEntity<Dictionary<string, object>>(
                    "Favourite",
                    r => r.HasOne<Movie>().WithMany()
                        .HasForeignKey("FavMovie")
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("FK_Fav_Movie"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("FavUser")
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("FK_Fav_User"),
                    j =>
                    {
                        j.HasKey("FavUser", "FavMovie").HasName("Favourites_pkey");
                        j.ToTable("Favourites");
                        j.IndexerProperty<int>("FavUser").HasColumnName("Fav_User");
                        j.IndexerProperty<int>("FavMovie").HasColumnName("Fav_movie");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}