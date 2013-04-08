// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NakedObjects;

namespace AdventureWorksModel {
    [IconName("gear.png")]
    public class WorkOrder : AWDomainObject {
        #region Title

        public override string ToString() {
            var t = new TitleBuilder();
            t.Append(Product).Append(StartDate, "d", null);
            return t.ToString();
        }

        #endregion

        #region ID

        [Hidden]
        public virtual int WorkOrderID { get; set; }

        #endregion

        #region OrderQty

        [MemberOrder(20)]
        public virtual int OrderQty { get; set; }

        public virtual string ValidateOrderQty(int qty) {
            var rb = new ReasonBuilder();
            if (qty <= 0) {
                rb.Append("Order Quantity must be > 0");
            }
            return rb.Reason;
        }

        #endregion

        #region StockedQty

        [MemberOrder(22)]
        [Disabled]
        public virtual int StockedQty { get; set; }

        #endregion

        #region ScrappedQty

        [MemberOrder(24)]
        public virtual short ScrappedQty { get; set; }

        #endregion

        #region StartDate

        [MemberOrder(30)]
        [Mask("d")]
        public virtual DateTime StartDate { get; set; }

        public virtual DateTime DefaultStartDate() {
            return DateTime.Now;
        }

        #endregion

        #region EndDate

        [Optionally]
        [MemberOrder(32)]
        [Mask("d")]
        public virtual DateTime? EndDate { get; set; }

        #endregion

        #region DueDate

        [MemberOrder(34)]
        [Mask("d")]
        public virtual DateTime DueDate { get; set; }

        public virtual DateTime DefaultDueDate() {
            return DateTime.Now.AddMonths(1);
        }

        #endregion

        #region ModifiedDate

        [MemberOrder(99)]
        [Disabled]
        public override DateTime ModifiedDate { get; set; }

        #endregion

        #region Product

        [MemberOrder(10)]
        public virtual Product Product { get; set; }

        [PageSize(20)]
        public IQueryable<Product> AutoCompleteProduct([MinLength(2)] string name) {
            return Container.Instances<Product>().Where(p => p.Name.Contains(name));
        }

        #endregion

        #region ScrapReason

        [Optionally]
        [MemberOrder(26)]
        public virtual ScrapReason ScrapReason { get; set; }

        #endregion

        #region WorkOrderRoutings

        private ICollection<WorkOrderRouting> _WorkOrderRouting = new List<WorkOrderRouting>();

        [Disabled]
        public virtual ICollection<WorkOrderRouting> WorkOrderRoutings {
            get { return _WorkOrderRouting; }
            set { _WorkOrderRouting = value; }
        }

        #region AddNewRouting (Action)

        [MemberOrder(1)]
        public WorkOrderRouting AddNewRouting(Location loc) {
            var wor = Container.NewTransientInstance<WorkOrderRouting>();
            wor.WorkOrder = this;
            wor.Location = loc;
            short highestSequence = 0;
            short increment = 1;
            if (WorkOrderRoutings.Count > 0) {
                highestSequence = WorkOrderRoutings.Max(n => n.OperationSequence);
            }
            highestSequence += increment;
            wor.OperationSequence = highestSequence;
            return wor;
        }

        #endregion

        #endregion
    }
}