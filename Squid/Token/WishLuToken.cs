using Squid.Log;
using Squid.Users;
using System;

namespace Squid.Token
{
    public class TokenExpiredException : ApplicationExceptionBase
    {
        public
        TokenExpiredException(String message,
                                params Object[] additionalInformation) :
            base(message, additionalInformation)
        {
        }
    }

    public class UserNotAuthorizedException : ApplicationExceptionBase
    {
        public
        UserNotAuthorizedException(String message,
                                   params Object[] additionalInformation) :
            base(message, additionalInformation)
        {
        }
    }

    public class WishLuToken
    {
        // The current "thread global" token.
        [ThreadStatic]
        private static WishLuToken currentToken;

        public User User { get; private set; }

        public WishLuToken() { }

        public WishLuToken(User user)
        {
            this.User = user;
        }

        public static WishLuToken CurrentToken
        {
            get
            {
                return currentToken;
            }
        }

        public static void CreateTokenForUser(User User)
        {
            currentToken = new WishLuToken(User);
        }

        internal static void RefreshTokenInternal(Guid token, DateTime now)
        {
            User user = User.SearchForUserByToken(token);

            if (user == null)
                return;
            else if (currentToken == null || currentToken.User != user)
                CreateTokenForUser(user);

            // If the user was not found or if the token was expired, throw an exception.             //
            //                                                                                          //
            if (user == null || user.TokenExpirationTime < now)
            {
                Logger.Log("User session expired or user was not authorized.");

                String message = "Service.Token.TokenExpiredError";

                throw new TokenExpiredException(message);
            }

            user.ExtendToken(now);
        }

        public static void RefreshToken(Guid token)
        {
            RefreshTokenInternal(token, DateTime.Now);
        }

        public static void Authenticate(Guid userId)
        {
            User currentUser = CurrentToken.User;
            bool isAllowed = true;

            if (currentUser == null)
                isAllowed = false;
            if (isAllowed && currentUser.Id != userId)
                isAllowed = currentUser.IsAdminUser;

            if (!isAllowed)
            {
                String message = ("Service.Token.NotAuthorizedError");

                throw new UserNotAuthorizedException(message);
            }
        }
    }
}