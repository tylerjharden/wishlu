// DEPRECATED AND SAVED
using Squid.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Squid.Users
{
    [DataContract]
    [Obsolete("Tornado code is no longer used.")]
    public class Invitation
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid InvitersUserId { get; set; }

        [DataMember]
        public String InvitersName { get; set; }

        [DataMember]
        public String InviteesEMail { get; set; }

        [DataMember]
        public String InviteesName { get; set; }

        public String LanguageId { get; set; }

        [DataMember]
        public String PersonalMessage { get; set; }

        static Invitation() { }

        public
        Invitation()
        {
            Id = Guid.Empty;
        }

        public void PerformGeneralValidations(List<ValidationError> validationErrors)
        {
            Debug.Assert(validationErrors != null);

            validationErrors.ValidateMaxLength("InvitersName", InvitersName, 50);
            validationErrors.ValidateMaxLength("InviteesEMail", InviteesEMail, 50);
            validationErrors.ValidateMaxLength("InviteesName", InviteesName, 50);
            validationErrors.ValidateMaxLength("LanguageId", LanguageId, 10);
            validationErrors.ValidateMaxLength("PersonalMessage", PersonalMessage, 1000);
            validationErrors.ValidateNotNull("InvitersUserId", InvitersUserId);
            validationErrors.ValidateNotNull("InvitersName", InvitersName);
            validationErrors.ValidateNotNull("InviteesEMail", InviteesEMail);
            validationErrors.ValidateNotNull("InviteesName", InviteesName);
            validationErrors.ValidateNotNull("LanguageId", LanguageId);
        }

        public void CreateInvitation()
        {
            List<ValidationError> validationErrors = new List<ValidationError>();

            if (Id == Guid.Empty)
                Id = Guid.NewGuid();

            PerformGeneralValidations(validationErrors);
            validationErrors.ThrowValidationException();
            //RecordHelper.InsertRecord(databaseInstance,this);
        }

        public static List<Invitation> GetInvitationsByInviteesEMail(String inviteesEMail)
        {
            Debug.Assert(!String.IsNullOrEmpty(inviteesEMail));

            //return RecordHelper.SearchForRecordsWhere(databaseInstance,
            //                                          "Inv_InviteesEMail = @InviteesEMail",null,
            //                                          DatabaseHelper.CreateParameter("@InviteesEMail",inviteesEMail));

            return null;
        }
    }
}