using Newtonsoft.Json;
using Squid.Database;
using Squid.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Squid.Wishes
{
    public class WishAssignment : GraphAssociation<Wishlu, Wish>
    {
        [JsonProperty("WishId")]
        public Guid WishId { get; set; }

        [JsonProperty("WishLuId")]
        public Guid WishLuId { get; set; }

        static WishAssignment()
        {
        }

        public WishAssignment()
        {
            Id = Guid.Empty;
        }

        //---------------------------------------------------------------------------------------------//
        // Perform General Validations.                                                                //
        //                                                                                             //
        public void PerformGeneralValidations(List<ValidationError> validationErrors)
        {
            Debug.Assert(validationErrors != null);

            validationErrors.ValidateNotNull("WishId", WishId);
            validationErrors.ValidateNotNull("WishLuId", WishLuId);
        }

        //---------------------------------------------------------------------------------------------//
        // Create Wish Assignment.                                                                     //
        //                                                                                             //
        public void CreateWishAssignment()
        {
            List<ValidationError> validationErrors = new List<ValidationError>();

            if (Id == Guid.Empty)
                Id = Guid.NewGuid();

            PerformGeneralValidations(validationErrors);
            validationErrors.ThrowValidationException();

            this.CreatedOn = DateTimeOffset.Now;
            this.LastModifiedOn = DateTimeOffset.Now;

            this.Alpha = Wishlu.GetWishLuById(WishLuId);
            this.Omega = Wish.GetWishById(WishId);

            this.Type = GraphAssociationType.CONTAINS_WISH;
            this.CreateExclusive();
        }

        //---------------------------------------------------------------------------------------------//
        // Delete Wish Assignment By Wish ID and WishLu ID.                                            //
        //                                                                                             //
        public static void DeleteByWishIdAndWishLuId(Guid wishId, Guid wishLuId)
        {
            try
            {
                Graph.Instance.Cypher
                  .OptionalMatch("(wishlu:Wishlu)-[r:CONTAINS_WISH]-(wish:Wish)")
                  .Where((Wishlu wishlu) => wishlu.Id == wishLuId)
                  .AndWhere((Wish wish) => wish.Id == wishId)
                  .Delete("r")
                  .ExecuteWithoutResults();
            }
            catch
            { }
        }

        //---------------------------------------------------------------------------------------------//
        // Delete Wish Assignment By WishLu ID.                                                        //
        //                                                                                             //
        public static int DeleteByWishLuId(Guid wishLuId)
        {
            try
            {
                int count = (int)Graph.Instance.Cypher
                   .OptionalMatch("(wishlu:Wishlu)-[r:CONTAINS_WISH]-(wish:Wish)")
                   .Where((Wishlu wishlu) => wishlu.Id == wishLuId)
                   .Return(r => r.Count())
                   .Results.First();

                Graph.Instance.Cypher
                   .OptionalMatch("(wishlu:Wishlu)-[r:CONTAINS_WISH]-(wish:Wish)")
                   .Where((Wishlu wishlu) => wishlu.Id == wishLuId)
                   .Delete("r")
                   .ExecuteWithoutResults();

                return count;
            }
            catch
            {
                return 0;
            }
        }

        public static void DeleteByWishId(Guid wishId)
        {
            try
            {
                Graph.Instance.Cypher
                   .OptionalMatch("(wishlu:Wishlu)-[r:CONTAINS_WISH]-(wish:Wish)")
                   .Where((Wish wish) => wish.Id == wishId)
                   .Delete("r")
                   .ExecuteWithoutResults();
            }
            catch
            { }
        }
    }
}