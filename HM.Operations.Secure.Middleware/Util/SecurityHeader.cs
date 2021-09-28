using System.ServiceModel.Channels;
using System.Xml;

namespace HM.Operations.Secure.Middleware.Util
{
    public class SecurityHeader : MessageHeader
    {
        public string userName;
        public string password;

        protected override void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteStartElement("wsse", Name, Namespace);
            writer.WriteXmlnsAttribute("wsse", Namespace);
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteStartElement("wsse", "UsernameToken", Namespace);

            writer.WriteStartElement("wsse", "Username", Namespace);
            writer.WriteValue(userName);
            writer.WriteEndElement();

            writer.WriteStartElement("wsse", "Password", Namespace);
            writer.WriteValue(password);
            writer.WriteEndElement();

            writer.WriteEndElement();

        }

        public override string Name => "Security";

        public override string Namespace => "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
    }

    public static class Constants
    {
        public static string LdapGroupAdditionErrorCode = "LDAP_ADD_GRP_ERR";
        public static string LdapUserCreationErrorCode = "LDAP_ADD_USR_ERR";
        public static string LdapUserExistErrorCode = "LDAP_USR_EXIST_ERR";

        public static string OperationLdapGroupAssign = "LDAP_GRP_ASSIGN";
        public static string OperationLdapGroupRemove = "LDAP_GRP_REMOVE";
        public static string OperationLdapUserCreation = "LDAP_USR_CREATE";
    }
}
