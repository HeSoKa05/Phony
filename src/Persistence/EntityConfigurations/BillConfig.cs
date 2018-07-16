﻿using Phony.Model;
using System.Data.Entity.ModelConfiguration;

namespace Phony.Persistence.EntityConfigurations
{
    public class BillConfig : EntityTypeConfiguration<Bill>
    {
        public BillConfig()
        {
            HasKey(b => b.Id);

            Property(b => b.Discount)
                .IsOptional();

            Property(b => b.TotalAfterDiscounts)
                .IsRequired();

            Property(b => b.TotalPayed)
                .IsRequired();

            HasRequired(b => b.Client)
                .WithMany(c => c.Bills)
                .HasForeignKey(b => b.ClientId)
                .WillCascadeOnDelete(false);

            HasRequired(b => b.Store)
                .WithMany(s => s.Bills)
                .HasForeignKey(b => b.StoreId)
                .WillCascadeOnDelete(false);

            HasRequired(b => b.Creator)
                .WithMany()
                .HasForeignKey(s => s.CreatedById)
                .WillCascadeOnDelete(false);

            Property(b => b.EditById)
                .IsOptional();

            HasOptional(b => b.Editor)
                .WithMany()
                .HasForeignKey(b => b.EditById)
                .WillCascadeOnDelete(false);
        }
    }
}