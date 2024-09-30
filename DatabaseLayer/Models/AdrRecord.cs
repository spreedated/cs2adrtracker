using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DatabaseLayer.Models
{
    public sealed record AdrRecord : IValidatableObject
    {
        public enum Outcomes
        {
            Unknown,
            Loss,
            Win,
            Draw
        }

        public int Id { get; set; }
        public int Value { get; set; }
        public Outcomes Outcome { get; set; }
        public long Timestamp { get; set; }
        public DateTime DateTime => DateTime.UnixEpoch.AddSeconds(this.Timestamp).ToLocalTime();

        public bool IsValid()
        {
            return !this.Validate(null).Any();
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> results = [];

            if (this.Value == default)
            {
                results.Add(new ValidationResult("Value must be set.", [nameof(this.Value)]));
            }

            return results;
        }
    }
}