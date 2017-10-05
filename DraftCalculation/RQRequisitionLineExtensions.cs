using System;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.CM;
using PX.Objects.TX;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PO;
using PX.Objects.SO;
using PX.Objects.AR;
using System.Collections.Generic;
using PX.Objects;
using PX.Objects.RQ;


namespace PX.Objects.RQ{
public class RQRequisitionLineExt: PXCacheExtension<PX.Objects.RQ.RQRequisitionLine>{


      
      #region UsrPatternCost

      
      [PXDBBaseCury()]
[PXDefault(TypeCode.Decimal, "0.0")]
      public virtual Decimal? UsrPatternCost{get;set;}
      public abstract class usrPatternCost : IBqlField{}

      #endregion



      
      #region UsrCuryPatternCost

      
      [PXDBCurrency(typeof(RQRequisitionLine.curyInfoID), typeof(RQRequisitionLineExt.usrPatternCost))]
[PXUIField(DisplayName = "Pattern Cost", IsReadOnly = true, Enabled = true)]
[PXDefault(TypeCode.Decimal, "0.0")]
      public virtual Decimal? UsrCuryPatternCost{get;set;}
      public abstract class usrCuryPatternCost : IBqlField{}

      #endregion



      
      #region UsrAdditionalCost

      
      [PXDBBaseCury()]
[PXDefault(TypeCode.Decimal, "0.0")]
      public virtual Decimal? UsrAdditionalCost{get;set;}
      public abstract class usrAdditionalCost : IBqlField{}

      #endregion



      
      #region UsrCuryAdditionalCost

      
      [PXDBCurrency(typeof(RQRequisitionLine.curyInfoID), typeof(RQRequisitionLineExt.usrAdditionalCost))]
[PXUIField(DisplayName = "Additional Cost", IsReadOnly= true, Enabled = true)]
[PXDefault(TypeCode.Decimal, "0.0")]
      public virtual Decimal? UsrCuryAdditionalCost{get;set;}
      public abstract class usrCuryAdditionalCost : IBqlField{}

      #endregion



      
      #region UsrIntermediateCost

      
      [PXDBBaseCury()]
[PXDefault(TypeCode.Decimal, "0.0")]
      public virtual Decimal? UsrIntermediateCost{get;set;}
      public abstract class usrIntermediateCost : IBqlField{}

      #endregion



      
      #region UsrCuryIntermediateCost

      
      [PXDBCurrency(typeof(RQRequisitionLine.curyInfoID), typeof(RQRequisitionLineExt.usrIntermediateCost))]
[PXUIField(DisplayName = "Intermediate Cost", IsReadOnly = true, Enabled = true)]
[PXDefault(TypeCode.Decimal, "0.0")]
[PXFormula(typeof(Add<Mult<RQRequisitionLine.orderQty, RQRequisitionLine.curyEstUnitCost>, IsNull<RQRequisitionLineExt.usrCuryPatternCost, decimal0>>), null)]
      public virtual Decimal? UsrCuryIntermediateCost{get;set;}
      public abstract class usrCuryIntermediateCost : IBqlField{}

      #endregion




//}



    //  [PXNonInstantiatedExtension]
//public class RQ_RQRequisitionLine_ExistingColumn: PXCacheExtension<PX.Objects.RQ.RQRequisitionLine>{


      
      #region CuryEstExtCost  
      
      [PXDBCurrency(typeof(RQRequisitionLine.curyInfoID), typeof(RQRequisitionLine.estExtCost))]
[PXUIField(DisplayName = "Est. Ext. Cost", Visibility = PXUIVisibility.SelectorVisible)]
[PXFormula(typeof(Add<Add<Mult<RQRequisitionLine.orderQty, RQRequisitionLine.curyEstUnitCost>, IsNull<RQRequisitionLineExt.usrCuryPatternCost, decimal0>>, IsNull<RQRequisitionLineExt.usrCuryAdditionalCost, decimal0>>), typeof(SumCalc<RQRequisition.curyEstExtCostTotal>))]
[PXDefault(TypeCode.Decimal, "0.0")]
      public Decimal? CuryEstExtCost{get;set;}

      #endregion
        
      #region PreventUpdate
      public abstract class preventUpdate : PX.Data.IBqlField
      {
      }
      [PXBool]
      public virtual Boolean? PreventUpdate { get; set; }
      #endregion  





}




}