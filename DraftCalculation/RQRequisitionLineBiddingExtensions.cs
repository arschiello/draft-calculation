using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.CR;
using PX.Objects;
using PX.Objects.RQ;


namespace PX.Objects.RQ{
public class RQRequisitionLineBiddingExt: PXCacheExtension<PX.Objects.RQ.RQRequisitionLineBidding>{


      
      #region UsrPatternCost

      
      [PXBaseCury()]
[PXDefault(TypeCode.Decimal, "0.0")]
      public virtual Decimal? UsrPatternCost{get;set;}
      public abstract class usrPatternCost : IBqlField{}

      #endregion



      
      #region UsrCuryPatternCost

      
      [PXCurrency(typeof(RQRequisitionLineBidding.curyInfoID), typeof(RQRequisitionLineBiddingExt.usrPatternCost))]
[PXUIField(DisplayName = "Pattern Cost")]
[PXDefault(TypeCode.Decimal, "0.0")]
      public virtual Decimal? UsrCuryPatternCost{get;set;}
      public abstract class usrCuryPatternCost : IBqlField{}

      #endregion




//}



      //[PXNonInstantiatedExtension]
//public class RQ_RQRequisitionLineBidding_ExistingColumn: PXCacheExtension<PX.Objects.RQ.RQRequisitionLineBidding>{


      
      #region CuryQuoteExtCost  
      
      [PXCurrency(typeof(RQRequisitionLineBidding.curyInfoID), typeof(RQRequisitionLineBidding.quoteExtCost))]
[PXUIField(DisplayName = "Bid Extended Cost", Visibility = PXUIVisibility.SelectorVisible)]
[PXFormula(typeof(Add<Mult<RQRequisitionLineBidding.quoteQty, RQRequisitionLineBidding.curyQuoteUnitCost>, IsNull<RQRequisitionLineBiddingExt.usrCuryPatternCost, decimal0>>))]
[PXDefault(TypeCode.Decimal, "0.0")]
      public Decimal? CuryQuoteExtCost{get;set;}

      #endregion





}




}