﻿using System.Data.Entity.ModelConfiguration;

namespace Phony.Persistence.EntityConfigurations
{
    public class TreasuryMoveConfig : EntityTypeConfiguration<Model.TreasuryMove>
    {
        public TreasuryMoveConfig()
        {
            HasKey(t => t.Id);

            HasRequired(t => t.Treasury)
                .WithMany(t => t.TreasuriesMoves)
                .HasForeignKey(t => t.TreasuryId)
                .WillCascadeOnDelete(true);

            HasRequired(t => t.Creator)
                .WithMany()
                .HasForeignKey(t => t.CreatedById)
                .WillCascadeOnDelete(false);

            Property(t => t.EditById)
                .IsOptional();

            HasOptional(t => t.Editor)
                .WithMany()
                .HasForeignKey(t => t.EditById)
                .WillCascadeOnDelete(false);
        }
    }
}