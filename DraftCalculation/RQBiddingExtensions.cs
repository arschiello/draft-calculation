using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects;
using PX.Objects.RQ;


namespace PX.Objects.RQ{
public class RQBiddingExt: PXCacheExtension<PX.Objects.RQ.RQBidding>{


      
      #region UsrPatternCost

      
      [PXDBBaseCury()]
[PXDefault(TypeCode.Decimal, "0.0")]
      public virtual Decimal? UsrPatternCost{get;set;}
      public abstract class usrPatternCost : IBqlField{}

      #endregion



      
      #region UsrCuryPatternCost

      
      [PXUIField(DisplayName = "Pattern Cost")]
[PXDBCurrency(typeof(RQBidding.curyInfoID), typeof(RQBiddingExt.usrPatternCost))]
[PXDefault(TypeCode.Decimal, "0.0")]
      public virtual Decimal? UsrCuryPatternCost{get;set;}
      public abstract class usrCuryPatternCost : IBqlField{}

      #endregion




//}



      //[PXNonInstantiatedExtension]
//public class RQ_RQBidding_ExistingColumn: PXCacheExtension<PX.Objects.RQ.RQBidding>{


      
      #region CuryQuoteExtCost  
      
      [PXDBCurrency(typeof(RQBidding.curyInfoID), typeof(RQBidding.quoteExtCost))]
[PXUIField(DisplayName = "Bid Extended Cost", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
[PXFormula(typeof(Add<Mult<RQBidding.quoteQty, RQBidding.curyQuoteUnitCost>, IsNull<RQBiddingExt.usrCuryPatternCost, decimal0>>), typeof(SumCalc<RQBiddingVendor.curyTotalQuoteExtCost>))]
[PXDefault(TypeCode.Decimal, "0.0")]
      public Decimal? CuryQuoteExtCost{get;set;}

      #endregion





}




}