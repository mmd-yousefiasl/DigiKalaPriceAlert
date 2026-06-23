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
            new Permission
            {
                Id = 28,
                ParentId = 2,
                Title = "جزئیات کاربر",
                UniqueName = "UserDetails"
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

            #region SentLetter
            new Permission
            {
                Id = 19,
                ParentId = 1,
                Title = "مدیریت ایمیل های ارسال شده",
                UniqueName = "ManageSentLetters"
            },
            new Permission
            {
                Id = 20,
                ParentId = 19,
                Title = "لیست ایمیل های ارسال شده",
                UniqueName = "SentLettersList"
            },
            new Permission
            {
                Id = 21,
                ParentId = 19,
                Title = "حذف ایمیل ارسال شده",
                UniqueName = "DeleteSentLetter"
            },
            new Permission
            {
                Id = 22,
                ParentId = 19,
                Title = "ارسال ایمیل جدید",
                UniqueName = "SendLetter"
            },
            #endregion

            #region PaymentData
            new Permission
            {
                Id = 23,
                ParentId = 1,
                Title = "مدیریت اطلاعات درگاه پرداخت",
                UniqueName = "ManagePaymentData"
            },
            new Permission
            {
                Id = 24,
                ParentId = 23,
                Title = "ویرایش اطلاعات درگاه پرداخت",
                UniqueName = "UpdatePaymentData"
            },
            #endregion

            #region BalanceTransaction
            new Permission
            {
                Id = 25,
                ParentId = 1,
                Title = "مدیریت تراکنش ها",
                UniqueName = "ManageBalanceTransaction"
            },
            new Permission
            {
                Id = 26,
                ParentId = 25,
                Title = "لیست تراکنش ها",
                UniqueName = "BalanceTransactionsList"
            },
            new Permission
            {
                Id = 27,
                ParentId = 25,
                Title = "افزودن تراکنش جدید",
                UniqueName = "CreateNewBalanceTransaction"
            },
            #endregion

            #region BlogCategory
            new Permission
            {
                Id = 29,
                ParentId = 1,
                Title = "مدیریت دسته بندی های مقالات",
                UniqueName = "ManageBlogCategories"
            },
            new Permission
            {
                Id = 30,
                ParentId = 29,
                Title = "لیست دسته بندی ها",
                UniqueName = "BlogCategoriesList"
            },
            new Permission
            {
                Id = 31,
                ParentId = 29,
                Title = "افزودن دسته بندی جدید",
                UniqueName = "CreateNewBlogCategory"
            },
            new Permission
            {
                Id = 32,
                ParentId = 29,
                Title = "ویرایش دسته بندی",
                UniqueName = "UpdateBlogCategory"
            },
            new Permission
            {
                Id = 33,
                ParentId = 29,
                Title = "حذف دسته بندی",
                UniqueName = "DeleteBlogCategory"
            },
            #endregion

            #region Blog
            new Permission
            {
                Id = 34,
                ParentId = 1,
                Title = "مدیریت مقاله ها",
                UniqueName = "ManageBlogs"
            },
            new Permission
            {
                Id = 35,
                ParentId = 34,
                Title = "لیست مقاله ها",
                UniqueName = "BlogsList"
            },
            new Permission
            {
                Id = 36,
                ParentId = 34,
                Title = "افزودن مقاله جدید",
                UniqueName = "CreateNewBlog"
            },
            new Permission
            {
                Id = 37,
                ParentId = 34,
                Title = "ویرایش مقاله",
                UniqueName = "UpdateBlog"
            },
            new Permission
            {
                Id = 38,
                ParentId = 34,
                Title = "حذف مقاله",
                UniqueName = "DeleteBlog"
            },
            #endregion

            #region Faq
            new Permission
            {
                Id = 39,
                ParentId = 1,
                Title = "مدیریت سوالات متداول",
                UniqueName = "ManageFaqs"
            },
            new Permission
            {
                Id = 40,
                ParentId = 39,
                Title = "لیست سوالات متداول",
                UniqueName = "FaqsList"
            },
            new Permission
            {
                Id = 41,
                ParentId = 39,
                Title = "افزودن سوال متداول جدید",
                UniqueName = "CreateNewFaq"
            },
            new Permission
            {
                Id = 42,
                ParentId = 39,
                Title = "ویرایش سوال متداول",
                UniqueName = "UpdateFaq"
            },
            new Permission
            {
                Id = 43,
                ParentId = 39,
                Title = "حذف سوال متداول",
                UniqueName = "DeleteFaq"
            },
            #endregion

            #region Media
            new Permission
            {
                Id = 44,
                ParentId = 1,
                Title = "مدیریت رسانه ها",
                UniqueName = "ManageMedia"
            },
            new Permission
            {
                Id = 45,
                ParentId = 44,
                Title = "لیست رسانه ها",
                UniqueName = "MediaList"
            },
            new Permission
            {
                Id = 46,
                ParentId = 44,
                Title = "افزودن رسانه جدید",
                UniqueName = "CreateNewMedia"
            },
            new Permission
            {
                Id = 47,
                ParentId = 44,
                Title = "حذف رسانه",
                UniqueName = "DeleteMedia"
            },
            #endregion

            #region Ticket
            new Permission
            {
                Id = 48,
                ParentId = 1,
                Title = "مدیریت تیکت های پشتیبانی",
                UniqueName = "ManageTickets"
            },
            new Permission
            {
                Id = 49,
                ParentId = 48,
                Title = "لیست تیکت های پشتیبانی",
                UniqueName = "TicketsList"
            },
            new Permission
            {
                Id = 50,
                ParentId = 48,
                Title = "جزئیات تیکت پشتیبانی",
                UniqueName = "TicketDetails"
            },
            new Permission
            {
                Id = 51,
                ParentId = 48,
                Title = "بستن تیکت پشتیبانی",
                UniqueName = "CloseTicket"
            },
            new Permission
            {
                Id = 52,
                ParentId = 48,
                Title = "تاریخچه تیکت",
                UniqueName = "TicketHistory"
            },
            new Permission
            {
                Id = 53,
                ParentId = 48,
                Title = "ویرایش وضعیت تیکت",
                UniqueName = "UpdateTicketStatus"
            },
            #endregion
        };

        //Last used id ==> 53
        public static List<Permission> Permissions = _permissions;
    }
}