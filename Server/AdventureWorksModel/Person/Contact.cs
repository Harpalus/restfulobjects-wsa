// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using NakedObjects;
using System.ComponentModel;

namespace AdventureWorksModel
{
    [IconName("cellphone.png")]
    public class Contact : AWDomainObject
    {
        #region Title

        public override string ToString()
        {
            var t = new TitleBuilder();
            if (NameStyle)
            {
                t.Append(LastName).Append(FirstName);
            }
            else
            {
                t.Append(FirstName).Append(LastName);
            }
            return t.ToString();
        }

        #endregion

        #region Injected Services

        // This region should contain properties to hold references to any services required by the
        // object.  Use the 'injs' shortcut to add a new service.

        #endregion

        #region Life Cycle Methods

        public override void Persisting()
        {
            base.Persisting();
            CreateSaltAndHash(InitialPassword);
        }

        [NotPersisted]
        [Hidden]
        public AWDomainObject Contactee { get; set; }

        [MemberOrder(2)]
        [NotPersisted]
        public ContactType ContactType { get; set; }

        public void Persisted()
        {
            if (Contactee is Store)
            {
                var contactRole = Container.NewTransientInstance<StoreContact>();
                contactRole.Store = (Store)Contactee;
                contactRole.Contact = this;
                contactRole.ContactType = ContactType;

                Container.Persist(ref contactRole);
            }
            else if (Contactee is Vendor)
            {
                var vendorContact = Container.NewTransientInstance<VendorContact>();
                vendorContact.Vendor = (Vendor)Contactee;
                vendorContact.Contact = this;
                vendorContact.ContactType = ContactType;

                Container.Persist(ref vendorContact);
            }
        }

        public virtual bool HideContactType()
        {
            return Container.IsPersistent(this);
        }

        #endregion

        #region ID

        [Hidden]
        public virtual int ContactID { get; set; }

        #endregion

        #region Name fields

        #region NameStyle

        [MemberOrder(15), DefaultValue(false), DisplayName("Reverse name order")]
        public virtual bool NameStyle { get; set; }

        #endregion

        #region Title

        [Optionally]
        [StringLength(8)]
        [MemberOrder(11)]
        public virtual string Title { get; set; }

        #endregion

        #region FirstName

        [StringLength(50)]
        [MemberOrder(12)]
        public virtual string FirstName { get; set; }

        #endregion

        #region MiddleName

        [StringLength(50)]
        [Optionally]
        [MemberOrder(13)]
        public virtual string MiddleName { get; set; }

        #endregion

        #region LastName

        [StringLength(50)]
        [MemberOrder(14)]
        public virtual string LastName { get; set; }

        #endregion

        #region Suffix

        [Optionally]
        [StringLength(10)]
        [MemberOrder(15)]
        public virtual string Suffix { get; set; }

        #endregion

        #endregion

        #region Communication

        #region EmailAddress

        [Optionally]
        [StringLength(50)]
        [MemberOrder(20)]
        [RegEx(Validation = @"^[\-\w\.]+@[\-\w\.]+\.[A-Za-z]+$")]
        public virtual string EmailAddress { get; set; }

        #endregion

        #region EmailPromotion

        [MemberOrder(21), DefaultValue(1)]
        public virtual int EmailPromotion { get; set; }

        #endregion

        #region Phone

        [Optionally]
        [StringLength(25)]
        [MemberOrder(25)]
        //TODO: Add Mask or RegEx validation and formatting
        public virtual string Phone { get; set; }

        #endregion

        #endregion

        #region Password

        #region PasswordHash

        [Hidden]
        public virtual string PasswordHash { get; set; }

        #endregion

        #region PasswordSalt

        [Hidden]
        public virtual string PasswordSalt { get; set; }

        #endregion

        #region ChangePassword (Action)

        [Hidden(WhenTo.UntilPersisted)]
        [MemberOrder(1)]
        public void ChangePassword([DataType(DataType.Password)] string oldPassword, [DataType(DataType.Password)] string newPassword, [Named("New Password (Confirm)"), DataType(DataType.Password)] string confirm)
        {
            CreateSaltAndHash(newPassword);
        }

        internal void CreateSaltAndHash(string newPassword)
        {
            PasswordSalt = CreateRandomSalt();
            PasswordHash = Hashed(newPassword, PasswordSalt);
        }

        public virtual string ValidateChangePassword(string oldPassword, string newPassword, string confirm)
        {
            var rb = new ReasonBuilder();
            if (Hashed(oldPassword, PasswordSalt) != PasswordHash)
            {
                rb.Append("Old Password is incorrect");
            }
            if (newPassword != confirm)
            {
                rb.Append("New Password and Confirmation don't match");
            }
            if (newPassword.Length < 6)
            {
                rb.Append("New Password must be at least 6 characters");
            }
            if (newPassword == oldPassword)
            {
                rb.Append("New Password should be different from Old Password");
            }
            return rb.Reason;
        }

        #endregion

        #region InitialPassword Property

        //See Persisting method
        [NotPersisted]
        [Hidden(WhenTo.OncePersisted)]
        [MemberOrder(27)]
        [DataType(DataType.Password)]
        public string InitialPassword { get; set; }

        public void ModifyInitialPassword([DataType(DataType.Password)] string value)
        {
            InitialPassword = value;
            ChangePassword(null, value, null);
        }

        #endregion

        private static string Hashed(string password, string salt)
        {
            string saltedPassword = password + salt;
            byte[] data = Encoding.UTF8.GetBytes(saltedPassword);
            byte[] hash = SHA256.Create().ComputeHash(data);
            char[] chars = Encoding.UTF8.GetChars(hash);
            return new string(chars);
        }

        private static string CreateRandomSalt()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var output = new StringBuilder();
            for (int i = 1; i <= 7; i++)
            {
                output.Append(chars.Substring(random.Next(chars.Length - 1), 1));
            }
            output.Append("=");
            return output.ToString();
        }

        #endregion

        #region AdditionalContactInfo

        [Optionally]
        [MemberOrder(30)]
        public virtual string AdditionalContactInfo { get; set; }

        #endregion

        #region Row Guid and Modified Date

        #region rowguid

        [Hidden]
        public override Guid rowguid { get; set; }

        #endregion

        #region ModifiedDate

        [MemberOrder(99)]
        [Disabled]
        public override DateTime ModifiedDate { get; set; }

        #endregion

        #endregion

    }
}