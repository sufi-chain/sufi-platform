using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;
using SufiChain.SufiAbp.AIManagement.Workspaces;
using SufiChain.SufiAbp.AIManagement.MCP.Entities;
using SufiChain.SufiAbp.AIManagement.AI;

namespace SufiChain.SufiAbp.AIManagement.EntityFrameworkCore;

public static class AIManagementDbContextModelCreatingExtensions
{
    public static void ConfigureAIManagement(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Workspace>(b =>
        {
            b.ToTable(AIManagementDbProperties.DbTablePrefix + "Workspaces", AIManagementDbProperties.DbSchema);
            
            b.ConfigureByConvention();
            b.ConfigureFullAuditedAggregateRoot();
            b.ConfigureMultiTenant();

            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.DefaultModel).IsRequired().HasMaxLength(256);
            b.Property(x => x.ApiKey).HasMaxLength(512);
            b.Property(x => x.ApiBaseUrl).HasMaxLength(512);
            b.Property(x => x.SystemPrompt).HasMaxLength(4096);
            b.Property(x => x.Temperature).IsRequired();
            b.Property(x => x.MaxTokens).IsRequired();
            b.Property(x => x.OpenAIApiMode).IsRequired();
            b.Property(x => x.InputCostPer1KTokens).HasPrecision(18, 8);
            b.Property(x => x.OutputCostPer1KTokens).HasPrecision(18, 8);
            b.Property(x => x.IsActive).IsRequired();

            b.HasIndex(x => x.Name).IsUnique();
            b.HasIndex(x => x.TenantId);
            
            // Configure the collection navigation using the backing field
            b.HasMany(x => x.ModelConfigurations)
                .WithOne()
                .HasForeignKey(c => c.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Tell EF Core to use the private backing field for the collection
            b.Navigation(x => x.ModelConfigurations).UsePropertyAccessMode(PropertyAccessMode.Field);
        });
        
        builder.Entity<AIModelConfiguration>(b =>
        {
            b.ToTable(AIManagementDbProperties.DbTablePrefix + "ModelConfigurations", AIManagementDbProperties.DbSchema);
            
            b.ConfigureByConvention();

            b.Property(x => x.WorkspaceId).IsRequired();
            b.Property(x => x.CapabilityType).IsRequired();
            b.Property(x => x.ModelId).IsRequired().HasMaxLength(256);
            b.Property(x => x.ApiEndpoint).HasMaxLength(512);
            b.Property(x => x.ApiKey).HasMaxLength(512);
            b.Property(x => x.IsEnabled).IsRequired();
            b.Property(x => x.Priority).IsRequired();
            b.Property(x => x.OpenAIApiMode);
            b.Property(x => x.InputCostPer1KTokens).HasPrecision(18, 8);
            b.Property(x => x.OutputCostPer1KTokens).HasPrecision(18, 8);
            b.Property(x => x.ConfigurationJson).HasMaxLength(4096);

            b.HasIndex(x => new { x.WorkspaceId, x.CapabilityType, x.Priority });
            b.HasIndex(x => x.IsEnabled);
        });
        
        builder.Entity<AIUsageLog>(b =>
        {
            b.ToTable(AIManagementDbProperties.DbTablePrefix + "UsageLogs", AIManagementDbProperties.DbSchema);
            
            b.ConfigureByConvention();
            b.ConfigureMultiTenant();

            b.Property(x => x.WorkspaceId).IsRequired();
            b.Property(x => x.CapabilityType).IsRequired();
            b.Property(x => x.ModelId).IsRequired().HasMaxLength(256);
            b.Property(x => x.Provider).IsRequired();
            b.Property(x => x.InputTokens);
            b.Property(x => x.OutputTokens);
            b.Property(x => x.TotalTokens);
            b.Property(x => x.HasTokenUsage).IsRequired();
            b.Property(x => x.UsageUnavailableReason).HasMaxLength(256);
            b.Property(x => x.EstimatedCost).HasPrecision(18, 8);
            b.Property(x => x.IsCostCalculated).IsRequired();
            b.Property(x => x.CostCalculationNote).HasMaxLength(256);
            b.Property(x => x.LatencyMs).IsRequired();
            b.Property(x => x.IsSuccess).IsRequired();
            b.Property(x => x.ErrorMessage).HasMaxLength(2048);

            // File integration properties
            b.Property(x => x.FileId);
            b.Property(x => x.FileUrl).HasMaxLength(2048);

            b.HasIndex(x => new { x.WorkspaceId, x.CreationTime });
            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => x.FileId);
        });
        
        builder.Entity<MCPServer>(b =>
        {
            b.ToTable(AIManagementDbProperties.DbTablePrefix + "MCPServers", AIManagementDbProperties.DbSchema);
            
            b.ConfigureByConvention();
            b.ConfigureFullAuditedAggregateRoot();
            b.ConfigureMultiTenant();

            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.WorkspaceId).IsRequired();
            b.Property(x => x.TransportType).IsRequired();
            b.Property(x => x.Endpoint).HasMaxLength(512);
            b.Property(x => x.Command).HasMaxLength(256);
            b.Property(x => x.ArgumentsJson).HasMaxLength(2048);
            b.Property(x => x.IsEnabled).IsRequired();
            b.Property(x => x.MetadataJson).HasMaxLength(4096);
            b.Property(x => x.LastConnectionError).HasMaxLength(1024);

            b.HasIndex(x => new { x.WorkspaceId, x.Name }).IsUnique();
            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => x.IsEnabled);
        });
    }
}
