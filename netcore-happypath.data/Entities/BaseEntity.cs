using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Logging;
using netcore_happypath.data.Models;
using System.ComponentModel.DataAnnotations;

namespace netcore_happypath.data.Entities
{
    /// <summary>
    /// Defines the contract that all entities have
    /// </summary>
    public interface IBaseEntity<TKey>
    {
        TKey Id { get; set; }
        string CreateUser { get; set; }
        DateTime CreateDateTime { get; set; }
        string UpdateUser { get; set; }
        Nullable<DateTime> UpdateDateTime { get; set; }
        string CreateDateTimeDisplay { get; }
    }

    /// <summary>
    /// The base entity contains common properties and methods to be used by all entities in the application
    /// </summary>
    public class BaseEntity<TKey> : IBaseEntity<TKey> where TKey : struct, IEquatable<TKey>
    {
        public BaseEntity()
        {
            CreateDateTime = DateTime.Now;
            CreateUser = string.Empty;
        }
        public TKey Id { get; set; }

        public string CreateUser { get; set; }
        public DateTime CreateDateTime { get; set; }
        public string UpdateUser { get; set; }
        public Nullable<DateTime> UpdateDateTime { get; set; }

        public string CreateDateTimeDisplay
        {
            get
            {
                return this.CreateDateTime.ToString("MM/dd/yyyy hh:mm:ss tt");
            }
        }

        public static void ThrowAndLogIfNotValid<TBaseEntity>(TBaseEntity item, ILogger logger)
            where TBaseEntity : BaseEntity<TKey>
        {
            if (logger != null)
            {
                try
                {
                    ThrowIfNotValid(item);
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Validation Errors for entity type " + typeof(TBaseEntity).Name + ".");
                    throw exception;
                }
            }
        }

        public static void ThrowAndLogIfNotValid<TBaseEntity>(List<TBaseEntity> items, ILogger logger)
            where TBaseEntity : BaseEntity<TKey>
        {
            try
            {
                ThrowIfNotValid(items);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Validation Errors for entity type " + typeof(TBaseEntity).Name + ".");
                throw exception;
            }
        }

        public static void ThrowIfNotValid<TBaseEntity>(List<TBaseEntity> items)
            where TBaseEntity : BaseEntity<TKey>
        {
            List<ListValidationResult> validationResults = new List<ListValidationResult>();
            for (int index = 0; index < items.Count; index++)
            {
                List<ValidationResult> results = new List<ValidationResult>();
                ValidationContext context = new ValidationContext(items[index], null, null);
                Validator.TryValidateObject(items[index], context, results, true);

                if (results.Any())
                {
                    validationResults.Add(new ListValidationResult()
                    {
                        Index = index,
                        ValidationResultMessages = results.Select(x => x.ErrorMessage).ToList()
                    });
                }
            }

            if (validationResults.Any())
            {
                throw new Exception(string.Join("\r\n ", validationResults.Select(x => x.Index + ": " + string.Join(", ", x.ValidationResultMessages))));
            }
        }

        public static void ThrowIfNotValid<TBaseEntity>(TBaseEntity item)
            where TBaseEntity : BaseEntity<TKey>
        {
            if (item == null)
            {
                throw new Exception("Attempt to validate a null object.");
            }

            ValidationContext context = new ValidationContext(item, null, null);
            List<ValidationResult> results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(item, context, results, true))
            {
                throw new Exception(string.Join(", ", results.Select(x => x.MemberNames + ": " + x.ErrorMessage)));
            }
        }

    }
}
