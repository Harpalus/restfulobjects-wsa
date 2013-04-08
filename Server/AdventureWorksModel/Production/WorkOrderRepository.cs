// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NakedObjects;
using NakedObjects.Services;

namespace AdventureWorksModel {
    [DisplayName("Work Orders")]
    public class WorkOrderRepository : AbstractFactoryAndRepository {
        #region Injected Services

        // This region should contain properties to hold references to any services required by the
        // object.  Use the 'injs' shortcut to add a new service.

        #endregion

        public WorkOrder RandomWorkOrder() {
            return Random<WorkOrder>();
        }

        public WorkOrder CreateNewWorkOrder(Product product) {
            var wo = NewTransientInstance<WorkOrder>();
            wo.Product = product;
            //MakePersistent();
            return wo;
        }

        [PageSize(20)]
        public IQueryable<Product> AutoComplete0CreateNewWorkOrder([MinLength(2)] string name) {
            return Container.Instances<Product>().Where(p => p.Name.Contains(name));
        }


        #region CurrentWorkOrders

        public IQueryable<WorkOrder> WorkOrders(Product product, bool currentOrdersOnly)
        {
            return from obj in Instances<WorkOrder>()
                                          where obj.Product.ProductID == product.ProductID &&
                                                (currentOrdersOnly == false || obj.EndDate == null)
                                          select obj;
        }

        #endregion

        #region Query Work Orders

        public IQueryable<WorkOrder> QueryWorkOrders([Optionally, TypicalLength(40)] string whereClause,
                                                [Optionally, TypicalLength(40)] string orderByClause,
                                                bool descending) {
            return DynamicQuery<WorkOrder>(whereClause, orderByClause, descending);
        }

        public virtual string ValidateQueryWorkOrders(string whereClause, string orderByClause, bool descending) {
            return ValidateDynamicQuery<WorkOrder>(whereClause, orderByClause, descending);
        }

        #endregion
    }
}