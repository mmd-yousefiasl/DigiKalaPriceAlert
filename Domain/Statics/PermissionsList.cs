using Domain.Models.Identity;

namespace Domain.Statics
{
    public class PermissionsList
    {
        private static readonly List<Permission> _permissions = new List<Permission>()
        {
            new Permission
            {
                Id = 1,
                ParentId = null,
                Title = "Admin Panel",
                UniqueName = "AdminPanel"
            },

            #region User
            new Permission
            {
                Id = 2,
                ParentId = 1,
                Title = "مدیریت کاربران",
                UniqueName = "ManageUsers"
            },
            new Permission
            {
                Id = 3,
                ParentId = 2,
                Title = "لیست کاربران",
                UniqueName = "UsersList"
            },
            new Permission
            {
                Id = 4,
                ParentId = 2,
                Title = "افزودن کاربر جدید",
                UniqueName = "CreateNewUser"
            },
            new Permission
            {
                Id = 5,
                ParentId = 2,
                Title = "ویرایش کاربر",
                UniqueName = "UpdateUser"
            },
            new Permission
            {
                Id = 6,
                ParentId = 2,
                Title = "حذف کاربر",
                UniqueName = "DeleteUser"
            },
            #endregion

            #region Role
            new Permission
            {
                Id = 7,
                ParentId = 1,
                Title = "مدیریت نقش ها",
                UniqueName = "ManageRoles"
            },
            new Permission
            {
                Id = 8,
                ParentId = 7,
                Title = "لیست نقش ها",
                UniqueName = "RolesList"
            },
            new Permission
            {
                Id = 9,
                ParentId = 7,
                Title = "افزودن نقش جدید",
                UniqueName = "CreateNewRole"
            },
            new Permission
            {
                Id = 10,
                ParentId = 7,
                Title = "ویرایش نقش",
                UniqueName = "UpdateRole"
            },
            new Permission
            {
                Id = 11,
                ParentId = 7,
                Title = "حذف نقش",
                UniqueName = "DeleteRole"
            },
            #endregion

            #region ContactUsForms
            new Permission
            {
                Id = 12,
                ParentId = 1,
                Title = "مدیریت فرم های تماس باما",
                UniqueName = "ManageContactUsForms"
            },
            new Permission
            {
                Id = 13,
                ParentId = 12,
                Title = "لیست فرم های تماس باما",
                UniqueName = "ContactUsFormsList"
            },
            new Permission
            {
                Id = 14,
                ParentId = 12,
                Title = "حذف فرم تماس باما",
                UniqueName = "DeleteContactUsForm"
            },
            new Permission
            {
                Id = 15,
                ParentId = 12,
                Title = "خواندن فرم تماس باما",
                UniqueName = "ReadContactUsForm"
            },
            #endregion

            #region NewsLetters
            new Permission
            {
                Id = 16,
                ParentId = 1,
                Title = "مدیریت ایمیل های خبرنامه",
                UniqueName = "ManageNewsLetters"
            },
            new Permission
            {
                Id = 17,
                ParentId = 16,
                Title = "لیست ایمیل های خبرنامه",
                UniqueName = "NewsLettersList"
            },
            new Permission
            {
                Id = 18,
                ParentId = 16,
                Title = "حذف ایمیل خبرنامه",
                UniqueName = "DeleteNewsLetters"
            },
            #endregion
        };

        //Last used id ==> 18
        public static List<Permission> Permissions = _permissions;
    }
}
