namespace ExpenseDemo.Models
{
    public class ExpensePortalConstant
    {
        public static class Resource
        {
            public static class AttributeName
            {
                public const string OwnerOrg = "OwnerOrg"; // coming from expense db
                public const string OwnerAlias = "OwnerAlias"; // coming from expense db
                public const string OwnerReportingChain = "OwnerReportingChain"; // coming from graph
                public const string Amount = "Amount"; // coming from expense db
                public const string Category = "Category"; // coming from expense db
            }
        }

        public static class Subject
        {
            public static class AttributeName
            {
                public const string Categories = "Categories"; // coming from attribute db
                public const string SafeLimit = "SafeLimit"; // coming from attribute db
                public const string Org = "Org"; // coming from attribute db
                public const string Alias = "Alias"; // coming from graph
                public const string ReportingChain = "ReportingChain"; // coming from graph
            }
        }
    }
}