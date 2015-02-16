using System;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace Squid.Products.Amazon
{
    class AmazonSigningBehaviorExtensionElement : BehaviorExtensionElement
    {
        public AmazonSigningBehaviorExtensionElement()
        {
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(AmazonSigningEndpointBehavior);
            }
        }

        protected override object CreateBehavior()
        {
            return new AmazonSigningEndpointBehavior(AccessKeyId, SecretKey);
        }

        [ConfigurationProperty("accessKeyId", IsRequired = true)]
        public string AccessKeyId
        {
            get { return (string)base["accessKeyId"]; }
            set { base["accessKeyId"] = value; }
        }

        [ConfigurationProperty("secretKey", IsRequired = true)]
        public string SecretKey
        {
            get { return (string)base["secretKey"]; }
            set { base["secretKey"] = value; }
        }
    }
}
