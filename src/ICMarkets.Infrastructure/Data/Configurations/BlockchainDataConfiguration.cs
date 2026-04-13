using ICMarkets.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ICMarkets.Infrastructure.Data.Configurations;

public class BlockchainDataConfiguration : IEntityTypeConfiguration<BlockchainData>
{
    public void Configure(EntityTypeBuilder<BlockchainData> builder)
    {
        builder.ToTable("blockchain_data");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Network).IsRequired().HasMaxLength(10);
        builder.Property(x => x.Chain).IsRequired().HasMaxLength(10);
        builder.Property(x => x.Hash).IsRequired().HasMaxLength(128);
        builder.Property(x => x.LatestUrl).HasMaxLength(500);
        builder.Property(x => x.PreviousHash).IsRequired().HasMaxLength(128);
        builder.Property(x => x.PreviousUrl).HasMaxLength(500);
        builder.Property(x => x.LastForkHash).IsRequired().HasMaxLength(128);

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // composite index for history lookups
        builder.HasIndex(x => new { x.Network, x.Chain, x.CreatedAt })
            .IsDescending(false, false, true);
    }
}
