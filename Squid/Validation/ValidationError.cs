using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Squid.Validation
{
    [DataContract]
    public
    class ValidationError
    {
        [DataMember]
        public String Message { get; private set; }

        [DataMember]
        public String MemberName { get; private set; }

        public
        ValidationError(String message, String memberName = null)
        {
            Debug.Assert(!String.IsNullOrEmpty(message));

            MemberName = memberName;
            Message = message;
        }
    }
}