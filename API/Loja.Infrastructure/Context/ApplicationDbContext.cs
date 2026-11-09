using Loja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Loja.Infrastructure.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    
    public DbSet<Pedidos> Pedidos { get; set; }
    public DbSet<HistoricoStatus> HistoricoStatus { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Produto
        builder.Entity<Pedidos>(entity =>
        {
            entity.ToTable("Pedidos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Cliente).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Produto).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Valor).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DataCriacao)
                .IsRequired()
                .HasColumnType("timestamp without time zone");
            entity.HasMany(p => p.HistoricoStatus)
                .WithOne(hs => hs.Pedido)
                .HasForeignKey(hs => hs.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<HistoricoStatus>(entity =>
        {
            entity.Property(historico => historico.DataAlteracao)
                .HasColumnType("timestamp without time zone");
        });
    }
}