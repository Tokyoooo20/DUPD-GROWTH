using System;

namespace DupdGrowth.Web.Models
{
    public class UserTableColumnOptions
    {
        public const string SectionName = "EntityMapping:UserColumns";

        public string Table { get; set; } = "users";

        public string Id { get; set; } = "user_id";

        public string Email { get; set; } = "email";

        public string Name { get; set; } = "firstname";

        public string ParentOfficeId { get; set; } = "parent_office_id";

        public string OfficeId { get; set; } = "office_id";

        public string PasswordHash { get; set; } = "password";

        public string CreatedAt { get; set; } = "created_at";

        public string IsApproved { get; set; } = "is_approved";
    }
}