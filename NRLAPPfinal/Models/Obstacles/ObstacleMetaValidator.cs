using System;
using System.Collections.Generic;
using NRLApp.Models.Obstacles;

namespace NRLApp.Models.Obstacles
{
    public record ValidationIssue(string FieldName, string Message);

    public class ObstacleValidationResult
    {
        public ObstacleValidationResult(double? heightMeters, IReadOnlyList<ValidationIssue> errors)
        {
            HeightMeters = heightMeters;
            Errors = errors;
        }

        public double? HeightMeters { get; }
        public IReadOnlyList<ValidationIssue> Errors { get; }
        public bool HasErrors => Errors.Count > 0;
    }

    public class ObstacleMetaValidator
    {
        public ObstacleValidationResult Validate(ObstacleMetaVm vm)
        {
            var errors = new List<ValidationIssue>();

            if (string.IsNullOrWhiteSpace(vm.ObstacleName) && string.IsNullOrWhiteSpace(vm.Category))
            {
                errors.Add(new ValidationIssue(nameof(vm.ObstacleName), "Skriv hva det er, eller velg en kategori."));
            }

            if (vm.HeightValue is null || vm.HeightValue < 0)
            {
                errors.Add(new ValidationIssue(nameof(vm.HeightValue), "Oppgi høyde."));
            }

            double? heightMeters = null;
            if (vm.HeightValue is not null && vm.HeightValue >= 0)
            {
                heightMeters = ConvertToMeters(vm.HeightValue.Value, vm.HeightUnit);

                if (heightMeters > 300)
                {
                    errors.Add(new ValidationIssue(nameof(vm.HeightValue), "Høyden kan ikke overstige 300 meter."));
                }
            }

            return new ObstacleValidationResult(heightMeters, errors);
        }

        public double ConvertToMeters(double heightValue, string? heightUnit)
        {
            var isFeet = string.Equals(heightUnit, "ft", StringComparison.OrdinalIgnoreCase);
            var meters = isFeet ? Math.Round(heightValue * 0.3048, 0) : heightValue;
            return meters;
        }
    }
}