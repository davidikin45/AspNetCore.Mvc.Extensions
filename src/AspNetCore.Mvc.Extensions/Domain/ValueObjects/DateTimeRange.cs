using AspNetCore.Mvc.Extensions.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.Domain.ValueObjects
{
    public class DateTimeRange : ValueObject
    {
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

        private DateTimeRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        private DateTimeRange(DateTime start, TimeSpan duration)
        {
            Start = start;
            End = start.Add(duration);
        }

        public static Result<DateTimeRange> Create(DateTime start, DateTime end)
        {
            var instance = new DateTimeRange(start, end);
            var errors = instance.Validate().ToList();
            if(errors.Count() > 0)
            {
                return Result.ObjectValidationFail<DateTimeRange>(errors);
            }

            //Validate
            return Result.Ok(instance);
        }

        public static Result<DateTimeRange> Create(DateTime start, TimeSpan duration)
        {
            var instance = new DateTimeRange(start, duration);
            var errors = instance.Validate().ToList();
            if (errors.Count() > 0)
            {
                return Result.ObjectValidationFail<DateTimeRange>(errors);
            }

            //Validate
            return Result.Ok(instance);
        }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield break;
        }

        protected DateTimeRange() { }

        public int DurationInMinutes()
        {
            return (End - Start).Minutes;
        }

        public DateTimeRange NewEnd(DateTime newEnd)
        {
            return new DateTimeRange(this.Start, newEnd);
        }

        public DateTimeRange NewDuration(TimeSpan newDuration)
        {
            return new DateTimeRange(this.Start, newDuration);
        }

        public DateTimeRange NewStart(DateTime newStart)
        {
            return new DateTimeRange(newStart, this.End);
        }

        public static DateTimeRange CreateOneDayRange(DateTime day)
        {
            return new DateTimeRange(day, day.AddDays(1));
        }

        public static DateTimeRange CreateOneWeekRange(DateTime startDay)
        {
            return new DateTimeRange(startDay, startDay.AddDays(7));
        }

        public bool Overlaps(DateTimeRange dateTimeRange)
        {
            return this.Start < dateTimeRange.End &&
                this.End > dateTimeRange.Start;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Start;
            yield return End;
        }
    }
}
