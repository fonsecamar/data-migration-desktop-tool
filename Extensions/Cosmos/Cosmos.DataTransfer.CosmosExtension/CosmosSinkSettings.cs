﻿using System.ComponentModel.DataAnnotations;
using Cosmos.DataTransfer.Interfaces;

namespace Cosmos.DataTransfer.CosmosExtension
{
    public class CosmosSinkSettings : CosmosSettingsBase, IDataExtensionSettings
    {
        public string? PartitionKeyPath { get; set; }
        public bool RecreateContainer { get; set; }
        public int BatchSize { get; set; } = 100;
        public int MaxRetryCount { get; set; } = 5;
        public int InitialRetryDurationMs { get; set; } = 200;
        public int? CreatedContainerMaxThroughput { get; set; }
        public bool UseAutoscaleForCreatedContainer { get; set; } = true;
        public bool IsServerlessAccount { get; set; } = false;
        public DataWriteMode WriteMode { get; set; } = DataWriteMode.Insert;

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            foreach (var item in base.Validate(validationContext))
            {
                yield return item;
            }

            if (RecreateContainer)
            {
                if (UseRbacAuth)
                {
                    yield return new ValidationResult("RBAC auth does not support Container creation", new[] { nameof(UseRbacAuth) });
                }

                if (string.IsNullOrWhiteSpace(PartitionKeyPath))
                {
                    yield return new ValidationResult("PartitionKeyPath must be specified when RecreateContainer is true", new[] { nameof(PartitionKeyPath) });
                }
            }
            if (!string.IsNullOrWhiteSpace(PartitionKeyPath) && !PartitionKeyPath.StartsWith("/"))
            {
                yield return new ValidationResult("PartitionKeyPath must start with /", new[] { nameof(PartitionKeyPath) });
            }

            if (string.IsNullOrWhiteSpace(PartitionKeyPath) && WriteMode is DataWriteMode.InsertStream or DataWriteMode.UpsertStream)
            {
                yield return new ValidationResult("PartitionKeyPath must be specified when WriteMode is set to InsertStream or UpsertStream", new[] { nameof(PartitionKeyPath), nameof(WriteMode) });
            }
        }
    }
}