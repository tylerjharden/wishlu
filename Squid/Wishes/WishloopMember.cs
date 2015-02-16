using Squid.Database;
using Squid.Users;
using Squid.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace Squid.Wishes
{
    public class WishloopMember : GraphAssociation<Wishloop, User>
    {
        [DataMember]
        public Guid WishloopId { get; set; }

        [DataMember]
        public Guid UserId { get; set; }

        //---------------------------------------------------------------------------------------------//
        // Wishloop Member Static Constructor.                                                         //
        //                                                                                             //
        static WishloopMember()
        {
        }

        //---------------------------------------------------------------------------------------------//
        // Wishloop Member Default-Constructor.                                                        //
        //                                                                                             //
        public WishloopMember()
        {
            Id = Guid.Empty;
        }

        //---------------------------------------------------------------------------------------------//
        // Perform General Validations.                                                                //
        //                                                                                             //
        public void PerformGeneralValidations(List<ValidationError> validationErrors)
        {
            Debug.Assert(validationErrors != null);

            validationErrors.ValidateNotNull("WishloopId", WishloopId);
            validationErrors.ValidateNotNull("UserId", UserId);
        }

        //---------------------------------------------------------------------------------------------//
        // Create Wishloop Member.                                                                     //
        //                                                                                             //
        public void CreateWishloopMember()
        {
            List<ValidationError> validationErrors = new List<ValidationError>();

            if (Id == Guid.Empty)
                Id = Guid.NewGuid();

            PerformGeneralValidations(validationErrors);
            validationErrors.ThrowValidationException();

            this.Alpha = Wishloop.GetWishloopById(WishloopId);
            this.Omega = User.GetUserById(UserId);

            this.Type = GraphAssociationType.HAS_MEMBER;

            this.CreateExclusive();
        }

        //---------------------------------------------------------------------------------------------//
        // Delete Wishloop Member By Wishloop's User ID and Member's User ID.                          //
        //                                                                                             //
        public static int DeleteByWishloopOwnerUserIdAndMemberUserId(Guid wishloopOwnerUserId, Guid memberUserId)
        {
            Graph.Instance.Cypher
                .OptionalMatch("(loop:Wishloop)-[r:HAS_MEMBER]-(user:User)")
                .Where((Wishloop loop) => loop.UserId == wishloopOwnerUserId)
                .AndWhere((User user) => user.Id == memberUserId)
                .Delete("r")
                .ExecuteWithoutResults();

            return 1;
        }

        //---------------------------------------------------------------------------------------------//
        // Delete Wishloop Member By Wishloop ID and User ID.                                          //
        //                                                                                             //
        public static int DeleteByWishloopIdAndUserId(Guid wishloopId, Guid userId)
        {
            Graph.Instance.Cypher
                 .OptionalMatch("(loop:Wishloop)-[r:HAS_MEMBER]-(user:User)")
                 .Where((Wishloop loop) => loop.Id == wishloopId)
                 .AndWhere((User user) => user.Id == userId)
                 .Delete("r")
                 .ExecuteWithoutResults();

            return 1;
        }

        //---------------------------------------------------------------------------------------------//
        // Delete Wishloop Member By Wishloop ID.                                                      //
        //                                                                                             //
        public static int DeleteByWishloopId(Guid wishloopId)
        {
            int count = (int)Graph.Instance.Cypher
                 .OptionalMatch("(loop:Wishloop)-[r:HAS_MEMBER]-(user:User)")
                 .Where((Wishloop loop) => loop.Id == wishloopId)
                 .Return(r => r.Count())
                 .Results.First();

            Graph.Instance.Cypher
               .OptionalMatch("(loop:Wishloop)-[r:HAS_MEMBER]-(user:User)")
               .Where((Wishloop loop) => loop.Id == wishloopId)
               .Delete("r")
               .ExecuteWithoutResults();

            return count;
        }
    }
}