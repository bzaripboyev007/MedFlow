using MedFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedFlow.DataLayer.Repository.Persistence.Configurations;

public class TelegramLinkConfiguration : IEntityTypeConfiguration<TelegramLink>
{
    public void Configure(EntityTypeBuilder<TelegramLink> builder)
    {
        builder.HasIndex(l => l.PhoneNumber).IsUnique();
        builder.HasIndex(l => l.ChatId).IsUnique();
    }
}
