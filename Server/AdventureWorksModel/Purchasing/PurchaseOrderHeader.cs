// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NakedObjects;

namespace AdventureWorksModel {
    [IconName("memo.png")]
    public class PurchaseOrderHeader : AWDomainObject {

        #region Life Cycle methods

        public void Created() {
            RevisionNumber = 0;
            Status = 1;
            OrderDate = DateTime.Today.Date;
        }

        public override void Updating() {
            base.Updating();
            byte increment = 1;
            RevisionNumber += increment;
        }

        #endregion

        #region Injected Services

        #region Injected: EmployeeRepository

        public EmployeeRepository EmployeeRepository { set; protected get; }

        #endregion

        #endregion

        #region ID

        [Hidden]
        public virtual int PurchaseOrderID { get; set; }

        #endregion

        #region Revision Number

        [Disabled]
        [MemberOrder(90)]
        public virtual byte RevisionNumber { get; set; }

        #endregion

        #region Status

        private static readonly string[] statusLabels = {"Pending", "Approved", "Rejected", "Complete"};

        [Hidden]
        [MemberOrder(10)]
        public virtual byte Status { get; set; }

        [DisplayName("Status")]
        [MemberOrder(1.1)]
        public virtual string StatusAsString {
            get { return statusLabels[Status - 1]; }
        }

        [Hidden]
        public virtual bool IsPending() {
            return Status == 1;
        }

        #endregion

        #region Dates

        [Title]
        [Mask("d")]
        [MemberOrder(11)]
        public virtual DateTime OrderDate { get; set; }

        [Optionally]
        [Mask("d")]
        [MemberOrder(20)]
        public virtual DateTime? ShipDate { get; set; }

        #endregion

        #region Amounts

        [MemberOrder(31)]
        [Disabled]
        [Mask("C")]
        public virtual decimal SubTotal { get; set; }

        [Disabled]
        [MemberOrder(32)]
        [Mask("C")]
        public virtual decimal TaxAmt { get; set; }

        [Disabled]
        [MemberOrder(33)]
        [Mask("C")]
        public virtual decimal Freight { get; set; }

        [Disabled]
        [MemberOrder(34)]
        [Mask("C")]
        public virtual decimal TotalDue { get; set; }

        #endregion

        #region ModifiedDate

        [MemberOrder(99)]
        [Disabled]
        public override DateTime ModifiedDate { get; set; }

        #endregion

        #region Order Placed By (Employee)

        [MemberOrder(12)]
        public virtual Employee OrderPlacedBy { get; set; }

        public Employee DefaultEmployee() {
            return EmployeeRepository.CurrentUserAsEmployee();
        }

        #endregion

        #region Details

        private ICollection<PurchaseOrderDetail> _details = new List<PurchaseOrderDetail>();

        [Disabled]
        public virtual ICollection<PurchaseOrderDetail> Details {
            get { return _details; }
            set { _details = value; }
        }

        #endregion

        #region ShipMethod

        [MemberOrder(22)]
        public virtual ShipMethod ShipMethod { get; set; }

        #endregion

        #region Vendor

        [Disabled]
        [MemberOrder(1)]
        public virtual Vendor Vendor { get; set; }

        #endregion

        #region CreateNewOrderDetail (Action)

        [MemberOrder(1)]
        public PurchaseOrderDetail AddNewDetail(Product prod, short qty) {
            var pod = Container.NewTransientInstance<PurchaseOrderDetail>();
            pod.PurchaseOrderHeader = this;
            pod.Product = prod;
            pod.OrderQty = qty;
            return pod;
        }

        public virtual string DisableAddNewDetail() {
            if (!IsPending()) {
                return "Cannot add to Purchase Order unless status is Pending";
            }
            return null;
        }

        public List<Product> Choices0AddNewDetail() {
            return Vendor.Products.Select(n => n.Product).ToList();
        }

        #endregion

        #region Approve (Action)

        [MemberOrder(1)]
        public void Approve() {
            Status = 2;
        }

        public virtual bool HideApprove() {
            return !IsPending();
        }

        public virtual string DisableApprove() {
            var rb = new ReasonBuilder();
            if (Details.Count < 1) {
                rb.Append("Purchase Order must have at least one Detail to be approved");
            }
            return rb.Reason;
        }

        #endregion
    }
}