using NRLApp.Models.Obstacles;
using Xunit;

namespace NRLApp.Tests.Models.Obstacles
{
    public class ObstacleMetaValidatorTests
    {
        private readonly ObstacleMetaValidator _validator = new();

        [Fact]
        public void Validate_ReturnsError_WhenNameAndCategoryMissing()
        {
            var vm = new ObstacleMetaVm
            {
                ObstacleName = string.Empty,
                Category = string.Empty,
                HeightValue = 10
            };

            var result = _validator.Validate(vm);

            Assert.True(result.HasErrors);
            Assert.Contains(result.Errors, e => e.FieldName == nameof(vm.ObstacleName));
        }

        [Fact]
        public void Validate_ReturnsError_WhenHeightIsNegative()
        {
            var vm = new ObstacleMetaVm
            {
                ObstacleName = "Bridge",
                HeightValue = -1
            };

            var result = _validator.Validate(vm);

            Assert.True(result.HasErrors);
            Assert.Contains(result.Errors, e => e.FieldName == nameof(vm.HeightValue));
        }

        [Fact]
        public void Validate_ConvertsFeetToMetersAndLimitsHeight()
        {
            var vm = new ObstacleMetaVm
            {
                ObstacleName = "Tower",
                HeightValue = 100,
                HeightUnit = "ft"
            };

            var result = _validator.Validate(vm);

            Assert.False(result.HasErrors);
            Assert.Equal(30, result.HeightMeters);
        }

        [Fact]
        public void Validate_AddsError_WhenHeightExceedsLimit()
        {
            var vm = new ObstacleMetaVm
            {
                ObstacleName = "Tall building",
                HeightValue = 350,
                HeightUnit = "m"
            };
            var result = _validator.Validate(vm);

            Assert.True(result.HasErrors);
            Assert.Contains(result.Errors, e => e.Message.Contains("300"));
        }

        [Theory]
        [InlineData(10, "m", 10)]
        [InlineData(50, "ft", 15)]
        [InlineData(0, "Ft", 0)]
        public void ConvertToMeters_HandlesFeetConversion(double value, string unit, double expected)
        {
            var result = _validator.ConvertToMeters(value, unit);

            Assert.Equal(expected, result);
        }
    }
}
