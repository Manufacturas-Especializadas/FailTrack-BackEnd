using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FailTrack.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Lines> Lines { get; set; }

    public virtual DbSet<Machines> Machines { get; set; }

    public virtual DbSet<Maintenance> Maintenance { get; set; }

    public virtual DbSet<Status> Status { get; set; }

    public virtual DbSet<Tooling> Tooling { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lines>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Lines__3214EC07C76CE990");

            entity.Property(e => e.LineName)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("lineName");
        });

        modelBuilder.Entity<Machines>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Machines__3214EC07CB50C84A");

            entity.Property(e => e.MachineName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("machineName");

            entity.HasOne(d => d.IdLineNavigation).WithMany(p => p.Machines)
                .HasForeignKey(d => d.IdLine)
                .HasConstraintName("FK__Machines__IdLine__398D8EEE");
        });

        modelBuilder.Entity<Maintenance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Maintena__3214EC078CE01951");

            entity.Property(e => e.ApplicantName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("applicantName");
            entity.Property(e => e.ClosingDate).HasColumnName("closingDate");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.FailureSolution)
                .IsUnicode(false)
                .HasColumnName("failureSolution");
            entity.Property(e => e.FaultDescription)
                .IsUnicode(false)
                .HasColumnName("faultDescription");
            entity.Property(e => e.Responsible)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("responsible");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

            entity.HasOne(d => d.IdLineNavigation).WithMany(p => p.Maintenance)
                .HasForeignKey(d => d.IdLine)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Maintenan__IdLin__49C3F6B7");

            entity.HasOne(d => d.IdMachineNavigation).WithMany(p => p.Maintenance)
                .HasForeignKey(d => d.IdMachine)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Maintenan__IdMac__4AB81AF0");

            entity.HasOne(d => d.IdStatusNavigation).WithMany(p => p.Maintenance)
                .HasForeignKey(d => d.IdStatus)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Maintenan__IdSta__4BAC3F29");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Status__3214EC077FE40017");

            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("statusName");
        });

        modelBuilder.Entity<Tooling>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tooling__3214EC07FE235A01");

            entity.Property(e => e.ApplicantName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("applicantName");
            entity.Property(e => e.ClosingDate).HasColumnName("closingDate");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.FailureSolution)
                .IsUnicode(false)
                .HasColumnName("failureSolution");
            entity.Property(e => e.FaultDescription)
                .IsUnicode(false)
                .HasColumnName("faultDescription");
            entity.Property(e => e.Responsible)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("responsible");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

            entity.HasOne(d => d.IdLineNavigation).WithMany(p => p.Tooling)
                .HasForeignKey(d => d.IdLine)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Tooling__IdLine__5070F446");

            entity.HasOne(d => d.IdMachineNavigation).WithMany(p => p.Tooling)
                .HasForeignKey(d => d.IdMachine)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Tooling__IdMachi__5165187F");

            entity.HasOne(d => d.IdStatusNavigation).WithMany(p => p.Tooling)
                .HasForeignKey(d => d.IdStatus)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Tooling__IdStatu__52593CB8");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}