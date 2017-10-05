using PX.SM;
using System;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.CM;
using PX.Objects.IN;
using PX.Objects.AR;
using PX.TM;
using PX.Objects.EP;
using PX.Objects.PO;
using PX.Objects.SO;
using CRLocation = PX.Objects.CR.Standalone.Location;
using System.Collections.Generic;
using PX.Objects;
using PX.Objects.RQ;


namespace PX.Objects.RQ{
public class RQRequisitionExt: PXCacheExtension<PX.Objects.RQ.RQRequisition>{


      
      #region UsrEngineeringCost

      
      [PXDBBaseCury()]
[PXDefault(TypeCode.Decimal, "0.0")]
      public virtual Decimal? UsrEngineeringCost{get;set;}
      public abstract class usrEngineeringCost : IBqlField{}

      #endregion



      
      #region UsrCuryEngineeringCost

      
      [PXUIField(DisplayName = "Est. Eng. Cost")]
[PXDBCurrency(typeof(RQRequisition.curyInfoID), typeof(RQRequisitionExt.usrEngineeringCost))]
[PXDefault(TypeCode.Decimal, "0.0")]
      public virtual Decimal? UsrCuryEngineeringCost{get;set;}
      public abstract class usrCuryEngineeringCost : IBqlField{}

      #endregion



      
      #region UsrShippingCost

      
      [PXDBBaseCury()]
[PXDefault(TypeCode.Decimal, "0.0")]
      public virtual Decimal? UsrShippingCost{get;set;}
      public abstract class usrShippingCost : IBqlField{}

      #endregion



      
      #region UsrCustomsClearanceCost

      
      [PXDBBaseCury()]
[PXDefault(TypeCode.Decimal, "0.0")]
      public virtual Decimal? UsrCustomsClearanceCost{get;set;}
      public abstract class usrCustomsClearanceCost : IBqlField{}

      #endregion



      
      #region UsrCuryShippingCost

      
      [PXUIField(DisplayName = "Est. Ship. Cost")]
[PXDBCurrency(typeof(RQRequisition.curyInfoID), typeof(RQRequisitionExt.usrShippingCost))]
[PXDefault(TypeCode.Decimal, "0.0")]
      public virtual Decimal? UsrCuryShippingCost{get;set;}
      public abstract class usrCuryShippingCost : IBqlField{}

      #endregion



      
      #region UsrCuryCustomsClearanceCost

      
      [PXUIField(DisplayName = "Est. Clear. Cost")]
[PXDBCurrency(typeof(RQRequisition.curyInfoID), typeof(RQRequisitionExt.usrCustomsClearanceCost))]
[PXDefault(TypeCode.Decimal, "0.0")]
      public virtual Decimal? UsrCuryCustomsClearanceCost{get;set;}
      public abstract class usrCuryCustomsClearanceCost : IBqlField{}

      #endregion




}




}