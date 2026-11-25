using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project_Management.Domain.Entities;
using Project_Management.Domain.Enums;

namespace Project_Management.Infrastructure.Persistence.Configurations;

internal sealed class TareaConfiguration : IEntityTypeConfiguration<Tarea>
{
    public void Configure(EntityTypeBuilder<Tarea> builder)
    {
        builder.ToTable("Tareas");
        builder.HasKey(tarea => tarea.Id);

        builder.Property(tarea => tarea.Id)
            .ValueGeneratedOnAdd();

        builder.Property(tarea => tarea.Titulo)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(tarea => tarea.Descripcion)
            .HasMaxLength(500);

        builder.Property(tarea => tarea.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(tarea => tarea.Prioridad)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(tarea => tarea.FechaLimite)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.HasOne(tarea => tarea.Proyecto)
            .WithMany(proyecto => proyecto.Tareas)
            .HasForeignKey(tarea => tarea.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(tarea => tarea.ProyectoId);
        builder.HasIndex(tarea => new { tarea.Estado, tarea.FechaLimite });
    }
}
