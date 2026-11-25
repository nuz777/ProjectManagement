using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project_Management.Domain.Entities;

namespace Project_Management.Infrastructure.Persistence.Configurations;

internal sealed class ProyectoConfiguration : IEntityTypeConfiguration<Proyecto>
{
    public void Configure(EntityTypeBuilder<Proyecto> builder)
    {
        builder.ToTable("Proyectos");
        builder.HasKey(proyecto => proyecto.Id);

        builder.Property(proyecto => proyecto.Id)
            .ValueGeneratedOnAdd();

        builder.Property(proyecto => proyecto.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(proyecto => proyecto.Descripcion)
            .HasMaxLength(500);

        builder.Property(proyecto => proyecto.FechaInicio)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(proyecto => proyecto.FechaFin)
            .IsRequired()
            .HasColumnType("timestamp with time zone");
    }
}
