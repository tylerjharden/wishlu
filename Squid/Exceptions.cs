using Squid.Validation;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Squid
{
    public class ApplicationExceptionBase : ApplicationException
    {
        private ArrayList mAdditionalInfoList = new ArrayList();
        public string AdditionalInfo
        {
            get
            {
                return this.GetAdditionalInfo(this.mAdditionalInfoList);
            }
        }

        public ApplicationExceptionBase(string Msg, params object[] AddInfLst)
            : base(Msg, ApplicationExceptionBase.GetInnerException(AddInfLst))
        {
            this.AppendAdditionalInfo(AddInfLst);
        }

        private static Exception GetInnerException(params object[] AddInfLst)
        {
            Exception Exc = null;
            if (AddInfLst.Length > 1)
            {
                Exc = (AddInfLst[0] as Exception);
            }
            return Exc;
        }

        public void AppendAdditionalInfo(params object[] AddInfLst)
        {
            if (AddInfLst == null)
            {
                return;
            }
            for (int i = 0; i < AddInfLst.Length; i++)
            {
                object Obj = AddInfLst[i];
                if (Obj is object[])
                {
                    this.AppendAdditionalInfo((object[])Obj);
                }
                else
                {
                    this.mAdditionalInfoList.Add(Obj);
                }
            }
        }

        private string GetAdditionalInfo(ArrayList AddInfLst)
        {
            string AddInfStr = "";
            foreach (object Obj in AddInfLst)
            {
                string InfStr;
                if (Obj is Exception)
                {
                    InfStr = (Obj as Exception).ToString();
                }
                else
                {
                    if (Obj is ArrayList)
                    {
                        InfStr = this.GetAdditionalInfo(AddInfLst);
                    }
                    else
                    {
                        InfStr = Obj.ToString();
                    }
                }
                if (AddInfStr.Length > 0 && InfStr.Length > 0)
                {
                    AddInfStr += "\n";
                }
                AddInfStr += InfStr;
            }
            return AddInfStr;
        }

        public string GetMessageAndAdditionalInfo()
        {
            try
            {
                return this.GetBaseException().ToString() + " Additional Info: " + this.GetAdditionalInfo(this.mAdditionalInfoList);
            }
            catch
            {
                try
                {
                    return this.GetBaseException().ToString();
                }
                catch
                {
                    return "No Exception.";
                }
            }
        }
    }
        

//================================================================================================//
// Operation Not Allowed Exception Class.                                                         //
//                                                                                                //
/// <summary>
/// <para>
///      The library throws instances of this class to in response to attempts to perform an
/// operation that is not allowed due to the current state of the associated objects or data.
/// </para>
/// </summary>
///
public
class OperationNotAllowedException : ApplicationExceptionBase
{
   //---------------------------------------------------------------------------------------------//
   // Construct an Operation Not Allowed Exception Object.                                        //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs an operation not allowed exception object with the
   /// specified message and additional information.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      Message describing the exception.
   /// </param>
   ///
   public
   OperationNotAllowedException(String message) :
      base(message)
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//------------------------------------------------------------------------------------------------//
// Item Not Found Exception Class.                                                                //
//                                                                                                //
/// <summary>
/// <para>
///      The library throws instances of this class when an operation attempts to find or update
/// a database item that is not found in the database.
/// </para>
/// </summary>
///
public
class ItemNotFoundException : ApplicationExceptionBase
{
   //---------------------------------------------------------------------------------------------//
   // Construct an Item Not Found Exception Object.                                               //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs an item not found exception object with the
   /// specified message and additional information.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      Message describing the exception.
   /// </param>
   ///
   /// <param name="additionalInformation">
   ///      List of objects specifying additional information.  For exception objects, the
   /// procedure obtains their messages as well as the messages of their inner exception objects,
   /// if any.  For all other types the procedure converts the object to a string and records the
   /// string.
   /// </param>
   ///
   public
   ItemNotFoundException(String          message,
                         params Object[] additionalInformation) :
      base(message,additionalInformation)
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//------------------------------------------------------------------------------------------------//
// Item Out-Of-Date Exception Class.                                                              //
//                                                                                                //
/// <summary>
/// <para>
///      The library throws instances of this class when an operation attempts to update a
/// database record with "stale" data, that is, with data that is not based on the most recent
/// version of the data stored in the record.
/// </para>
/// </summary>
///
public
class ItemOutOfDateException : ApplicationExceptionBase
{
   //---------------------------------------------------------------------------------------------//
   // Construct an Item Out-Of-Date Exception Object.                                             //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs an item out-of-date exception object with the
   /// specified message and additional information.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      Message describing the exception.
   /// </param>
   ///
   /// <param name="additionalInformation">
   ///      List of objects specifying additional information.  For exception objects, the
   /// procedure obtains their messages as well as the messages of their inner exception objects,
   /// if any.  For all other types the procedure converts the object to a string and records the
   /// string.
   /// </param>
   ///
   public
   ItemOutOfDateException(String          message,
                          params Object[] additionalInformation) :
      base(message,additionalInformation)
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//================================================================================================//
// Already Invited Exception.                                                                     //
//                                                                                                //
/// <summary>
/// <para>
///      The user class throws exceptions of this type when there is an attempt to invite an
/// invitee a second time.
/// </para>
/// </summary>
///
/// <remarks>
/// <para>
///      The exception's validation errors list  will include an entry for the "InviteesEmail"
/// property.
/// </para>
/// </remarks>
///
public
class AlreadyInvitedException : ValidationException
{
   //---------------------------------------------------------------------------------------------//
   // Construct an Already Invited Exception Object.                                              //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs an already invited exception object with the specified
   /// message.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      The message describing the exception.  This should be a user-friendly message in the
   /// appropriate language.
   /// </param>
   ///
   /// <param name="additionalInformation">
   ///      List of objects specifying additional information.  For exception objects, the
   /// procedure obtains their messages as well as the messages of their inner exception objects,
   /// if any.  For all other types the procedure converts the object to a string and records the
   /// string.
   /// </param>
   ///
   public
   AlreadyInvitedException(String          message,
                           params Object[] additionalInformation) :
      base(message,new List<ValidationError>() { new ValidationError(message,"InviteesEmail") },additionalInformation)
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//================================================================================================//
// Friendship Already Requested Exception.                                                        //
//                                                                                                //
/// <summary>
/// <para>
///      The user class throws exceptions of this type when there is an attempt to request
/// friendship for a pair of users that already have a friendship request.
/// </para>
/// </summary>
///
/// <remarks>
/// <para>
///      The exception's validation errors list  will include an entry for the "InvitersUserId"
/// and "InviteesUserId" properties.
/// </para>
/// </remarks>
///
public
class FriendshipAlreadyRequestedException : ValidationException
{
   //---------------------------------------------------------------------------------------------//
   // Construct an Friendship Already Requested Exception Object.                                 //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs an friendship already requested exception object with the
   /// specified message.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      The message describing the exception.  This should be a user-friendly message in the
   /// appropriate language.
   /// </param>
   ///
   /// <param name="additionalInformation">
   ///      List of objects specifying additional information.  For exception objects, the
   /// procedure obtains their messages as well as the messages of their inner exception objects,
   /// if any.  For all other types the procedure converts the object to a string and records the
   /// string.
   /// </param>
   ///
   public
   FriendshipAlreadyRequestedException(String          message,
                                       params Object[] additionalInformation) :
      base(message,new List<ValidationError>() {
                                                  new ValidationError(message,"InvitersUserId"),
                                                  new ValidationError(message,"InviteesUserId")
                                               },
           additionalInformation)
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//================================================================================================//
// Friendship Already Created Exception.                                                          //
//                                                                                                //
/// <summary>
/// <para>
///      The user class throws exceptions of this type when there is an attempt to request
/// friendship for a pair of users that already have a friendship request.
/// </para>
/// </summary>
///
/// <remarks>
/// <para>
///      The exception's validation errors list  will include an entry for the "InvitersUserId"
/// and "InviteesUserId" properties.
/// </para>
/// </remarks>
///
public
class FriendshipAlreadyCreatedException : ValidationException
{
   //---------------------------------------------------------------------------------------------//
   // Construct an Friendship Already Created Exception Object.                                   //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs an friendship already created exception object with the
   /// specified message.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      The message describing the exception.  This should be a user-friendly message in the
   /// appropriate language.
   /// </param>
   ///
   /// <param name="additionalInformation">
   ///      List of objects specifying additional information.  For exception objects, the
   /// procedure obtains their messages as well as the messages of their inner exception objects,
   /// if any.  For all other types the procedure converts the object to a string and records the
   /// string.
   /// </param>
   ///
   public
   FriendshipAlreadyCreatedException(String          message,
                                     params Object[] additionalInformation) :
      base(message,new List<ValidationError>() {
                                                  new ValidationError(message,"InvitersUserId"),
                                                  new ValidationError(message,"InviteesUserId")
                                               },
           additionalInformation)
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//================================================================================================//
// Not a Friend Exception.                                                                        //
//                                                                                                //
/// <summary>
/// <para>
///      The system throws exceptions of this type when there is an attempt to perform an
/// operation on a friend when the specified friend is not actually a friend of the specified
/// user.
/// </para>
/// </summary>
///
/// <remarks>
/// <para>
///      The exception's validation errors list  will include an entry for the "FriendUserId"
/// property.
/// </para>
/// </remarks>
///
public
class NotAFriendException : ValidationException
{
   //---------------------------------------------------------------------------------------------//
   // Construct a Not a Friend Exception Object.                                                  //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs a not a friend exception object with the specified message.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      The message describing the exception.  This should be a user-friendly message in the
   /// appropriate language.
   /// </param>
   ///
   /// <param name="additionalInformation">
   ///      List of objects specifying additional information.  For exception objects, the
   /// procedure obtains their messages as well as the messages of their inner exception objects,
   /// if any.  For all other types the procedure converts the object to a string and records the
   /// string.
   /// </param>
   ///
   public
   NotAFriendException(String          message,
                       params Object[] additionalInformation) :
      base(message,new List<ValidationError>() {
                                                  new ValidationError(message,"FriendUserId"),
                                               },
           additionalInformation)
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//================================================================================================//
// Invitation E-Mail Error Exception.                                                             //
//                                                                                                //
/// <summary>
/// <para>
///      The user class throws exceptions of this type when there is an attempt to send an
/// invitation email fails.
/// </para>
/// </summary>
///
/// <remarks>
/// <para>
///      The exception's validation errors list  will include an entry for the "InviteesEmail"
/// property.
/// </para>
/// </remarks>
///
public
class InvitationEMailErrorException : ValidationException
{
   //---------------------------------------------------------------------------------------------//
   // Construct an Invitation E-Mail Error Exception Object.                                      //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs an invitation e-mail error exception object with the specified
   /// message.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      The message describing the exception.  This should be a user-friendly message in the
   /// appropriate language.
   /// </param>
   ///
   /// <param name="additionalInformation">
   ///      List of objects specifying additional information.  For exception objects, the
   /// procedure obtains their messages as well as the messages of their inner exception objects,
   /// if any.  For all other types the procedure converts the object to a string and records the
   /// string.
   /// </param>
   ///
   public
   InvitationEMailErrorException(String          message,
                                 params Object[] additionalInformation) :
      base(message,new List<ValidationError>() { new ValidationError(message,"InviteesEmail") },additionalInformation)
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//================================================================================================//
// Login ID Already In Use Exception.                                                             //
//                                                                                                //
/// <summary>
/// <para>
///      The user class throws exceptions of this type when there is an attempt to register a
/// user or update a user with a Login ID that matches another user's login ID.
/// </para>
/// </summary>
///
/// <remarks>
/// <para>
///      The exception should contain a validation error that lists "LoginId" as the invalid
/// member".
/// </para>
/// </remarks>
///
public
class LoginIdAlreadyInUseException : ValidationException
{
   //---------------------------------------------------------------------------------------------//
   // Construct a User Already Registered Exception Object.                                       //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs a user already registered exception object with the
   /// specified message.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      The message describing the exception.  This should be a user-friendly message in the
   /// appropriate language.
   /// </param>
   ///
   public
   LoginIdAlreadyInUseException(String message) :
      base(message,new List<ValidationError>() { new ValidationError(message,"LoginId") })
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//================================================================================================//
// Login ID Not Found Exception.                                                                  //
//                                                                                                //
/// <summary>
/// <para>
///      The user class throws exceptions of this type when an attempt to find a user by login ID
/// fails.
/// </para>
/// </summary>
///
/// <remarks>
/// <para>
///      The exception should contain a validation error that lists "LoginId" as the invalid
/// member".
/// </para>
/// </remarks>
///
public
class LoginIdNotFoundException : ValidationException
{
   //---------------------------------------------------------------------------------------------//
   // Construct a Login ID Not Found Exception Object.                                            //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs a login ID not found exception object with the specified
   /// message.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      The message describing the exception.  This should be a user-friendly message in the
   /// appropriate language.
   /// </param>
   ///
   public
   LoginIdNotFoundException(String message) :
      base(message,new List<ValidationError>() { new ValidationError(message,"LoginId") })
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//================================================================================================//
// User Inactive Exception.                                                                       //
//                                                                                                //
/// <summary>
/// <para>
///      The user class throws exceptions of this type when an attempt to login fails because the
/// user is inactive.
/// </para>
/// </summary>
///
/// <remarks>
/// <para>
///      The exception should contain a validation error that lists "LoginId" as the invalid
/// member".
/// </para>
/// </remarks>
///
public
class UserInactiveException : ValidationException
{
   //---------------------------------------------------------------------------------------------//
   // Construct a User Inactive Exception Object.                                                 //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs a user inactive exception object with the specified
   /// message.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      The message describing the exception.  This should be a user-friendly message in the
   /// appropriate language.
   /// </param>
   ///
   public
   UserInactiveException(String message) :
      base(message,new List<ValidationError>() { new ValidationError(message,"LoginId") })
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//================================================================================================//
// Password Error Exception.                                                                      //
//                                                                                                //
/// <summary>
/// <para>
///      The user class throws exceptions of this type when an attempt to login fails because the
/// user is inactive.
/// </para>
/// </summary>
///
/// <remarks>
/// <para>
///      The exception should contain a validation error that lists "Password" as the invalid
/// member".
/// </para>
/// </remarks>
///
public
class PasswordErrorException : ValidationException
{
   //---------------------------------------------------------------------------------------------//
   // Construct a Password Error Exception Object.                                                //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs a password error exception object with the specified
   /// message.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      The message describing the exception.  This should be a user-friendly message in the
   /// appropriate language.
   /// </param>
   ///
   public
   PasswordErrorException(String message) :
      base(message,new List<ValidationError>() { new ValidationError(message,"Password") })
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//================================================================================================//
// Wishloop Member Already Recorded Exception.                                                    //
//                                                                                                //
/// <summary>
/// <para>
///      The user class throws exceptions of this type when there is an attempt to record a
/// wishloop member a second time.
/// </para>
/// </summary>
///
/// <remarks>
/// <para>
///      The exception's validation errors list will include entries for the "WishloopId" and
/// "UserId" properties.
/// </para>
/// </remarks>
///
public
class WishloopMemberAlreadyRecordedException : ValidationException
{
   //---------------------------------------------------------------------------------------------//
   // Construct an Wishloop Member Already Recorded Exception Object.                             //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs an wishloop member already recorded exception object with
   /// the specified message.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      The message describing the exception.  This should be a user-friendly message in the
   /// appropriate language.
   /// </param>
   ///
   /// <param name="additionalInformation">
   ///      List of objects specifying additional information.  For exception objects, the
   /// procedure obtains their messages as well as the messages of their inner exception objects,
   /// if any.  For all other types the procedure converts the object to a string and records the
   /// string.
   /// </param>
   ///
   public
   WishloopMemberAlreadyRecordedException(String          message,
                                          params Object[] additionalInformation) :
      base(message,new List<ValidationError>() {
                                                  new ValidationError(message,"WishloopId"),
                                                  new ValidationError(message,"UserId")
                                               },
           additionalInformation)
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//================================================================================================//
// Not User's Wishloop Exception.                                                                 //
//                                                                                                //
/// <summary>
/// <para>
///      The system throws exceptions of this type when there is an attempt to perform an
/// operation on wishloop fails because the specified wishloop does not belong to the specified
/// user.
/// </para>
/// </summary>
///
/// <remarks>
/// <para>
///      The exception's validation errors list  will include an entry for the "WishloopId"
/// property.
/// </para>
/// </remarks>
///
public
class NotUsersWishloopException : ValidationException
{
   //---------------------------------------------------------------------------------------------//
   // Construct a Not User's Wishloop Exception Object.                                           //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs a not user's wishloop exception object with the specified
   /// message.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      The message describing the exception.  This should be a user-friendly message in the
   /// appropriate language.
   /// </param>
   ///
   /// <param name="additionalInformation">
   ///      List of objects specifying additional information.  For exception objects, the
   /// procedure obtains their messages as well as the messages of their inner exception objects,
   /// if any.  For all other types the procedure converts the object to a string and records the
   /// string.
   /// </param>
   ///
   public
   NotUsersWishloopException(String          message,
                             params Object[] additionalInformation) :
      base(message,new List<ValidationError>() {
                                                  new ValidationError(message,"WishloopId"),
                                               },
           additionalInformation)
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//================================================================================================//
// WishLu Subscriber Already Recorded Exception.                                                  //
//                                                                                                //
/// <summary>
/// <para>
///      The user class throws exceptions of this type when there is an attempt to record a
/// wishlu subscriber a second time.
/// </para>
/// </summary>
///
/// <remarks>
/// <para>
///      The exception's validation errors list will include entries for the "WishLuId" and
/// "WishloopId" properties.
/// </para>
/// </remarks>
///
public
class WishLuSubscriberAlreadyRecordedException : ValidationException
{
   //---------------------------------------------------------------------------------------------//
   // Construct an WishLu Subscriber Already Recorded Exception Object.                           //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs an wishlu subscriber already recorded exception object with
   /// the specified message.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      The message describing the exception.  This should be a user-friendly message in the
   /// appropriate language.
   /// </param>
   ///
   /// <param name="additionalInformation">
   ///      List of objects specifying additional information.  For exception objects, the
   /// procedure obtains their messages as well as the messages of their inner exception objects,
   /// if any.  For all other types the procedure converts the object to a string and records the
   /// string.
   /// </param>
   ///
   public
   WishLuSubscriberAlreadyRecordedException(String          message,
                                            params Object[] additionalInformation) :
      base(message,new List<ValidationError>() {
                                                  new ValidationError(message,"WishLuId"),
                                                  new ValidationError(message,"WishloopId")
                                               },
           additionalInformation)
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//================================================================================================//
// Wish Assignment Already Recorded Exception.                                                    //
//                                                                                                //
/// <summary>
/// <para>
///      The user class throws exceptions of this type when there is an attempt to record a
/// wish assignment a second time.
/// </para>
/// </summary>
///
/// <remarks>
/// <para>
///      The exception's validation errors list will include entries for the "WishId" and
/// "WishLuId" properties.
/// </para>
/// </remarks>
///
public
class WishAssignmentAlreadyRecordedException : ValidationException
{
   //---------------------------------------------------------------------------------------------//
   // Construct an Wish Assignment Already Recorded Exception Object.                             //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs an wish assignment already recorded exception object with
   /// the specified message.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      The message describing the exception.  This should be a user-friendly message in the
   /// appropriate language.
   /// </param>
   ///
   /// <param name="additionalInformation">
   ///      List of objects specifying additional information.  For exception objects, the
   /// procedure obtains their messages as well as the messages of their inner exception objects,
   /// if any.  For all other types the procedure converts the object to a string and records the
   /// string.
   /// </param>
   ///
   public
   WishAssignmentAlreadyRecordedException(String          message,
                                          params Object[] additionalInformation) :
      base(message,new List<ValidationError>() {
                                                  new ValidationError(message,"WishId"  ),
                                                  new ValidationError(message,"WishLuId")
                                               },
           additionalInformation)
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//================================================================================================//
// Wish Already Pledged Exception.                                                                //
//                                                                                                //
/// <summary>
/// <para>
///      The user class throws exceptions of this type when there is an attempt to pledge a wish
/// a second time.
/// </para>
/// </summary>
///
/// <remarks>
/// <para>
///      The exception's validation errors list will include an entry for the "WishId" property.
/// </para>
/// </remarks>
///
public
class WishAlreadyPledgedException : ValidationException
{
   //---------------------------------------------------------------------------------------------//
   // Construct a Wish Already Pledged Exception Object.                                          //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs a wish already pledged exception object with the
   /// specified message.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      The message describing the exception.  This should be a user-friendly message in the
   /// appropriate language.
   /// </param>
   ///
   /// <param name="additionalInformation">
   ///      List of objects specifying additional information.  For exception objects, the
   /// procedure obtains their messages as well as the messages of their inner exception objects,
   /// if any.  For all other types the procedure converts the object to a string and records the
   /// string.
   /// </param>
   ///
   public
   WishAlreadyPledgedException(String          message,
                                  params Object[] additionalInformation) :
      base(message,new List<ValidationError>() {
                                                  new ValidationError(message,"WishId")
                                               },
           additionalInformation)
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//================================================================================================//
// Wish Already Granted Exception.                                                                //
//                                                                                                //
/// <summary>
/// <para>
///      The user class throws exceptions of this type when there is an attempt to grant a wish a
/// second time.
/// </para>
/// </summary>
///
/// <remarks>
/// <para>
///      The exception's validation errors list will include an entry for the "WishId" property.
/// </para>
/// </remarks>
///
public
class WishAlreadyGrantedException : ValidationException
{
   //---------------------------------------------------------------------------------------------//
   // Construct a Wish Already Granted Exception Object.                                          //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs a wish already granted exception object with the specified
   /// message.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      The message describing the exception.  This should be a user-friendly message in the
   /// appropriate language.
   /// </param>
   ///
   /// <param name="additionalInformation">
   ///      List of objects specifying additional information.  For exception objects, the
   /// procedure obtains their messages as well as the messages of their inner exception objects,
   /// if any.  For all other types the procedure converts the object to a string and records the
   /// string.
   /// </param>
   ///
   public
   WishAlreadyGrantedException(String          message,
                               params Object[] additionalInformation) :
      base(message,new List<ValidationError>() {
                                                  new ValidationError(message,"WishId")
                                               },
           additionalInformation)
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//================================================================================================//
// Image Format Not Supported Exception.                                                          //
//                                                                                                //
/// <summary>
/// <para>
///      The user class throws exceptions of this type when there is an attempt to upload an
/// image file in an unrecognized/unsupported format.
/// </para>
/// </summary>
///
public
class ImageFormatNotSupportedException : ApplicationExceptionBase
{
   //---------------------------------------------------------------------------------------------//
   // Construct an Image Format Not Supported Exception Object.                                   //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs an image format not supported exception object with the
   /// specified message.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      The message describing the exception.  This should be a user-friendly message in the
   /// appropriate language.
   /// </param>
   ///
   /// <param name="additionalInformation">
   ///      List of objects specifying additional information.  For exception objects, the
   /// procedure obtains their messages as well as the messages of their inner exception objects,
   /// if any.  For all other types the procedure converts the object to a string and records the
   /// string.
   /// </param>
   ///
   public
   ImageFormatNotSupportedException(String          message,
                                    params Object[] additionalInformation) :
      base(message,additionalInformation)
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//================================================================================================//
// Password Reset Link Expired Exception.                                                         //
//                                                                                                //
/// <summary>
/// <para>
///      The user class throws exceptions of this type when there is an attempt to reset a user's
/// password when the reset password Id (and the link it is embedded in) has expired.
/// </para>
/// </summary>
///
/// <remarks>
/// <para>
///      The exception's validation errors list will include an entry for the "WishId" property.
/// </para>
/// </remarks>
///
public
class PasswordResetLinkExpiredException : ApplicationExceptionBase
{
   //---------------------------------------------------------------------------------------------//
   // Construct a Password Reset Link Expired Exception Object.                                   //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs a password reset link expired exception object with the
   /// specified message.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      The message describing the exception.  This should be a user-friendly message in the
   /// appropriate language.
   /// </param>
   ///
   /// <param name="additionalInformation">
   ///      List of objects specifying additional information.  For exception objects, the
   /// procedure obtains their messages as well as the messages of their inner exception objects,
   /// if any.  For all other types the procedure converts the object to a string and records the
   /// string.
   /// </param>
   ///
   public
   PasswordResetLinkExpiredException(String          message,
                                     params Object[] additionalInformation) :
      base(message,additionalInformation)
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//------------------------------------------------------------------------------------------------//
// Pledge Not Granted Exception Class.                                                            //
//                                                                                                //
/// <summary>
/// <para>
///      The library throws instances of this class when a pledge is not in the granted status
/// when it is expected to be.
/// </para>
/// </summary>
///
public
class PledgeNotGrantedException : ApplicationExceptionBase
{
   //---------------------------------------------------------------------------------------------//
   // Construct a Pledge Not Granted Exception Object.                                            //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs a pledge not granted exception object with the specified
   /// message and additional information.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      Message describing the exception.
   /// </param>
   ///
   /// <param name="additionalInformation">
   ///      List of objects specifying additional information.  For exception objects, the
   /// procedure obtains their messages as well as the messages of their inner exception objects,
   /// if any.  For all other types the procedure converts the object to a string and records the
   /// string.
   /// </param>
   ///
   public
   PledgeNotGrantedException(String          message,
                             params Object[] additionalInformation) :
      base(message,additionalInformation)
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//------------------------------------------------------------------------------------------------//
// Pledge Granted Exception Class.                                                                //
//                                                                                                //
/// <summary>
/// <para>
///      The library throws instances of this class when an attempt to grant a wish through a
/// pledge finds that the pledge has already been granted.
/// </para>
/// </summary>
///
public
class PromiseConfirmedException : ApplicationExceptionBase
{
   //---------------------------------------------------------------------------------------------//
   // Construct a Pledge Granted Exception Object.                                                //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs a pledge granted exception object with the specified
   /// message and additional information.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      Message describing the exception.
   /// </param>
   ///
   /// <param name="additionalInformation">
   ///      List of objects specifying additional information.  For exception objects, the
   /// procedure obtains their messages as well as the messages of their inner exception objects,
   /// if any.  For all other types the procedure converts the object to a string and records the
   /// string.
   /// </param>
   ///
   public
   PromiseConfirmedException(String          message,
                          params Object[] additionalInformation) :
      base(message,additionalInformation)
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//------------------------------------------------------------------------------------------------//
// Pledge Canceled Exception Class.                                                               //
//                                                                                                //
/// <summary>
/// <para>
///      The library throws instances of this class when an attempt to grant a wish through a
/// pledge finds that the pledge has been canceled.
/// </para>
/// </summary>
///
public
class PromiseCanceledException : ApplicationExceptionBase
{
   //---------------------------------------------------------------------------------------------//
   // Construct a Pledge Canceled Exception Object.                                               //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs a pledge canceled exception object with the specified
   /// message and additional information.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      Message describing the exception.
   /// </param>
   ///
   /// <param name="additionalInformation">
   ///      List of objects specifying additional information.  For exception objects, the
   /// procedure obtains their messages as well as the messages of their inner exception objects,
   /// if any.  For all other types the procedure converts the object to a string and records the
   /// string.
   /// </param>
   ///
   public
   PromiseCanceledException(String          message,
                          params Object[] additionalInformation) :
      base(message,additionalInformation)
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//------------------------------------------------------------------------------------------------//
// Pledge Expired Exception Class.                                                                //
//                                                                                                //
/// <summary>
/// <para>
///      The library throws instances of this class when an attempt to grant a wish through a
/// pledge finds that the pledge has expired.
/// </para>
/// </summary>
///
public
class PledgeExpiredException : ApplicationExceptionBase
{
   //---------------------------------------------------------------------------------------------//
   // Construct a Pledge Expired Exception Object.                                                //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs a pledge expired exception object with the specified
   /// message and additional information.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      Message describing the exception.
   /// </param>
   ///
   /// <param name="additionalInformation">
   ///      List of objects specifying additional information.  For exception objects, the
   /// procedure obtains their messages as well as the messages of their inner exception objects,
   /// if any.  For all other types the procedure converts the object to a string and records the
   /// string.
   /// </param>
   ///
   public
   PledgeExpiredException(String          message,
                          params Object[] additionalInformation) :
      base(message,additionalInformation)
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//------------------------------------------------------------------------------------------------//
// Invalid Search String Exception Class.                                                         //
//                                                                                                //
/// <summary>
/// <para>
///      The library throws instances of this class when an attempt to perform a product search
/// fails because the search string was invalid.
/// </para>
/// </summary>
///
/// <remarks>
/// <para>
///      The exception's validation errors list  will include an entry for the "SearchString"
/// property.
/// </para>
/// </remarks>
///
public
class InvalidSearchStringException : ValidationException
{
   //---------------------------------------------------------------------------------------------//
   // Construct an Invalid Search String Exception Object.                                        //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This procedure constructs an invalid search string exception object with the
   /// specified message and additional information.
   /// </para>
   /// </summary>
   ///
   /// <param name="message">
   ///      Message describing the exception.
   /// </param>
   ///
   /// <param name="additionalInformation">
   ///      List of objects specifying additional information.  For exception objects, the
   /// procedure obtains their messages as well as the messages of their inner exception objects,
   /// if any.  For all other types the procedure converts the object to a string and records the
   /// string.
   /// </param>
   ///
   public
   InvalidSearchStringException(String          message,
                                params Object[] additionalInformation) :
      base(message,new List<ValidationError>() { new ValidationError(message,"SearchString") },additionalInformation)
   {
   }
   //---------------------------------------------------------------------------------------------//
}
//================================================================================================//
}

