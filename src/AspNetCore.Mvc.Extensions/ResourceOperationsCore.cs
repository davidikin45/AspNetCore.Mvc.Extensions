namespace AspNetCore.Mvc.Extensions
{
    public class ResourceCollectionsCore
    {
        public class Admin
        {
            public class Scopes
            {
                public const string Full = "full";
                public const string Create = "create";
                public const string Read = "read";
                public const string Update = "update";
                public const string Delete = "delete";
            }
        }

        public class CRUD
        {
            public class Operations
            {
                public const string Create = "create";
                public const string Read = "read";
                public const string ReadOwner = "read-if-owner";
                public const string Update = "update";
                public const string UpdateOwner = "update-if-owner";
                public const string Delete = "delete";
                public const string DeleteOwner = "delete-if-owner";
            }
        }

        public class Auth
        {
            public const string Name = "auth";

            public class Operations
            {
                public const string Register = "register";
                public const string Authenticate = "authenticate";
                public const string ForgotPassword = "forgot-password";
                public const string ResetPassword = "reset-password";
            }

            public class Scopes
            {
                public const string Register = Name + "." + Operations.Register;
                public const string Authenticate = Name + "." + Operations.Authenticate;
                public const string ForgotPassword = Name + "." + Operations.ForgotPassword;
                public const string ResetPassword = Name + "." + Operations.ResetPassword;
            }
        }
    }
}
